using HoneyLibrary.PackageRepositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HoneyLibrary.PackageSources
{
	internal class LocalPackageSource : IPackageSource
	{
		private readonly string physicalPath;

		public LocalPackageSource(string physicalPath)
		{
			this.physicalPath = physicalPath;
		}

		public void Copy(string targetPath)
		{
			File.Copy(physicalPath, targetPath, true);
		}
	}
}
