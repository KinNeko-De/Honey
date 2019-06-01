using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HoneyLibrary.PackageLists
{
    public interface IPackageInfo
    {
        string PackageId { get; }

        string PackageVersion { get; }

		Nullable<DateTimeOffset> Created { get; }

		Nullable<DateTimeOffset> LastUpdated { get; }

		string LockedByAction { get; }

		string LockedByProcess { get; }
	}
}
