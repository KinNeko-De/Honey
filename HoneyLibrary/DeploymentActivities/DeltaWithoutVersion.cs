using System;
using System.Collections.Generic;
using System.Linq;
using ZipArchivExtensions;

namespace HoneyLibrary.DeploymentActivities
{
	public class DeltaWithoutVersion
	{
		private readonly string fileDeploymentKey;
		private readonly Action<ParallelReadableZipArchiveEntry, string, ParallelReadableZipArchiveEntry, string> addUpgradableEntry;
		private readonly Action<ParallelReadableZipArchiveEntry, string> addNewEntry;
		private readonly Action<ParallelReadableZipArchiveEntry, string> addRemovedEntry;

		public DeltaWithoutVersion(string fileDeploymentKey, Action<ParallelReadableZipArchiveEntry, string, ParallelReadableZipArchiveEntry, string> addUpgradableEntry, Action<ParallelReadableZipArchiveEntry, string> addNewEntry, Action<ParallelReadableZipArchiveEntry, string> addRemovedEntry)
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
				responsibleInstalledEntries = installedEntries.Where(x => ResponsibleFor(x)).ToDictionary(x => GetRelativePathWithOutKey(x, fileDeploymentKey));
			}
			else
			{
				responsibleInstalledEntries = new Dictionary<string, ParallelReadableZipArchiveEntry>();
			}

			foreach (var newEntry in responsibleNewEntries)
			{
				var newEntryRelativeFullName = GetRelativePathWithOutKey(newEntry, fileDeploymentKey);

				ParallelReadableZipArchiveEntry installedEntry;
				if (responsibleInstalledEntries.TryGetValue(newEntryRelativeFullName, out installedEntry))
				{
					var installedEntryRelativeFullName = GetRelativePathWithOutKey(installedEntry, fileDeploymentKey);
					addUpgradableEntry(newEntry, newEntryRelativeFullName, installedEntry, installedEntryRelativeFullName);
					responsibleInstalledEntries.Remove(newEntryRelativeFullName);
				}
				else
				{
					addNewEntry(newEntry, newEntryRelativeFullName);
				}
			}

			foreach (var removedEntry in responsibleInstalledEntries.Values)
			{
				var removedEntryRelativeFullName = GetRelativePathWithOutKey(removedEntry, fileDeploymentKey);
				addRemovedEntry(removedEntry, removedEntryRelativeFullName);
			}
		}

		private bool ResponsibleFor(ParallelReadableZipArchiveEntry entry)
		{
			return entry.FullName.StartsWith(fileDeploymentKey, StringComparison.OrdinalIgnoreCase);
		}

		public string GetRelativePathWithOutKey(ParallelReadableZipArchiveEntry entry, string key)
		{
			string relativePath = entry.FullName.Remove(0, key.Length + 1);
			return relativePath;
		}
	}
}