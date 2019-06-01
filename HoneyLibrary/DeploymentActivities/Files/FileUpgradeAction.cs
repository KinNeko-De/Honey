using HoneyLibrary.PackageDeployment;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZipArchivExtensions;

namespace HoneyLibrary.DeploymentActivities.Files
{
    public class FileUpgradeAction : IDeploymentComponent
    {
		readonly ParallelReadableZipArchiveEntry entry;
		readonly string rootPathOfApplication;

        public FileUpgradeAction(ParallelReadableZipArchiveEntry zipArchiveEntry, string targetPath)
        {
            entry = zipArchiveEntry;
            this.rootPathOfApplication = targetPath;
        }

        public void Upgrade()
        {
            UpgradeEntry(entry, rootPathOfApplication);
        }

        private void UpgradeEntry(ParallelReadableZipArchiveEntry entry, string targetPath)
        {
            if (File.Exists(targetPath))
            {
                var existingFileLastWriteTime = File.GetLastWriteTimeUtc(targetPath);
                var entryLastWriteTime = entry.LastWriteTime.UtcDateTime;
                if (existingFileLastWriteTime.Equals(entryLastWriteTime))
                {
                    return;
                }
            }

            InstallEntry(entry, targetPath);
        }

        private void InstallEntry(ParallelReadableZipArchiveEntry entry, string targetPath)
        {
            var directoryName = Path.GetDirectoryName(targetPath);
            Directory.CreateDirectory(directoryName);
            entry.ExtractToFile(targetPath, true);
        }
    }
}
