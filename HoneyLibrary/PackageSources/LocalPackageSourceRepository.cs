using HoneyLibrary.PackageRepositories;
using HoneyLibrary.PackageDeployment;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HoneyLibrary.PackageSources
{
	public class LocalPackageSourceRepository : IPackageSourceRepository
	{
		readonly string physicalPath;

		public LocalPackageSourceRepository(string physicalPath)
		{
			this.physicalPath = physicalPath;
		}

		public IPackageSource FindPackage(string packageId, Version packageVersion)
		{
			string packageFile = Path.Combine(physicalPath, NugetPackage.GetPackageFileName(packageId, packageVersion));

			return new LocalPackageSource(packageFile);
		}
	}
}
