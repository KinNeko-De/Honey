using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using HoneyLibrary.PackageLists;
using HoneyLibraryTest.PackageLists.TestData;
using NUnit.Framework;

namespace HoneyLibraryTest.PackageLists
{
	/// <summary>
	/// SystemUnderTest is defined as PackageListRepository, PackageList and PackageInfo
	/// Tests with real files
	/// </summary>
	public class PackageListRepositoryTest
	{
		private string testPath;

		[SetUp]
		public void CreateTestDirectory()
		{
			testPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			Directory.CreateDirectory(testPath);
		}

		[TearDown]
		public void DeleteTestDirectory()
		{
			Directory.Delete(testPath, true);
		}

		#region TestData
		private PackageListRepository CreateSystemUnderTest(IReadOnlyCollection<IPackageListMigration> packageListMigrations = null)
		{
			return new PackageListRepository(new PathInstallLocation(testPath), packageListMigrations);
		}

		private void CreatePackageList(string content)
		{
			File.WriteAllText(TestPackageListFileName(), content);
		}

		private XDocument ReadPackageList()
		{
			return XDocument.Load(TestPackageListFileName());
		}

		private string TestPackageListFileName()
		{
			return Path.Combine(testPath, PackageListRepository.PackageListFileName);
		}
		
		private PackageBuilder CreateLockedTestPackage(string inputPackageId = "mytestpackage")
		{
			var inputPackageVersion = new Version(1, 2, 3, 4).ToString();
			var inputLockedByAction = "TestAction";
			var inputLockedByProcess = "1234";
			var inputCreated = new DateTimeOffset(2019, 1, 1, 1, 1, 1, TimeSpan.FromHours(2));
			var inputLastUpdated = new DateTimeOffset(2019, 1, 2, 1, 1, 1, TimeSpan.FromHours(2));

			var package = new PackageBuilder()
				.SetPackageId(inputPackageId)
				.SetPackageVersion(inputPackageVersion)
				.SetLockedByAction(inputLockedByAction)
				.SetLockedByProcess(inputLockedByProcess)
				.SetCreated(inputCreated)
				.SetLastUpdated(inputLastUpdated);

			return package;
		}

		private PackageBuilder CreateNonLockedTestPackage()
		{
			var inputPackageId = "mytestpackage";
			var inputPackageVersion = new Version(1, 2, 3, 4).ToString();
			var inputCreated = new DateTimeOffset(2019, 1, 1, 1, 1, 1, TimeSpan.FromHours(2));

			var package = new PackageBuilder()
				.SetPackageId(inputPackageId)
				.SetPackageVersion(inputPackageVersion)
				.SetCreated(inputCreated);

			return package;
		}

		private class AddVersionAttributeIfNotExists : IPackageListMigration
		{
			public void Migrate(XDocument packageList)
			{
				string packageListVersion = GetPackageListVersion(packageList);

				switch (packageListVersion)
				{
					case null:
						// if the version attribute is not set it will be added
						packageList.Root.SetAttributeValue(PackageList.XmlRootVersionAttribute, PackageList.currentVersion);
						break;
					default:
						break;
				}
			}

			private static string GetPackageListVersion(XDocument packageList)
			{
				var versionAttribute = packageList.Root.Attribute(PackageList.XmlRootVersionAttribute);

				if (versionAttribute == null)
				{
					return null;
				}

				return versionAttribute.Value;
			}
		}
		#endregion TestData

		#region GetPackageInfo GetSinglePackageInfo
		[Test]
		public void GetPackageInfo_NoPackageListExists()
		{
			var sut = CreateSystemUnderTest();

			var packageInfo = sut.GetPackageInfo("mytestpackage", ListMode.LimitOutput, MatchMode.IdExact);

			Assert.That(packageInfo, Is.Null);
		}

		[Test]
		public void GetSinglePackageInfo_NoPackageListExists()
		{
			var sut = CreateSystemUnderTest();

			var packageInfo = sut.GetSinglePackageInfo("mytestpackage");

			Assert.That(packageInfo, Is.Null);
		}

		[Test]
		public void GetSinglePackageInfo_PackageListExists_ListModeLimitOutput()
		{
			var package = CreateLockedTestPackage();
			var packages = new PackagesBuilder().AddPackage(package).Build();
			CreatePackageList(packages);

			var sut = CreateSystemUnderTest();
			var packageInfo = sut.GetSinglePackageInfo(package.PackageId, ListMode.LimitOutput);

			Assert.That(packageInfo, Is.Not.Null);
			Assert.That(packageInfo.PackageId, Is.EqualTo(package.PackageId));
			Assert.That(packageInfo.PackageVersion, Is.EqualTo(package.PackageVersion));
			Assert.That(packageInfo.LockedByAction, Is.Null);
			Assert.That(packageInfo.LockedByProcess, Is.Null);
			Assert.That(packageInfo.Created, Is.Null);
			Assert.That(packageInfo.LastUpdated, Is.Null);
		}

		[Test]
		public void GetSinglePackageInfo_CaseInsensitive()
		{
			var nameInXml = "mYtESTpaCKage";
			var searchId = "myTestPackage";

			var package = CreateLockedTestPackage(inputPackageId: nameInXml);
			var packages = new PackagesBuilder().AddPackage(package).Build();
			CreatePackageList(packages);

			var sut = CreateSystemUnderTest();
			var packageInfo = sut.GetSinglePackageInfo(searchId, ListMode.LimitOutput);

			Assert.That(packageInfo, Is.Not.Null);
			Assert.That(packageInfo.PackageId, Is.EqualTo(nameInXml));
			Assert.That(packageInfo.PackageVersion, Is.EqualTo(package.PackageVersion));
			Assert.That(packageInfo.LockedByAction, Is.Null);
			Assert.That(packageInfo.LockedByProcess, Is.Null);
			Assert.That(packageInfo.Created, Is.Null);
			Assert.That(packageInfo.LastUpdated, Is.Null);
		}

		[Test]
		public void GetPackageInfo_PackageListExists_ListModeLimitOutput()
		{
			ListMode listMode = ListMode.LimitOutput;

			var package = CreateLockedTestPackage();
			var packages = new PackagesBuilder().AddPackage(package).Build();
			CreatePackageList(packages);

			var sut = CreateSystemUnderTest();
			var packageInfos = sut.GetPackageInfo(package.PackageId, listMode, MatchMode.IdExact);

			Assert.That(packageInfos, Is.Not.Null);
			Assert.That(packageInfos.Count, Is.EqualTo(1));
			var packageInfo = packageInfos.ElementAt(0);
			Assert.That(packageInfo.PackageId, Is.EqualTo(package.PackageId));
			Assert.That(packageInfo.PackageVersion, Is.EqualTo(package.PackageVersion));
			Assert.That(packageInfo.LockedByAction, Is.Null);
			Assert.That(packageInfo.LockedByProcess, Is.Null);
			Assert.That(packageInfo.Created, Is.Null);
			Assert.That(packageInfo.LastUpdated, Is.Null);
		}

		[Test]
		public void GetSinglePackageInfo_PackageListExists_ListModeFull()
		{
			var package = CreateLockedTestPackage();
			var packages = new PackagesBuilder().AddPackage(package).Build();
			CreatePackageList(packages);

			var sut = CreateSystemUnderTest();
			var packageInfo = sut.GetSinglePackageInfo(package.PackageId, ListMode.Full);

			Assert.That(packageInfo, Is.Not.Null);
			Assert.That(packageInfo.PackageId, Is.EqualTo(package.PackageId));
			Assert.That(packageInfo.PackageVersion, Is.EqualTo(package.PackageVersion));
			Assert.That(packageInfo.LockedByAction, Is.EqualTo(package.PackageLockedByAction));
			Assert.That(packageInfo.LockedByProcess, Is.EqualTo(package.PackageLockedByProcess));
			Assert.That(packageInfo.Created, Is.EqualTo(package.PackageCreated));
			Assert.That(packageInfo.LastUpdated, Is.EqualTo(package.PackageLastUpdated));
		}

		[Test]
		public void GetPackageInfo_PackageListExists_ListModeFull()
		{
			ListMode listMode = ListMode.Full;

			var package = CreateLockedTestPackage();
			var packages = new PackagesBuilder().AddPackage(package).Build();
			CreatePackageList(packages);

			var sut = CreateSystemUnderTest();
			var packageInfos = sut.GetPackageInfo(package.PackageId, listMode, MatchMode.IdExact);

			Assert.That(packageInfos, Is.Not.Null);
			Assert.That(packageInfos.Count, Is.EqualTo(1));
			var packageInfo = packageInfos.ElementAt(0);
			Assert.That(packageInfo.PackageId, Is.EqualTo(package.PackageId));
			Assert.That(packageInfo.PackageVersion, Is.EqualTo(package.PackageVersion));
			Assert.That(packageInfo.LockedByAction, Is.EqualTo(package.PackageLockedByAction));
			Assert.That(packageInfo.LockedByProcess, Is.EqualTo(package.PackageLockedByProcess));
			Assert.That(packageInfo.Created, Is.EqualTo(package.PackageCreated));
			Assert.That(packageInfo.LastUpdated, Is.EqualTo(package.PackageLastUpdated));
		}

		#region Matchmode
		[Test]
		public void GetSinglePackageInfo_PackageListExists_MatchModeIdExact_NotMatchExact()
		{
			string packageId = "mytestPackage";
			string searchString = packageId + " ";

			var package = CreateLockedTestPackage(packageId);
			var packages = new PackagesBuilder().AddPackage(package).Build();
			CreatePackageList(packages);

			var sut = CreateSystemUnderTest();
			var packageInfo = sut.GetSinglePackageInfo(searchString);

			Assert.That(packageInfo, Is.Null);
		}

		[Test]
		public void GetPackageInfo_PackageListExists_MatchModeIdExact_NotMatchExact()
		{
			MatchMode matchMode = MatchMode.IdExact;
			string packageId = "mytestPackage";
			string searchString = packageId + " ";

			var package = CreateLockedTestPackage(packageId);
			var packages = new PackagesBuilder().AddPackage(package).Build();
			CreatePackageList(packages);

			var sut = CreateSystemUnderTest();
			var packageInfo = sut.GetPackageInfo(searchString, ListMode.LimitOutput, matchMode);

			Assert.That(packageInfo, Is.Empty);
		}

		[Test]
		public void GetSinglePackageInfo_PackageListExists_MatchModeIdExact_MatchExact()
		{
			string packageId = "mytestPackage";
			string searchString = packageId;

			var package = CreateLockedTestPackage(packageId);
			var packages = new PackagesBuilder().AddPackage(package).Build();
			CreatePackageList(packages);

			var sut = CreateSystemUnderTest();
			var packageInfo = sut.GetSinglePackageInfo(searchString);

			Assert.That(packageInfo, Is.Not.Null);
			Assert.That(packageInfo.PackageId, Is.EqualTo(package.PackageId));
			Assert.That(packageInfo.PackageVersion, Is.EqualTo(package.PackageVersion));
			Assert.That(packageInfo.LockedByAction, Is.Null);
			Assert.That(packageInfo.LockedByProcess, Is.Null);
			Assert.That(packageInfo.Created, Is.Null);
			Assert.That(packageInfo.LastUpdated, Is.Null);
		}

		[Test]
		public void GetPackageInfo_PackageListExists_MatchModeIdExact_MatchExact()
		{
			MatchMode matchMode = MatchMode.IdExact;
			string packageId = "mytestPackage";
			string searchString = packageId;

			var package = CreateLockedTestPackage(packageId);
			var packages = new PackagesBuilder().AddPackage(package).Build();
			CreatePackageList(packages);

			var sut = CreateSystemUnderTest();
			var packageInfos = sut.GetPackageInfo(searchString, ListMode.LimitOutput, matchMode);

			Assert.That(packageInfos, Is.Not.Null);
			Assert.That(packageInfos.Count, Is.EqualTo(1));
			var packageInfo = packageInfos.ElementAt(0);
			Assert.That(packageInfo.PackageId, Is.EqualTo(package.PackageId));
			Assert.That(packageInfo.PackageVersion, Is.EqualTo(package.PackageVersion));
			Assert.That(packageInfo.LockedByAction, Is.Null);
			Assert.That(packageInfo.LockedByProcess, Is.Null);
			Assert.That(packageInfo.Created, Is.Null);
			Assert.That(packageInfo.LastUpdated, Is.Null);
		}

		[Test]
		public void GetPackageInfo_MatchModeIdExact_CaseInsensitive()
		{
			MatchMode matchMode = MatchMode.IdExact;
			var nameInXml = "mYtESTpaCKage";
			var searchId = "myTestPackage";

			var package = CreateLockedTestPackage(inputPackageId: nameInXml);
			var packages = new PackagesBuilder().AddPackage(package).Build();
			CreatePackageList(packages);

			var sut = CreateSystemUnderTest();
			var packageInfos = sut.GetPackageInfo(searchId, ListMode.LimitOutput, matchMode);

			Assert.That(packageInfos, Is.Not.Null);
			Assert.That(packageInfos.Count, Is.EqualTo(1));
			var packageInfo = packageInfos.ElementAt(0);
			Assert.That(packageInfo.PackageId, Is.EqualTo(package.PackageId));
			Assert.That(packageInfo.PackageVersion, Is.EqualTo(package.PackageVersion));
			Assert.That(packageInfo.LockedByAction, Is.Null);
			Assert.That(packageInfo.LockedByProcess, Is.Null);
			Assert.That(packageInfo.Created, Is.Null);
			Assert.That(packageInfo.LastUpdated, Is.Null);
		}

		[Test]
		public void GetPackageInfo_PackageListExists_MatchModeIdContain_NotContains()
		{
			MatchMode matchMode = MatchMode.IdContains;
			string packageId = "mytestPackage";
			string searchString = "iamnotthere";

			var package = CreateLockedTestPackage(packageId);
			var packages = new PackagesBuilder().AddPackage(package).Build();
			CreatePackageList(packages);

			var sut = CreateSystemUnderTest();
			var packageInfo = sut.GetPackageInfo(searchString, ListMode.LimitOutput, matchMode);

			Assert.That(packageInfo, Is.Empty);
		}

		[Test]
		public void GetPackageInfo_PackageListExists_MatchModeIdContains_Contains()
		{
			MatchMode matchMode = MatchMode.IdContains;
			string packageId = "mytestPackage";
			string searchString = "testP";

			var package = CreateLockedTestPackage(packageId);
			var packages = new PackagesBuilder().AddPackage(package).Build();
			CreatePackageList(packages);

			var sut = CreateSystemUnderTest();
			var packageInfos = sut.GetPackageInfo(searchString, ListMode.LimitOutput, matchMode);

			Assert.That(packageInfos, Is.Not.Null);
			Assert.That(packageInfos, Is.Not.Null);
			Assert.That(packageInfos.Count, Is.EqualTo(1));
			var packageInfo = packageInfos.ElementAt(0);
			Assert.That(packageInfo.PackageId, Is.EqualTo(package.PackageId));
			Assert.That(packageInfo.PackageVersion, Is.EqualTo(package.PackageVersion));
			Assert.That(packageInfo.LockedByAction, Is.Null);
			Assert.That(packageInfo.LockedByProcess, Is.Null);
			Assert.That(packageInfo.Created, Is.Null);
			Assert.That(packageInfo.LastUpdated, Is.Null);
		}

		/// <summary>
		/// The GetSinglePackageInfo Method is for upgrading packages. A unique id is crucial there and not every code should check that
		/// </summary>
		[Test]
		public void GetSinglePackageInfo_MultipleEntriesWithSameIdExists_ThrowsException()
		{
			string firstPackageId = "myTestPackage";
			string secondPackageId = "myTestPackage";

			var firstPackage = CreateLockedTestPackage(firstPackageId);
			var secondPackage = CreateLockedTestPackage(secondPackageId);
			var packages = new PackagesBuilder()
				.AddPackage(firstPackage)
				.AddPackage(secondPackage)
				.Build();
			CreatePackageList(packages);

			var sut = CreateSystemUnderTest();
			var exception = Assert.Throws<InvalidOperationException>(() => sut.GetSinglePackageInfo(firstPackageId));
			StringAssert.Contains(firstPackageId, exception.Message);
		}

		[Test]
		public void GetSinglePackageInfo_PackageListIsCorrupted_ShouldNotDeleted()
		{
			var brokenList = "<Packages></Packages";
			File.WriteAllText(TestPackageListFileName(), brokenList);

			var sut = CreateSystemUnderTest();

			var exception = Assert.Throws<InvalidOperationException>(() => sut.GetPackageInfo(string.Empty, ListMode.Full, MatchMode.All));

			Assert.That(exception.Message, Contains.Substring(TestPackageListFileName()));
			Assert.That(exception.InnerException, Is.TypeOf<XmlException>());
			File.Exists(TestPackageListFileName());
			var content = File.ReadAllText(TestPackageListFileName());
			Assert.That(content, Is.EqualTo(brokenList));
		}

		/// <summary>
		/// The GetPackageInfo Method is for querying the packages. Its crucial that the user get all infos that he needs and get no exception
		/// </summary>
		[Test]
		public void GetPackageInfo_MultipleEntriesWithSameIdExists_ReturnsBoth()
		{
			MatchMode matchMode = MatchMode.IdContains;
			string firstPackageId = "myTestPackage";
			string secondPackageId = "myTestPackage";

			var firstPackage = CreateLockedTestPackage(firstPackageId);
			var secondPackage = CreateLockedTestPackage(secondPackageId);
			var packages = new PackagesBuilder()
				.AddPackage(firstPackage)
				.AddPackage(secondPackage)
				.Build();
			CreatePackageList(packages);

			var sut = CreateSystemUnderTest();
			var packageInfos = sut.GetPackageInfo(firstPackageId, ListMode.LimitOutput, matchMode);

			Assert.That(packageInfos, Is.Not.Null);
			Assert.That(packageInfos.Count, Is.EqualTo(2));
		}

		[Test]
		public void GetPackageInfo_MultipleEntriesExists_MatchModeIdContains_Contains()
		{
			MatchMode matchMode = MatchMode.IdContains;
			string firstPackageId = "myTestPackage";
			string secondPackageId = "AnotherTestPackage";
			string thirdPackageId = "AnotherPackage";
			string searchPattern = "TestPackage";

			var firstPackage = CreateLockedTestPackage(firstPackageId);
			var secondPackage = CreateLockedTestPackage(secondPackageId);
			var thirdPackage = CreateLockedTestPackage(thirdPackageId);
			var packages = new PackagesBuilder()
				.AddPackage(firstPackage)
				.AddPackage(secondPackage)
				.AddPackage(thirdPackage)
				.Build();
			CreatePackageList(packages);

			var sut = CreateSystemUnderTest();
			var packageInfos = sut.GetPackageInfo(searchPattern, ListMode.LimitOutput, matchMode);

			Assert.That(packageInfos, Is.Not.Null);
			Assert.That(packageInfos.Count, Is.EqualTo(2));
			Assert.That(packageInfos.Where(x => x.PackageId.Equals(firstPackageId)).ToArray().Length, Is.EqualTo(1));
			Assert.That(packageInfos.Where(x => x.PackageId.Equals(secondPackageId)).ToArray().Length, Is.EqualTo(1));
		}

		[Test]
		public void GetPackageInfo_IsCaseInsensitive_MatchModeIdContains_Contains()
		{
			MatchMode matchMode = MatchMode.IdContains;
			string firstPackageId = "mytestPackage";
			string secondPackageId = "AnotherTestPackage";
			string searchPattern = "TEstpACkAgE";

			var firstPackage = CreateLockedTestPackage(firstPackageId);
			var secondPackage = CreateLockedTestPackage(secondPackageId);
			var packages = new PackagesBuilder()
				.AddPackage(firstPackage)
				.AddPackage(secondPackage)
				.Build();
			CreatePackageList(packages);

			var sut = CreateSystemUnderTest();
			var packageInfos = sut.GetPackageInfo(searchPattern, ListMode.LimitOutput, matchMode);

			Assert.That(packageInfos, Is.Not.Null);
			Assert.That(packageInfos.Count, Is.EqualTo(2));
			Assert.That(packageInfos.Where(x => x.PackageId.Equals(firstPackageId)).ToArray().Length, Is.EqualTo(1));
			Assert.That(packageInfos.Where(x => x.PackageId.Equals(secondPackageId)).ToArray().Length, Is.EqualTo(1));
		}

		[Test]
		public void GetPackageInfo_MultipleEntriesExists_MatchModeAll()
		{
			MatchMode matchMode = MatchMode.All;
			string firstPackageId = "myTestPackage";
			string secondPackageId = "AnotherTestPackage";
			string thirdPackageId = "AnotherPackage";
			string searchString = "TestPackage";

			var firstPackage = CreateLockedTestPackage(firstPackageId);
			var secondPackage = CreateLockedTestPackage(secondPackageId);
			var thirdPackage = CreateLockedTestPackage(thirdPackageId);
			var packages = new PackagesBuilder()
				.AddPackage(firstPackage)
				.AddPackage(secondPackage)
				.AddPackage(thirdPackage)
				.Build();
			CreatePackageList(packages);

			var sut = CreateSystemUnderTest();
			var packageInfos = sut.GetPackageInfo(searchString, ListMode.LimitOutput, matchMode);

			Assert.That(packageInfos, Is.Not.Null);
			Assert.That(packageInfos.Count, Is.EqualTo(3));
			Assert.That(packageInfos.Where(x => x.PackageId.Equals(firstPackageId)).ToArray().Length, Is.EqualTo(1));
			Assert.That(packageInfos.Where(x => x.PackageId.Equals(secondPackageId)).ToArray().Length, Is.EqualTo(1));
			Assert.That(packageInfos.Where(x => x.PackageId.Equals(thirdPackageId)).ToArray().Length, Is.EqualTo(1));
		}
		#endregion MatchMode

		[Test]
		public void GetSinglePackageInfo_LastUpdateIsNull()
		{
			Nullable<DateTimeOffset> lastUpdated = null;
			var package = CreateLockedTestPackage();
			package.SetLastUpdated(lastUpdated);
			var packages = new PackagesBuilder().AddPackage(package).Build();
			CreatePackageList(packages);

			var sut = CreateSystemUnderTest();
			var packageInfo = sut.GetSinglePackageInfo(package.PackageId);

			Assert.That(packageInfo, Is.Not.Null);
			Assert.That(packageInfo.LastUpdated, Is.EqualTo(lastUpdated));
		}

		[Test]
		public void GetPackageInfo_LastUpdateIsNull()
		{
			Nullable<DateTimeOffset> lastUpdated = null;

			ListMode listMode = ListMode.Full;

			var package = CreateLockedTestPackage();
			package.SetLastUpdated(lastUpdated);
			var packages = new PackagesBuilder().AddPackage(package).Build();
			CreatePackageList(packages);

			var sut = CreateSystemUnderTest();
			var packageInfos = sut.GetPackageInfo(package.PackageId, listMode, MatchMode.IdExact);

			Assert.That(packageInfos, Is.Not.Null);
			Assert.That(packageInfos.Count, Is.EqualTo(1));
			var packageInfo = packageInfos.ElementAt(0);
			Assert.That(packageInfo.LastUpdated, Is.EqualTo(lastUpdated));
		}
		#endregion GetPackageInfo

		#region StartActionOnPackage
		[Test]
		public void StartActionOnPackage_NoPackageListExists()
		{
			string packageId = "mytestpackage";
			Version packageVersion = new Version(1, 2, 3);

			var sut = CreateSystemUnderTest();

			sut.StartActionOnPackage(packageId, packageVersion);

			var packageInfos = sut.GetPackageInfo(packageId, ListMode.Full, MatchMode.IdExact);

			Assert.That(packageInfos, Is.Not.Null);
			Assert.That(packageInfos.Count, Is.EqualTo(1));
			var packageInfo = packageInfos.ElementAt(0);
			Assert.That(packageInfo.PackageId, Is.EqualTo(packageId));
			Assert.That(packageInfo.PackageVersion, Is.EqualTo(packageVersion.ToString()));
			Assert.That(packageInfo.LockedByAction, Is.Not.Null);
			Assert.That(packageInfo.LockedByProcess, Is.EqualTo(Process.GetCurrentProcess().Id.ToString()));
			Assert.That(packageInfo.Created, Is.Not.Null);
			Assert.That(packageInfo.LastUpdated, Is.Null);
		}

		[Test]
		public void StartActionOnPackage_PackageListExists()
		{
			var package = CreateNonLockedTestPackage();
			var packages = new PackagesBuilder().AddPackage(package).Build();
			CreatePackageList(packages);

			var sut = CreateSystemUnderTest();

			sut.StartActionOnPackage(package.PackageId, new Version(package.PackageVersion));

			var packageInfos = sut.GetPackageInfo(package.PackageId, ListMode.Full, MatchMode.IdExact);

			Assert.That(packageInfos, Is.Not.Null);
			Assert.That(packageInfos.Count, Is.EqualTo(1));
			var packageInfo = packageInfos.ElementAt(0);
			Assert.That(packageInfo.PackageId, Is.EqualTo(package.PackageId));
			Assert.That(packageInfo.PackageVersion, Is.EqualTo(package.PackageVersion));
			Assert.That(packageInfo.LockedByAction, Is.Not.Null);
			Assert.That(packageInfo.LockedByProcess, Is.EqualTo(Process.GetCurrentProcess().Id.ToString()));
			Assert.That(packageInfo.Created, Is.Not.Null);
			Assert.That(packageInfo.LastUpdated, Is.Not.Null);
		}
		#endregion StartActionOnPackage

		#region EndActionOnPackage
		[Test]
		public void EndActionOnPackage_NoPackageListExists()
		{
			string packageId = "mytestpackage";
			Version packageVersion = new Version(1, 2, 3);

			var sut = CreateSystemUnderTest();

			sut.EndActionOnPackage(packageId, packageVersion);

			var packageInfos = sut.GetPackageInfo(packageId, ListMode.Full, MatchMode.IdExact);

			Assert.That(packageInfos, Is.Not.Null);
			Assert.That(packageInfos.Count, Is.EqualTo(1));
			var packageInfo = packageInfos.ElementAt(0);
			Assert.That(packageInfo.PackageId, Is.EqualTo(packageId));
			Assert.That(packageInfo.PackageVersion, Is.EqualTo(packageVersion.ToString()));
			Assert.That(packageInfo.LockedByAction, Is.EqualTo(string.Empty));
			Assert.That(packageInfo.LockedByProcess, Is.EqualTo(string.Empty));
			Assert.That(packageInfo.Created, Is.Not.Null);
		}

		[Test]
		public void EndActionOnPackage_PackageListExists()
		{
			var package = CreateNonLockedTestPackage();
			var packages = new PackagesBuilder().AddPackage(package).Build();
			CreatePackageList(packages);

			var sut = CreateSystemUnderTest();

			sut.EndActionOnPackage(package.PackageId, new Version(package.PackageVersion));

			var packageInfos = sut.GetPackageInfo(package.PackageId, ListMode.Full, MatchMode.IdExact);

			Assert.That(packageInfos, Is.Not.Null);
			Assert.That(packageInfos.Count, Is.EqualTo(1));
			var packageInfo = packageInfos.ElementAt(0);
			Assert.That(packageInfo.PackageId, Is.EqualTo(package.PackageId));
			Assert.That(packageInfo.PackageVersion, Is.EqualTo(package.PackageVersion));
			Assert.That(packageInfo.LockedByAction, Is.EqualTo(string.Empty));
			Assert.That(packageInfo.LockedByProcess, Is.EqualTo(string.Empty));
			Assert.That(packageInfo.Created, Is.Not.Null);
			Assert.That(packageInfo.LastUpdated, Is.Not.Null);
		}
		#endregion EndActionOnPackage 

		#region PackageIsLocked
		[Test]
		public void PackageIsLocked_CanBeListedByAnotherProcess()
		{
			var package = CreateLockedTestPackage();
			var packages = new PackagesBuilder().AddPackage(package).Build();
			CreatePackageList(packages);

			var sut = CreateSystemUnderTest();

			var packageInfos = sut.GetPackageInfo(package.PackageId, ListMode.LimitOutput, MatchMode.IdExact);

			Assert.That(packageInfos, Is.Not.Null);
			Assert.That(packageInfos.Count, Is.EqualTo(1));
			var packageInfo = packageInfos.ElementAt(0);
			Assert.That(packageInfo.PackageId, Is.EqualTo(package.PackageId));
		}

		[Test]
		public void PackageIsLocked_CanBeUnlockedBySameProcess()
		{
			string myTestPackageId = "myTestPackage";
			Version myTestPackageVersion = new Version(1, 2, 3);

			var sut = CreateSystemUnderTest();
			sut.StartActionOnPackage(myTestPackageId, myTestPackageVersion);
			sut.EndActionOnPackage(myTestPackageId, myTestPackageVersion);

			Assert.Pass();
		}

		[Test]
		public void PackageIsLocked_CanBeLockedBySameProcess()
		{
			string myTestPackageId = "myTestPackage";
			Version myTestPackageVersion = new Version(1, 2, 3);

			var sut = CreateSystemUnderTest();

			sut.StartActionOnPackage(myTestPackageId, myTestPackageVersion);
			sut.StartActionOnPackage(myTestPackageId, myTestPackageVersion);

			Assert.Pass();
		}

		[Test]
		public void PackageIsLocked_CanNotBeLockedByAnotherProcess()
		{
			var package = CreateLockedTestPackage();
			var packages = new PackagesBuilder().AddPackage(package).Build();
			CreatePackageList(packages);

			var sut = CreateSystemUnderTest();

			var exception = Assert.Throws<InvalidOperationException>(() => sut.StartActionOnPackage(package.PackageId, new Version(package.PackageVersion)));
			StringAssert.Contains(package.PackageId, exception.Message);
			StringAssert.Contains(package.PackageLockedByProcess, exception.Message);
			StringAssert.Contains(package.PackageLockedByAction, exception.Message);
		}
		#endregion PackageIsLocked

		#region FileIsUsedByAnotherProcess ParallelBehavior
		[Test]
		public void FileIsUsedByAnotherProcess_FileIsLockedForWriteOperation_FileCanBeOpenedForGetPackageInfo()
		{
			var package = CreateNonLockedTestPackage();
			var packages = new PackagesBuilder().AddPackage(package).Build();
			CreatePackageList(packages);

			var sut = CreateSystemUnderTest();
			using (sut.OpenPackageListForWriteOperation())
			{
				sut.GetPackageInfo(package.PackageId, ListMode.LimitOutput, MatchMode.IdExact);
			};
		}

		[Test]
		public void FileIsUsedByAnotherProcess_FileIsLockedForWriteOperation_FileCanNotBeOpenedForStartActionOnPackage()
		{
			var package = CreateNonLockedTestPackage();
			var packages = new PackagesBuilder().AddPackage(package).Build();
			CreatePackageList(packages);

			var sut = CreateSystemUnderTest();
			using (sut.OpenPackageListForWriteOperation())
			{
				var exception = Assert.Throws<PackageFileCanNotBeAccessedException>(() => sut.StartActionOnPackage(package.PackageId, new Version(package.PackageVersion)));
				Assert.That(exception.InnerExceptions, Is.Not.Null);
				Assert.That(exception.InnerExceptions.Count, Is.GreaterThan(2));
				StringAssert.Contains(TestPackageListFileName(), exception.Message);
			};
		}

		[Test]
		public void FileIsUsedByAnotherProcess_FileIsLockedForWriteOperation_FileCanNotBeOpenedForEndActionOnPackage()
		{
			var package = CreateNonLockedTestPackage();
			var packages = new PackagesBuilder().AddPackage(package).Build();
			CreatePackageList(packages);

			var sut = CreateSystemUnderTest();
			using (sut.OpenPackageListForWriteOperation())
			{
				var exception = Assert.Throws<PackageFileCanNotBeAccessedException>(() => sut.EndActionOnPackage(package.PackageId, new Version(package.PackageVersion)));
				Assert.That(exception.InnerExceptions, Is.Not.Null);
				Assert.That(exception.InnerExceptions.Count, Is.GreaterThan(2));
				StringAssert.Contains(TestPackageListFileName(), exception.Message);
			};

		}

		[Test]
		public async Task GetSinglePackageInfo_FileIsUsedByAnotherProcess_FileIsTemporaryLockedForWriteOperation_StartActionOnPackageRetries()
		{
			var package = CreateNonLockedTestPackage();
			var packages = new PackagesBuilder().AddPackage(package).Build();
			CreatePackageList(packages);

			var sut = CreateSystemUnderTest();
			using (var fileStream = sut.OpenPackageListForWriteOperation())
			{
				var task = Task.Run(() => sut.StartActionOnPackage(package.PackageId, new Version(package.PackageVersion)));
				Thread.Sleep(1000);
				fileStream.Close();

				await task;
			};

			var packageInfo = sut.GetSinglePackageInfo(package.PackageId, ListMode.Full);
			Assert.That(packageInfo.LockedByProcess, Is.EqualTo(Process.GetCurrentProcess().Id.ToString()));
		}

		[Test]
		public async Task GetPackageInfo_FileIsUsedByAnotherProcess_FileIsTemporaryLockedForWriteOperation_StartActionOnPackageRetries()
		{
			var package = CreateNonLockedTestPackage();
			var packages = new PackagesBuilder().AddPackage(package).Build();
			CreatePackageList(packages);

			var sut = CreateSystemUnderTest();
			using (var fileStream = sut.OpenPackageListForWriteOperation())
			{
				var task = Task.Run(() => sut.StartActionOnPackage(package.PackageId, new Version(package.PackageVersion)));
				Thread.Sleep(1000);
				fileStream.Close();

				await task;
			};

			var packageInfos = sut.GetPackageInfo(package.PackageId, ListMode.Full, MatchMode.IdExact);
			Assert.That(packageInfos, Is.Not.Null);
			Assert.That(packageInfos.Count, Is.EqualTo(1));
			var packageInfo = packageInfos.ElementAt(0);
			Assert.That(packageInfo.LockedByProcess, Is.EqualTo(Process.GetCurrentProcess().Id.ToString()));
		}

		[Test]
		public async Task GetSinglePackageInfo_FileIsUsedByAnotherProcess_FileIsTemporaryLockedForWriteOperation_EndActionOnPackageRetries()
		{
			var package = CreateNonLockedTestPackage();
			var packages = new PackagesBuilder().AddPackage(package).Build();
			CreatePackageList(packages);

			var sut = CreateSystemUnderTest();
			using (var fileStream = sut.OpenPackageListForWriteOperation())
			{
				var task = Task.Run(() => sut.EndActionOnPackage(package.PackageId, new Version(package.PackageVersion)));
				Thread.Sleep(1000);
				fileStream.Close();

				await task;
			};

			var packageInfo = sut.GetSinglePackageInfo(package.PackageId, ListMode.Full);
			Assert.That(packageInfo.LockedByProcess, Is.Not.Null);
		}

		[Test]
		public async Task GetPackageInfo_FileIsUsedByAnotherProcess_FileIsTemporaryLockedForWriteOperation_EndActionOnPackageRetries()
		{
			var package = CreateNonLockedTestPackage();
			var packages = new PackagesBuilder().AddPackage(package).Build();
			CreatePackageList(packages);

			var sut = CreateSystemUnderTest();
			using (var fileStream = sut.OpenPackageListForWriteOperation())
			{
				var task = Task.Run(() => sut.EndActionOnPackage(package.PackageId, new Version(package.PackageVersion)));
				Thread.Sleep(1000);
				fileStream.Close();

				await task;
			};

			var packageInfos = sut.GetPackageInfo(package.PackageId, ListMode.Full, MatchMode.IdExact);
			Assert.That(packageInfos, Is.Not.Null);
			Assert.That(packageInfos.Count, Is.EqualTo(1));
			var packageInfo = packageInfos.ElementAt(0);
			Assert.That(packageInfo.LockedByProcess, Is.Not.Null);
		}

		[Test]
		public void FileIsUsedByAnotherProcess_FileIsLockedForWriteOperation_GetPackageInfoWorks()
		{
			var package = CreateNonLockedTestPackage();
			var packages = new PackagesBuilder().AddPackage(package).Build();
			CreatePackageList(packages);

			var sut = CreateSystemUnderTest();
			using (var fileStream = sut.OpenPackageListForWriteOperation())
			{
				var packageInfo = sut.GetPackageInfo(package.PackageId, ListMode.LimitOutput, MatchMode.IdExact);
				Assert.That(packageInfo, Is.Not.Null);
			};
		}

		[Test]
		public void GetSinglePackageInfo_FileIsUsedByAnotherProcess_FileIsLockedForReadOperation_StartActionOnPackageWorks()
		{
			var package = CreateNonLockedTestPackage();
			var packages = new PackagesBuilder().AddPackage(package).Build();
			CreatePackageList(packages);

			var sut = CreateSystemUnderTest();
			using (var fileStream = sut.OpenPackageListForReadOperation())
			{
				sut.StartActionOnPackage(package.PackageId, new Version(package.PackageVersion));
			};

			var packageInfo = sut.GetSinglePackageInfo(package.PackageId, ListMode.Full);
			Assert.That(packageInfo.LockedByProcess, Is.EqualTo(Process.GetCurrentProcess().Id.ToString()));
		}

		[Test]
		public void GetPackageInfo_FileIsUsedByAnotherProcess_FileIsLockedForReadOperation_StartActionOnPackageWorks()
		{
			var package = CreateNonLockedTestPackage();
			var packages = new PackagesBuilder().AddPackage(package).Build();
			CreatePackageList(packages);

			var sut = CreateSystemUnderTest();
			using (var fileStream = sut.OpenPackageListForReadOperation())
			{
				sut.StartActionOnPackage(package.PackageId, new Version(package.PackageVersion));
			};

			var packageInfos = sut.GetPackageInfo(package.PackageId, ListMode.Full, MatchMode.IdExact);
			Assert.That(packageInfos, Is.Not.Null);
			Assert.That(packageInfos.Count, Is.EqualTo(1));
			var packageInfo = packageInfos.ElementAt(0);
			Assert.That(packageInfo.LockedByProcess, Is.EqualTo(Process.GetCurrentProcess().Id.ToString()));
		}

		[Test]
		public void GetSinglePackageInfo_FileIsUsedByAnotherProcess_FileIsLockedForReadOperation_EndActionOnPackageWork()
		{
			var package = CreateNonLockedTestPackage();
			var packages = new PackagesBuilder().AddPackage(package).Build();
			CreatePackageList(packages);

			var sut = CreateSystemUnderTest();
			using (var fileStream = sut.OpenPackageListForReadOperation())
			{
				sut.EndActionOnPackage(package.PackageId, new Version(package.PackageVersion));
			};

			var packageInfo = sut.GetSinglePackageInfo(package.PackageId, ListMode.Full);
			Assert.That(packageInfo.LastUpdated, Is.Not.Null);
		}

		[Test]
		public void GetPackageInfo_FileIsUsedByAnotherProcess_FileIsLockedForReadOperation_EndActionOnPackageWork()
		{
			var package = CreateNonLockedTestPackage();
			var packages = new PackagesBuilder().AddPackage(package).Build();
			CreatePackageList(packages);

			var sut = CreateSystemUnderTest();
			using (var fileStream = sut.OpenPackageListForReadOperation())
			{
				sut.EndActionOnPackage(package.PackageId, new Version(package.PackageVersion));
			};

			var packageInfos = sut.GetPackageInfo(package.PackageId, ListMode.Full, MatchMode.IdExact);
			Assert.That(packageInfos, Is.Not.Null);
			Assert.That(packageInfos.Count, Is.EqualTo(1));
			var packageInfo = packageInfos.ElementAt(0);
			Assert.That(packageInfo.LastUpdated, Is.Not.Null);
		}

		[Test]
		public void GetSinglePackageInfo_FileIsUsedByAnotherProcess_FileIsLockedForReadOperation_GetPackageInfoWorks()
		{
			var package = CreateNonLockedTestPackage();
			var packages = new PackagesBuilder().AddPackage(package).Build();
			CreatePackageList(packages);

			var sut = CreateSystemUnderTest();
			using (var fileStream = sut.OpenPackageListForReadOperation())
			{
				var packageInfo = sut.GetSinglePackageInfo(package.PackageId);
				Assert.That(packageInfo, Is.Not.Null);
			};
		}

		[Test]
		public void GetPackageInfo_FileIsUsedByAnotherProcess_FileIsLockedForReadOperation_GetPackageInfoWorks()
		{
			var package = CreateNonLockedTestPackage();
			var packages = new PackagesBuilder().AddPackage(package).Build();
			CreatePackageList(packages);

			var sut = CreateSystemUnderTest();
			using (var fileStream = sut.OpenPackageListForReadOperation())
			{
				var packageInfo = sut.GetPackageInfo(package.PackageId, ListMode.LimitOutput, MatchMode.IdExact);
				Assert.That(packageInfo, Is.Not.Null);
			};
		}
		#endregion  PackagesFileIsIsUsages ParallelBehaviorr

		#region Migration
		[Test]
		public void Migration_StartActionOnPackage_PackageListIsMigratedAndVersionIsAddedIfNoVersionExists()
		{
			var expectedVersion = "1.0";

			var sut = CreateSystemUnderTest(new[] { new AddVersionAttributeIfNotExists() });
		
			var previous = new PackagesBuilder().SetPackagesVersion(string.Empty).Build();
			CreatePackageList(previous);

			sut.StartActionOnPackage("mytestpackage", new Version(1, 2, 3));

			var xmlDocument = ReadPackageList();
			var version = xmlDocument.Element(HoneyLibrary.PackageLists.PackageList.XmlRoot).Attribute(HoneyLibrary.PackageLists.PackageList.XmlPackageVersion).Value;

			Assert.That(version, Is.EqualTo(expectedVersion));
		}

		[Test]
		public void Migration_StartActionOnPackage_PackageListIsNotMigratedAndVersionIsNotAddedIfVersionExists()
		{
			var expectedVersion = "myversion";

			var sut = CreateSystemUnderTest(new[] { new AddVersionAttributeIfNotExists() });

			var previous = new PackagesBuilder().SetPackagesVersion(expectedVersion).Build();
			CreatePackageList(previous);

			sut.StartActionOnPackage("mytestpackage", new Version(1, 2, 3));

			var xmlDocument = ReadPackageList();
			var version = xmlDocument.Element(HoneyLibrary.PackageLists.PackageList.XmlRoot).Attribute(HoneyLibrary.PackageLists.PackageList.XmlPackageVersion).Value;

			Assert.That(version, Is.EqualTo(expectedVersion));
		}

		[Test]
		public void Migration_EndActionOnPackage_PackageListIsMigratedAndVersionIsAddedIfNoVersionExists()
		{
			var expectedVersion = "1.0";

			var sut = CreateSystemUnderTest(new[] { new AddVersionAttributeIfNotExists() });

			var previous = new PackagesBuilder().SetPackagesVersion(string.Empty).Build();
			CreatePackageList(previous);

			sut.EndActionOnPackage("mytestpackage", new Version(1, 2, 3));

			var xmlDocument = ReadPackageList();
			var version = xmlDocument.Element(HoneyLibrary.PackageLists.PackageList.XmlRoot).Attribute(HoneyLibrary.PackageLists.PackageList.XmlPackageVersion).Value;

			Assert.That(version, Is.EqualTo(expectedVersion));
		}

		[Test]
		public void Migration_EndActionOnPackage_PackageListIsNotMigratedAndVersionIsNotAddedIfVersionExists()
		{
			var expectedVersion = "myversion";

			var sut = CreateSystemUnderTest(new[] { new AddVersionAttributeIfNotExists() });

			var previous = new PackagesBuilder().SetPackagesVersion(expectedVersion).Build();
			CreatePackageList(previous);

			sut.EndActionOnPackage("mytestpackage", new Version(1, 2, 3));

			var xmlDocument = ReadPackageList();
			var version = xmlDocument.Element(HoneyLibrary.PackageLists.PackageList.XmlRoot).Attribute(HoneyLibrary.PackageLists.PackageList.XmlPackageVersion).Value;

			Assert.That(version, Is.EqualTo(expectedVersion));
		}

		[Test]
		[Ignore("Remove Package is not not implemented :)")]
		public void Migration_RemovePackage_PackageListIsMigratedAndVersionIsAddedIfNoVersionExists()
		{
			var expectedVersion = "1.0";

			var sut = CreateSystemUnderTest(new[] { new AddVersionAttributeIfNotExists() });

			var previous = new PackagesBuilder().SetPackagesVersion(string.Empty).Build();
			CreatePackageList(previous);

			sut.RemovePackage("mytestpackage");

			var xmlDocument = ReadPackageList();
			var version = xmlDocument.Element(HoneyLibrary.PackageLists.PackageList.XmlRoot).Attribute(HoneyLibrary.PackageLists.PackageList.XmlPackageVersion).Value;

			Assert.That(version, Is.EqualTo(expectedVersion));
		}

		[Test]
		[Ignore("Remove Package is not not implemented :)")]
		public void Migration_RemovePackage_PackageListIsNotMigratedAndVersionIsNotAddedIfVersionExists()
		{
			var expectedVersion = "myversion";

			var sut = CreateSystemUnderTest(new[] { new AddVersionAttributeIfNotExists() });

			var previous = new PackagesBuilder().SetPackagesVersion(expectedVersion).Build();
			CreatePackageList(previous);

			sut.RemovePackage("mytestpackage");

			var xmlDocument = ReadPackageList();
			var version = xmlDocument.Element(HoneyLibrary.PackageLists.PackageList.XmlRoot).Attribute(HoneyLibrary.PackageLists.PackageList.XmlPackageVersion).Value;

			Assert.That(version, Is.EqualTo(expectedVersion));
		}
		#endregion Migration
	}
}
