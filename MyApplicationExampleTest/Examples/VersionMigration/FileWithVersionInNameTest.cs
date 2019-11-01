using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MyApplicationExample;
using HoneyLibrary.PackageDeployment;
using NUnit.Framework;
using Assert = NUnit.Framework.Assert;
using Microsoft.Extensions.Logging.Abstractions;

namespace MyApplicationExampleTest.Examples.VersionMigration
{
	public class FileWithVersionInNameTest
	{
		const string versionexamplePackage1 = "honey.versionexample.1.0.0.nupkg";
		const string versionexamplePackage2 = "honey.versionexample.2.0.0.nupkg";

		private readonly string namespaceForEmbbededResources = typeof(FileWithVersionInNameTest).Namespace + ".TestData";
		private readonly string performanceInstallPath = Path.Combine(Path.GetTempPath(), "HoneyPerformanceVersionExample0EE19577-3CD6-4C6B-9531-8E8044F360A9");

		/// <summary>
		/// Be carefull with the performance that is calculated inside this test
		/// the performance tests shows that there is some kind of caching somewhere
		/// running the code twice is very much faster
		/// </summary>
		[Test]
		public void VersionExample_FilesAreCreated_FileIsModifiedByNewVersionButNotReplaced_FileIsRemoved()
		{
			string sourcePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			string targetPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

			try
			{
				Directory.CreateDirectory(sourcePath);
				Directory.CreateDirectory(targetPath);
				EmbeddedResourceExtensions.CopyEmbeddedResource(namespaceForEmbbededResources, versionexamplePackage1, sourcePath);
				EmbeddedResourceExtensions.CopyEmbeddedResource(namespaceForEmbbededResources, versionexamplePackage2, sourcePath);

				string expectedVersion1 = "1.0";
				string expectedContent = "DeployedWithVersion1.0";

				NugetPackage package1 = new NugetPackage($@"{Path.Combine(sourcePath, versionexamplePackage1)}");
				Stopwatch stopwatch = Stopwatch.StartNew();
				package1.Upgrade(NullLogger.Instance, new DeploymentComponentFactory(new HoneyInstallLocation(), new PathInstallLocation(targetPath)), null);
				stopwatch.Stop();
				Console.WriteLine($"Install takes {stopwatch.ElapsedMilliseconds } ms.");
				var firstFileName = Path.Combine(targetPath, $"DeployedWithVersion{expectedVersion1}.txt");
				Assert.IsTrue(File.Exists(firstFileName));
				Assert.That(File.ReadAllText(firstFileName), Is.EqualTo(expectedContent));
				var fileThatWillBeRemovedWithVersion2 = Path.Combine(targetPath, $"AnotherDeploymentWithVersion{expectedVersion1}.txt");
				Assert.IsTrue(File.Exists(fileThatWillBeRemovedWithVersion2));

				string expectedVersion2 = "2.0";
				NugetPackage package2 = new NugetPackage($@"{Path.Combine(sourcePath, versionexamplePackage2)}");
				Stopwatch stopwatch2 = Stopwatch.StartNew();
				package2.Upgrade(NullLogger.Instance, new DeploymentComponentFactory(new HoneyInstallLocation(), new PathInstallLocation(targetPath)), package1);
				stopwatch2.Stop();
				Console.WriteLine($"Upgrade takes {stopwatch2.ElapsedMilliseconds } ms.");
				var secondFileName = Path.Combine(targetPath, $"DeployedWithVersion{expectedVersion2}.txt");
				Assert.IsFalse(File.Exists(firstFileName));
				Assert.IsTrue(File.Exists(secondFileName));
				Assert.That(File.ReadAllText(secondFileName), Is.EqualTo(expectedContent));
				Assert.IsFalse(File.Exists(fileThatWillBeRemovedWithVersion2));
			}
			finally
			{
				CleanUpDirectory(sourcePath);
				CleanUpDirectory(targetPath);
			}
		}

		[Test, Order(1)]
		public void VersionExample_FileIsModifiedByNewVersionButNotReplaced_Performance_Part1()
		{
			string sourcePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			try
			{
				Directory.CreateDirectory(sourcePath);
				Directory.CreateDirectory(performanceInstallPath);
				EmbeddedResourceExtensions.CopyEmbeddedResource(namespaceForEmbbededResources, versionexamplePackage1, sourcePath);
				string expectedVersion1 = "1.0";
				string expectedContent = "DeployedWithVersion1.0";

				NugetPackage package1 = new NugetPackage($@"{Path.Combine(sourcePath, versionexamplePackage1)}");
				Stopwatch stopwatch = Stopwatch.StartNew();
				package1.Upgrade(NullLogger.Instance, new DeploymentComponentFactory(new HoneyInstallLocation(), new PathInstallLocation(performanceInstallPath)), null);
				stopwatch.Stop();
				Console.WriteLine($"Install takes {stopwatch.ElapsedMilliseconds } ms.");
				var firstFileName = Path.Combine(performanceInstallPath, $"DeployedWithVersion{expectedVersion1}.txt");
				Assert.IsTrue(File.Exists(firstFileName));
				Assert.That(File.ReadAllText(firstFileName), Is.EqualTo(expectedContent));
			}
			finally
			{
				CleanUpDirectory(sourcePath);
			}
		}

		[Test, Order(2)]
		public void VersionExample_FileIsModifiedByNewVersionButNotReplaced_Performance_Part2()
		{
			string sourcePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			try
			{
				Directory.CreateDirectory(sourcePath);
				EmbeddedResourceExtensions.CopyEmbeddedResource(namespaceForEmbbededResources, versionexamplePackage1, sourcePath);
				EmbeddedResourceExtensions.CopyEmbeddedResource(namespaceForEmbbededResources, versionexamplePackage2, sourcePath);
				string expectedVersion1 = "1.0";
				string expectedVersion2 = "2.0";
				string expectedContent = "DeployedWithVersion1.0";
				var firstFileName = Path.Combine(performanceInstallPath, $"DeployedWithVersion{expectedVersion1}.txt");

				NugetPackage package1 = new NugetPackage($@"{Path.Combine(sourcePath, versionexamplePackage1)}");
				NugetPackage package2 = new NugetPackage($@"{Path.Combine(sourcePath, versionexamplePackage2)}");
				Stopwatch stopwatch2 = Stopwatch.StartNew();
				package2.Upgrade(NullLogger.Instance, new DeploymentComponentFactory(new HoneyInstallLocation(), new PathInstallLocation(performanceInstallPath)), package1);
				stopwatch2.Stop();
				Console.WriteLine($"Install takes {stopwatch2.ElapsedMilliseconds } ms.");
				var secondFileName = Path.Combine(performanceInstallPath, $"DeployedWithVersion{expectedVersion2}.txt");
				Assert.IsFalse(File.Exists(firstFileName));
				Assert.IsTrue(File.Exists(secondFileName));
				Assert.That(File.ReadAllText(secondFileName), Is.EqualTo(expectedContent));
			}
			finally 
			{
				CleanUpDirectory(sourcePath);
			}

		}

		[Test, Order(3)]
		public void CleanUp()
		{
			CleanUpDirectory(performanceInstallPath);
		}

		private void CleanUpDirectory(string directoryToCleanUp)
		{
			if(!directoryToCleanUp.StartsWith(Path.GetTempPath())) {
				throw new InvalidOperationException($"you dont want to delete {directoryToCleanUp}");
			}

			Directory.Delete(directoryToCleanUp, true);
		}
	}
}
