using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HoneyLibrary.PackageDeployment
{
    public interface INugetFileDownloader
    {
        NugetPackage StorePackage(string packageId, Version packageVersion, string downloadUri, string targetDirectory);
    }
}
