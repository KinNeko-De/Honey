using HoneyLibrary.PackageDeployment;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZipArchivExtensions;

namespace MyApplicationExample.Examples.VersionMigration
{
	public class FileWithVersionInNameUpgradeAction : IDeploymentComponent
	{
		private readonly ParallelReadableZipArchiveEntry installedEntry;
		private readonly Version installedVersion;
		private readonly ParallelReadableZipArchiveEntry newEntry;
		private readonly Version newVersion;
		private readonly string rootPathOfApplication;

		public FileWithVersionInNameUpgradeAction(ParallelReadableZipArchiveEntry installedEntry, Version installedVersion, ParallelReadableZipArchiveEntry newEntry, Version newVersion, string targetPath)
		{
			this.installedEntry = installedEntry;
			this.installedVersion = installedVersion;
			this.newEntry = newEntry;
			this.newVersion = newVersion;
			this.rootPathOfApplication = targetPath;
		}

		public void Upgrade()
		{
			UpgradeInstalledEntry();
		}

		private void UpgradeInstalledEntry()
		{
			var installedFile = GetFileNameContainingVersion(installedVersion);
			var newFileName = GetFileNameContainingVersion(newVersion);
			File.Move(installedFile, newFileName);
		}

		private string GetFileNameContainingVersion(Version version)
		{
			var targetDirectory = Path.GetDirectoryName(rootPathOfApplication);
			var targetFile = Path.GetFileNameWithoutExtension(rootPathOfApplication) + version;
			var targetExtension = Path.GetExtension(rootPathOfApplication);

			return Path.Combine(targetDirectory, targetFile + targetExtension);
		}
	}
}
