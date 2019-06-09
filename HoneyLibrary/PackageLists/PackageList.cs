using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace HoneyLibrary.PackageLists
{
	public class PackageList
	{
		internal const string XmlRoot = "Packages";
		internal const string XmlRootVersionAttribute = "Version";
		internal const string XmlPackage = "Package";
		internal const string XmlPackageId = "Id";
		internal const string XmlPackageVersion = "Version";
		internal const string XmlCreated = "Created";
		internal const string XmlLastUpdated = "LastUpdated";
		internal const string XmlLockedByAction = "LockedByAction";
		internal const string XmlLockedByProcess = "LockedByProcess";
		internal const string currentVersion = "1.0";

		private XDocument xpackageList;

		public PackageList(XDocument xpackageList)
		{
			this.xpackageList = xpackageList;
		}

		internal static PackageList Load(FileStream fileStream, IEnumerable<IPackageListMigration> packageListMigrations)
		{
			XDocument packageList = XDocument.Load(fileStream, LoadOptions.PreserveWhitespace);

			MigratePackagList(packageList, packageListMigrations);

			return new PackageList(packageList);
		}

		/// <summary>
		/// Possibility to migrate the packageList here. See the tests for an example
		/// </summary>
		/// <param name="packageList"></param>
		private static void MigratePackagList(XDocument packageList, IEnumerable<IPackageListMigration> packageListMigrations)
		{
			if(packageListMigrations != null)
			{
				foreach (var packageListMigration in packageListMigrations)
				{
					packageListMigration.Migrate(packageList);
				}
			}
		}

		internal static PackageList CreateNewPackageList()
		{
			var xpackageList = new XDocument(
						new XDeclaration("1.0", "utf-8", "yes"),
						new XElement(XmlRoot,
							new XAttribute(XmlRootVersionAttribute, currentVersion)
						)
						);
			return new PackageList(xpackageList);
		}

		internal void UpdatePackageListInfo(Version version, string action, string processId, PackageInfo packageInfo)
		{
			var packageListInfoElement = packageInfo.packageListInfoElement;
			var lockedbyProcess = packageListInfoElement.Element(XmlLockedByProcess).Value;
			if (!string.IsNullOrEmpty(lockedbyProcess) && lockedbyProcess != Process.GetCurrentProcess().Id.ToString())
			{
				// Throw honey specific exception and handle this different than unexpected exception (reduced output to non stacktrace and return specific exit code)
				throw new InvalidOperationException($"Action '{packageListInfoElement.Element(XmlLockedByAction).Value}' is on progress by process '{packageListInfoElement.Element(XmlLockedByProcess).Value}'");
			}

			packageListInfoElement.Element(XmlPackageVersion).SetValue(version);
			packageListInfoElement.Element(XmlLockedByAction).SetValue(action);
			packageListInfoElement.Element(XmlLockedByProcess).SetValue(processId);
			packageListInfoElement.Element(XmlLastUpdated).SetValue(DateTimeOffset.Now);
		}

		public IReadOnlyCollection<IPackageInfo> FetchPackageListInfo(string searchPattern, ListMode listMode, MatchMode matchMode)
		{
			var packageListInfoElement = FetchPackageListInfo(searchPattern, matchMode);
			return packageListInfoElement.Select(x => ReadPackageInfo(x, listMode)).ToArray();
		}

		private IEnumerable<XElement> FetchPackageListInfo(string searchPattern, MatchMode matchMode)
		{
			Func<string, bool> matchFunction = GetMatchFunction(searchPattern, matchMode);

			IEnumerable<XElement> matchingPackageIds = xpackageList.Root.Elements(XmlPackage).Where(x => matchFunction.Invoke(x.Element(XmlPackageId).Value));

			return matchingPackageIds;
		}

		internal PackageInfo FetchSinglePackageListInfo(string packageId)
		{
			return FetchSinglePackageListInfo(packageId, ListMode.Full);
		}

		internal PackageInfo FetchSinglePackageListInfo(string packageId, ListMode listMode)
		{
			XElement xelement = FetchSinglePackageListElement(packageId);
			return ReadPackageInfo(xelement, listMode);
		}

		private XElement FetchSinglePackageListElement(string packageId)
		{
			Func<string, bool> matchFunction = GetMatchFunction(packageId, MatchMode.IdExact);

			IEnumerable<XElement> matchingPackageIds = xpackageList.Root.Elements(XmlPackage).Where(x => matchFunction.Invoke(x.Element(XmlPackageId).Value));

			if (matchingPackageIds.Count() > 1)
			{
				throw new InvalidOperationException($"Multiple entries of package id { packageId } were found in the package repository. This is not supported.");
			}

			return matchingPackageIds.SingleOrDefault();
		}

		private Func<string, bool> GetMatchFunction(string packageId, MatchMode matchMode)
		{
			Func<string, bool> matchFuction;

			switch (matchMode)
			{
				case MatchMode.All:
					matchFuction = new Func<string, bool>((xmlPackageId) => true);
					break;
				case MatchMode.IdExact:
					matchFuction = new Func<string, bool>((xmlPackageId) => xmlPackageId == packageId);
					break;
				case MatchMode.IdContains:
					matchFuction = new Func<string, bool>((xmlPackageId) => xmlPackageId.IndexOf(packageId, StringComparison.InvariantCultureIgnoreCase) >= 0);
					break;
				default:
					throw new InvalidEnumArgumentException(nameof(matchMode), (int)matchMode, typeof(MatchMode));
			}

			return matchFuction;
		}

		internal PackageInfo AddPackageListInfo(string packageId, Version version, string action, string processId)
		{
			XElement packageListInfoElement =
				new XElement(XmlPackage,
					new XElement(XmlPackageId, packageId),
					new XElement(XmlPackageVersion, version),
					new XElement(XmlLockedByAction, action),
					new XElement(XmlLockedByProcess, processId),
					new XElement(XmlCreated, DateTimeOffset.Now),
					new XElement(XmlLastUpdated)
				);
			xpackageList.Root.Add(packageListInfoElement);

			return ReadPackageInfo(packageListInfoElement, ListMode.Full);
		}

		internal PackageInfo ReadPackageInfo(XElement packageListInfoElement, ListMode listMode)
		{
			if (packageListInfoElement == null)
			{
				return null;
			}

			PackageInfo packageInfo = new PackageInfo(packageListInfoElement)
			{
				PackageId = packageListInfoElement.Element(XmlPackageId).Value,
				PackageVersion = packageListInfoElement.Element(XmlPackageVersion).Value
			};

			if (listMode == ListMode.Full)
			{
				// costs around 10ms more
				packageInfo.LockedByAction = packageListInfoElement.Element(XmlLockedByAction).Value;
				packageInfo.LockedByProcess = packageListInfoElement.Element(XmlLockedByProcess).Value;
				packageInfo.Created = DateTimeOffset.Parse(packageListInfoElement.Element(XmlCreated).Value);
				var lastUpdatedValue = packageListInfoElement.Element(XmlLastUpdated).Value;
				packageInfo.LastUpdated = string.IsNullOrEmpty(lastUpdatedValue) ? (DateTimeOffset?)null : DateTimeOffset.Parse(lastUpdatedValue);
			};

			return packageInfo;
		}

		public void Save(Stream stream)
		{
			xpackageList.Save(stream);
		}
	}
}
