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
	public class FileWithVersionInNameRemoveAction : IDeploymentComponent
	{
		private readonly ParallelReadableZipArchiveEntry entry;
		private readonly string targetPath;
		private readonly Version version;

		public FileWithVersionInNameRemoveAction(ParallelReadableZipArchiveEntry zipArchiveEntry, Version version, string targetPath)
		{
			entry = zipArchiveEntry;
			this.targetPath = targetPath;
			this.version = version;
		}

		public void Upgrade()
		{
			RemoveEntry(entry, targetPath);
		}

		private void RemoveEntry(ParallelReadableZipArchiveEntry entry, string targetPath)
		{
			var targetDirectory = Path.GetDirectoryName(targetPath);
			var targetFile = Path.GetFileNameWithoutExtension(targetPath) + version;
			var targetExtension = Path.GetExtension(targetPath);
			
			File.Delete(Path.Combine(targetDirectory, targetFile + targetExtension));
		}
	}
}
