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
    public class FileDeploymentComponentStrategy : IDeploymentComponentStrategy
    {
        List<IDeploymentComponent> deploymentComponents = new List<IDeploymentComponent>();
        private readonly string applicationInstallationDir;
        private readonly string fileDeploymentKey;

        public FileDeploymentComponentStrategy(string applicationInstallationDir, string fileDeploymentKey)
        {
            this.applicationInstallationDir = applicationInstallationDir;
            this.fileDeploymentKey = fileDeploymentKey;
        }

		public void BuildDelta(IReadOnlyCollection<ParallelReadableZipArchiveEntry> installedEntries, IReadOnlyCollection<ParallelReadableZipArchiveEntry> newEntries)
		{
			var deltaStrategy = new DeltaWithoutVersion(fileDeploymentKey, AddUpgradableEntry, AddNewEntry, AddRemovedEntry);
			deltaStrategy.BuildDelta(installedEntries, newEntries);
		}

		private void AddUpgradableEntry(ParallelReadableZipArchiveEntry newEntry, string newEntryRelativeFullName, ParallelReadableZipArchiveEntry installedEntry, string installedEntryRelativeFullName)
		{
             var targetPath = GetTargetPath(newEntryRelativeFullName);
             deploymentComponents.Add(new FileUpgradeAction(newEntry, targetPath));
        }

        public void AddNewEntry(ParallelReadableZipArchiveEntry newEntry, string newEntryRelativeFullName)
        {
			var targetPath = GetTargetPath(newEntryRelativeFullName);
            deploymentComponents.Add(new FileUpgradeAction(newEntry, targetPath));
        }

        public void AddRemovedEntry(ParallelReadableZipArchiveEntry removedEntry, string removedEntryRelativeFullName)
        {
            var targetPath = GetTargetPath(removedEntryRelativeFullName);
            deploymentComponents.Add(new FileRemoveAction(removedEntry, targetPath));
        }

        public IReadOnlyCollection<IDeploymentComponent> CreateDeploymentComponents()
        {
            return deploymentComponents;
        }

		public string GetTargetPath(string entryRelativeFullName)
        {
            var targetPath = Path.Combine(applicationInstallationDir, entryRelativeFullName);
            return targetPath;
        }
	}
}
