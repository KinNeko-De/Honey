using System;
using System.Collections.Generic;
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

		internal static void AbortIfLockedByProcess(XElement packageListInfoElement)
		{
			var lockedbyProcess = packageListInfoElement.Element(XmlLockedByProcess).Value;
			if (!string.IsNullOrEmpty(lockedbyProcess) && lockedbyProcess != Process.GetCurrentProcess().Id.ToString())
			{
				// Throw honey specific exception and handle this different than unexpected exception (reduced output to non stacktrace and return specific exit code)
				throw new InvalidOperationException($"Package {packageListInfoElement.Element(XmlPackageId)} can not be locked. Action '{packageListInfoElement.Element(XmlLockedByAction).Value}' is on progress by process '{packageListInfoElement.Element(XmlLockedByProcess).Value}'");
			}
		}

		internal static void UpdatePackageListInfo(Version version, string action, string processId, XElement packageListInfoElement)
		{
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

		internal static XDocument CreateNewPackageList()
		{
			return new XDocument(
						new XDeclaration("1.0", "utf-8", "yes"),
						new XElement(XmlRoot, 
							new XAttribute(XmlRootVersionAttribute, currentVersion)
						)
						);
		}

		public static XDocument Load(FileStream fileStream)
		{
			XDocument packageList = XDocument.Load(fileStream, LoadOptions.PreserveWhitespace);

			MigratePackagList(packageList);

			return packageList;
		}

		/// <summary>
		/// Example for a migration of a list
		/// </summary>
		/// <param name="packageList"></param>
		private static void MigratePackagList(XDocument packageList)
		{
			string packageListVersion = GetPackageListVersion(packageList);

			switch(packageListVersion)
			{
				case null:
					// if the version attribute is not set it will be added
					packageList.Root.SetAttributeValue(XmlRootVersionAttribute, currentVersion);
					break;
				default:
					break;
			}
		}

		private static string GetPackageListVersion(XDocument packageList)
		{
			var versionAttribute = packageList.Root.Attribute(XmlRootVersionAttribute);

			if(versionAttribute == null)
			{
				return null;
			}

			return versionAttribute.Value;
		}

		internal static XElement FetchPackageListInfo(string packageId, XDocument packageList)
		{
			IEnumerable<XElement> matchingPackageIds = packageList.Root.Elements(XmlPackage).Where(x => x.Element(XmlPackageId).Value == packageId);
			if (matchingPackageIds.Count() > 1)
			{
				throw new InvalidOperationException($"Multiple entries of package id { packageId } were found in the package repository. This is not supported.");
			}
			return matchingPackageIds.SingleOrDefault();
		}

		internal static XElement CreatePackageListInfo(string packageId, Version version, string action, string processId, XDocument packageList)
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
			packageList.Root.Add(packageListInfoElement);
			return packageListInfoElement;
		}

		internal static IPackageInfo ReadPackageInfo(XElement packageListInfoElement, ListMode listMode)
		{
			if (packageListInfoElement == null)
			{
				return null;
			}

			PackageInfo packageInfo = new PackageInfo()
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
	}
}
