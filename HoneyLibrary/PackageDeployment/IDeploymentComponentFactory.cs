using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZipArchivExtensions;

namespace HoneyLibrary.PackageDeployment
{
    public interface IDeploymentComponentFactory
    {
        // TODO replace nugetpackage with nugetpackageinformation that is extracted from the ziparchive. for debugging and logging
        IReadOnlyCollection<IDeploymentComponent> CreateDeploymentComponents(NugetPackage newPackage, NugetPackage installedPackage, IReadOnlyCollection<ParallelReadableZipArchiveEntry> installedEntries, IReadOnlyCollection<ParallelReadableZipArchiveEntry> newEntries);
    }
}