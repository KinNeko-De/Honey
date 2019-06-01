using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HoneyLibrary.PackageLists
{
	public class PackageInfo : IPackageInfo
	{
		public string PackageId { get; internal set; }

		public string PackageVersion { get; internal set; }

		public Nullable<DateTimeOffset> Created { get; internal set; }

		public Nullable<DateTimeOffset> LastUpdated { get; internal set; }

		public string LockedByAction { get; internal set; }

		public string LockedByProcess { get; internal set; }
	}
}
