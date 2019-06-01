using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HoneyLibrary.PackageRepositories
{
    public interface INugetPackageRepositoryConfig
    {
        DirectoryInfo PackagesDirectory { get; }
        DirectoryInfo PackagesInstallDirectory { get; }
        DirectoryInfo PackagesLibraryDirectory { get; }
    }
}
