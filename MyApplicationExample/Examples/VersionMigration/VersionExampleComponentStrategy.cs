using HoneyLibrary.PackageDeployment;
using HoneyLibrary.DeploymentActivities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZipArchivExtensions;

namespace MyApplicationExample.Examples.VersionMigration
{
	public class VersionExampleComponentStrategy : IDeploymentComponentStrategy
	{
		List<IDeploymentComponent> deploymentComponents = new List<IDeploymentComponent>();
		private readonly string applicationInstallationDir;
		private readonly string fileDeploymentKey;

		public VersionExampleComponentStrategy(string applicationInstallationDir, string fileDeploymentKey)
		{
			this.applicationInstallationDir = applicationInstallationDir;
			this.fileDeploymentKey = fileDeploymentKey;

		}

		public void BuildDelta(IReadOnlyCollection<ParallelReadableZipArchiveEntry> installedEntries, IReadOnlyCollection<ParallelReadableZipArchiveEntry> newEntries)
		{
			var deltaStrategy = new DeltaWithVersion(fileDeploymentKey, AddUpgradableEntry, AddNewEntry, AddRemovedEntry);
			deltaStrategy.BuildDelta(installedEntries, newEntries);
		}

		private void AddUpgradableEntry(ParallelReadableZipArchiveEntry newEntry, Version newVersion, string newEntryRelativeFullName, ParallelReadableZipArchiveEntry installedEntry, Version installedVersion, string installedEntryRelativeFullName)
		{
			var targetPath = GetTargetPath(newEntryRelativeFullName);
			deploymentComponents.Add(new FileWithVersionInNameUpgradeAction(installedEntry, installedVersion, newEntry, newVersion, targetPath));
		}

		private void AddNewEntry(ParallelReadableZipArchiveEntry newEntry, Version newVersion, string newEntryRelativeFullName)
		{
			var targetPath = GetTargetPath(newEntryRelativeFullName);
			deploymentComponents.Add(new FileWithVersionInNameInstallAction(newEntry, newVersion, targetPath));
		}

		private void AddRemovedEntry(ParallelReadableZipArchiveEntry removedEntry, Version removedVersion, string removedEntryRelativeFullName)
		{
			var targetPath = GetTargetPath(removedEntryRelativeFullName);
			deploymentComponents.Add(new FileWithVersionInNameRemoveAction(removedEntry, removedVersion, targetPath));
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
