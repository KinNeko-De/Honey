using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace HoneyLibrary.PackageLists
{
	internal class PackageInfo : IPackageInfo
	{
		internal XElement packageListInfoElement;

		internal PackageInfo(XElement packageListInfoElement)
		{
			this.packageListInfoElement = packageListInfoElement;
		}

		public string PackageId { get; internal set; }

		public string PackageVersion { get; internal set; }

		public Nullable<DateTimeOffset> Created { get; internal set; }

		public Nullable<DateTimeOffset> LastUpdated { get; internal set; }

		public string LockedByAction { get; internal set; }

		public string LockedByProcess { get; internal set; }
		public XElement XElement { get; }
	}
}
