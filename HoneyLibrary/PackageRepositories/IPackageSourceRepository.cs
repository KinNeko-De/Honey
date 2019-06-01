using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HoneyLibrary.PackageRepositories
{
	public interface IPackageSourceRepository
	{
		IPackageSource FindPackage(string packageId, Version packageVersion);
	}
}
