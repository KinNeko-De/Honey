using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HoneyLibrary.PackageLists
{
    public interface IPackageListRepository
    {
		void StartActionOnPackage(string packageId, Version version);

        void EndActionOnPackage(string packageId, Version version);

		IPackageInfo GetPackageInfo(string packageId, ListMode listMode);

	}
}
