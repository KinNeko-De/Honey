using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HoneyLibrary.PackageRepositories
{
    public class NugetPackageRepositoryConfig : INugetPackageRepositoryConfig
    {
        public NugetPackageRepositoryConfig(IInstallLocation honeyInstallLocation)
        {
            PackagesDirectory = new DirectoryInfo(honeyInstallLocation.GetInstallLocation());
            PackagesLibraryDirectory = new DirectoryInfo(Path.Combine(PackagesDirectory.FullName, "lib"));
            PackagesInstallDirectory = new DirectoryInfo(Path.Combine(PackagesDirectory.FullName, "ins"));
        }

        public DirectoryInfo PackagesDirectory { get; }
        public DirectoryInfo PackagesLibraryDirectory { get; }
        public DirectoryInfo PackagesInstallDirectory { get; }
    }
}
