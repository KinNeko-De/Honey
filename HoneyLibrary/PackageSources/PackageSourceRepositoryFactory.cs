using HoneyLibrary.PackageRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HoneyLibrary.PackageSources
{
    public class PackageSourceRepositoryFactory : IPackageSourceRepositoryFactory
    {
        public IPackageSourceRepository CreateRepository(string packageSource)
        {
            Uri uri = new Uri(packageSource);
            if(uri.IsFile)
            {
                return new LocalPackageSourceRepository(uri.LocalPath);
			}
            else
            {
				return new DataServicePackageSourceRepository(uri);
            }
        }
    }
}
