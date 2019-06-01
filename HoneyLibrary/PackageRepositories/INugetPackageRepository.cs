using HoneyLibrary.PackageDeployment;
using System;

namespace HoneyLibrary.PackageRepositories
{
    public interface INugetPackageRepository
    {
        NugetPackage InstallPackage(string packageId, Version version, string source);

        NugetPackage ReadInstalledPackage(string packageId);

        void DeletePackage(string packageId);

        void ArchivePackage(string packageId);
    }
}