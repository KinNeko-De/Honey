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
	public class FileWithVersionInNameInstallAction : IDeploymentComponent
	{
		private readonly ParallelReadableZipArchiveEntry newEntry;
		private readonly Version newVersion;
		private readonly string rootPathOfApplication;

		public FileWithVersionInNameInstallAction(ParallelReadableZipArchiveEntry newEntry, Version newVersion, string targetPath)
		{
			this.newEntry = newEntry;
			this.newVersion = newVersion;
			this.rootPathOfApplication = targetPath;
		}

		public void Upgrade()
		{
			InstallNewEntry();
		}

		private void InstallNewEntry()
		{
			var directoryName = Path.GetDirectoryName(rootPathOfApplication);
			Directory.CreateDirectory(directoryName);
			var newFileName = GetFileNameContainingVersion(newVersion);
			newEntry.ExtractToFile(newFileName, true);
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
