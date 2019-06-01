using System;
using System.Collections.Generic;
using System.Linq;
using ZipArchivExtensions;

namespace HoneyLibrary.DeploymentActivities
{
	public class DeltaWithVersion
	{
		private readonly string fileDeploymentKey;
		private readonly Action<ParallelReadableZipArchiveEntry, Version, string, ParallelReadableZipArchiveEntry, Version, string> addUpgradableEntry;
		private readonly Action<ParallelReadableZipArchiveEntry, Version, string> addNewEntry;
		private readonly Action<ParallelReadableZipArchiveEntry, Version, string> addRemovedEntry;

		public DeltaWithVersion(string fileDeploymentKey, Action<ParallelReadableZipArchiveEntry, Version, string, ParallelReadableZipArchiveEntry, Version, string> addUpgradableEntry, Action<ParallelReadableZipArchiveEntry, Version, string> addNewEntry, Action<ParallelReadableZipArchiveEntry, Version, string> addRemovedEntry)
		{
			this.fileDeploymentKey = fileDeploymentKey;
			this.addUpgradableEntry = addUpgradableEntry;
			this.addNewEntry = addNewEntry;
			this.addRemovedEntry = addRemovedEntry;
		}

		public void BuildDelta(IReadOnlyCollection<ParallelReadableZipArchiveEntry> installedEntries, IReadOnlyCollection<ParallelReadableZipArchiveEntry> newEntries)
		{
			var responsibleNewEntries = newEntries.Where(x => ResponsibleFor(x));

			Dictionary<string, ParallelReadableZipArchiveEntry> responsibleInstalledEntries;
			if (installedEntries != null)
			{
				responsibleInstalledEntries = installedEntries.Where(x => ResponsibleFor(x)).ToDictionary(x => GetRelativePathWithOutKeyAndVersion(x, fileDeploymentKey, GetVersion(x)));
			}
			else
			{
				responsibleInstalledEntries = new Dictionary<string, ParallelReadableZipArchiveEntry>();
			}

			foreach (var newEntry in responsibleNewEntries)
			{
				var newVersion = GetVersion(newEntry);
				var newEntryRelativeFullName = GetRelativePathWithOutKeyAndVersion(newEntry, fileDeploymentKey, newVersion);

				ParallelReadableZipArchiveEntry installedEntry;
				if (responsibleInstalledEntries.TryGetValue(newEntryRelativeFullName, out installedEntry))
				{
					var installedVersion = GetVersion(installedEntry);
					var installedEntryRelativeFullName = GetRelativePathWithOutKeyAndVersion(installedEntry, fileDeploymentKey, installedVersion);
					addUpgradableEntry(newEntry, newVersion, newEntryRelativeFullName, installedEntry, installedVersion, installedEntryRelativeFullName);
					responsibleInstalledEntries.Remove(newEntryRelativeFullName);
				}
				else
				{
					addNewEntry(newEntry, newVersion, newEntryRelativeFullName);
				}
			}

			foreach (var removedEntry in responsibleInstalledEntries.Values)
			{
				var removedVersion = GetVersion(removedEntry);
				var removedEntryRelativeFullName = GetRelativePathWithOutKeyAndVersion(removedEntry, fileDeploymentKey, removedVersion);
				addRemovedEntry(removedEntry, removedVersion, removedEntryRelativeFullName);
			}

		}

		private bool ResponsibleFor(ParallelReadableZipArchiveEntry entry)
		{
			return entry.FullName.StartsWith(fileDeploymentKey, StringComparison.OrdinalIgnoreCase);
		}

		public string GetRelativePathWithOutKeyAndVersion(ParallelReadableZipArchiveEntry entry, string key, Version version)
		{
			string relativePath = entry.FullName.Remove(0, key.Length + 1 + version.ToString().Length + 1);
			return relativePath;
		}

		public Version GetVersion(ParallelReadableZipArchiveEntry entry)
		{
			int startIndex = entry.FullName.IndexOf(fileDeploymentKey + "/") + fileDeploymentKey.Length + 1;
			int endIndex = entry.FullName.IndexOf("/", startIndex);
			string version = entry.FullName.Substring(startIndex, endIndex - startIndex);
			return new Version(version);
		}
	}
}
