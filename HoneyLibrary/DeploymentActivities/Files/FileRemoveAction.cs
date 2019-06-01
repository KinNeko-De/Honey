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
    public class FileRemoveAction : IDeploymentComponent
    {
        readonly ParallelReadableZipArchiveEntry entry;
        readonly string targetPath;

        public FileRemoveAction(ParallelReadableZipArchiveEntry zipArchiveEntry, string targetPath)
        {
            entry = zipArchiveEntry;
            this.targetPath = targetPath;
        }

        public void Upgrade()
        {
            RemoveEntry(entry, targetPath);
        }

        private void RemoveEntry(ParallelReadableZipArchiveEntry entry, string targetPath)
        {
            File.Delete(targetPath);
        }
    }
}
