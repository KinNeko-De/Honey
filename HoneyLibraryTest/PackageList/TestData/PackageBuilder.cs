using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HoneyLibraryTest.PackageList.TestData
{
	internal class PackageBuilder
	{
		const string placeholderPackageId = "{{PackageId}}";
		const string placeholderPackageVersion = "{{PackageVersion}}";
		const string placeholderLockedByAction = "{{PackageLockedByAction}}";
		const string placeholderLockedByProcess = "{{PackageLockedByProcess}}";
		const string placeholderCreated = "{{PackageCreated}}";
		const string placeholderLastUpdated = "{{PackageLastUpdated}}";

		readonly string content = $@"<Package>
	<Id>{placeholderPackageId}</Id>
	<Version>{placeholderPackageVersion}</Version>
	<LockedByAction>{placeholderLockedByAction}</LockedByAction>
	<LockedByProcess>{placeholderLockedByProcess}</LockedByProcess>
	<Created>{placeholderCreated}</Created>
	<LastUpdated>{placeholderLastUpdated}</LastUpdated>
</Package>";

		public string PackageId { get; private set; } = "myTestPackage";
		public string PackageVersion { get; private set; } = "1.2.3.4";
		public string PackageLockedByAction { get; private set; } = null;
		public string PackageLockedByProcess { get; private set; } = null;
		public Nullable<DateTimeOffset> PackageCreated { get; private set; } = new DateTimeOffset(2019, 1, 1, 12, 0, 1, TimeSpan.FromHours(2));
		public Nullable<DateTimeOffset> PackageLastUpdated { get; private set; } = null;

		public PackageBuilder SetPackageId(string packageId)
		{
			this.PackageId = packageId;
			return this;
		}

		public PackageBuilder SetPackageVersion(string packageVersion)
		{
			this.PackageVersion = packageVersion;
			return this;
		}

		public PackageBuilder SetLockedByAction(string action)
		{
			this.PackageLockedByAction = action;
			return this;
		}

		public PackageBuilder SetLockedByProcess(string processId)
		{
			this.PackageLockedByProcess = processId;
			return this;
		}

		public PackageBuilder SetCreated(Nullable<DateTimeOffset> created)
		{
			this.PackageCreated = created;
			return this;
		}

		public PackageBuilder SetLastUpdated(Nullable<DateTimeOffset> lastUpdated)
		{
			this.PackageLastUpdated = lastUpdated;
			return this;
		}

		public string Build()
		{
			StringBuilder stringBuilder = new StringBuilder(content);
			stringBuilder.Replace(placeholderPackageId, PackageId);
			stringBuilder.Replace(placeholderPackageVersion, PackageVersion);
			stringBuilder.Replace(placeholderLockedByAction, PackageLockedByAction);
			stringBuilder.Replace(placeholderLockedByProcess, PackageLockedByProcess);
			stringBuilder.Replace(placeholderCreated, PackageCreated.ToString());
			stringBuilder.Replace(placeholderLastUpdated, PackageLastUpdated.ToString());
			return stringBuilder.ToString();
		}
	}
}
