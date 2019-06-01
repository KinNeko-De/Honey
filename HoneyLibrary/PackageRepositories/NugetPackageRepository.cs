using HoneyLibrary.PackageDeployment;
using System;
using System.IO;
using System.Linq;

namespace HoneyLibrary.PackageRepositories
{
	public class NugetPackageRepository : INugetPackageRepository
    {
        private readonly IPackageSourceRepositoryFactory packageSourceRepositoryFactory;
        private readonly INugetPackageRepositoryConfig packageRepositoryConfig;

        public NugetPackageRepository(
            INugetPackageRepositoryConfig nugetPackageRepositoryConfig,
			IPackageSourceRepositoryFactory packageSourceRepositoryFactory)
        {
            this.packageRepositoryConfig = nugetPackageRepositoryConfig;
            this.packageSourceRepositoryFactory = packageSourceRepositoryFactory;
        }

        private static void ClearTargetPathInCaseOfPreviousFailedInstallation(string targetPath)
        {
            if (Directory.Exists(targetPath))
            {
                Directory.Delete(targetPath, true);
            }
        }

        public NugetPackage InstallPackage(string packageId, Version packageVersion, string downloadUri)
        {
            string installPath = GetInstallPathForPackage(packageId);
            DirectoryInfo targetDirectory = Directory.CreateDirectory(installPath);
			string targetPath = Path.Combine(installPath, NugetPackage.GetPackageFileName(packageId, packageVersion));

            IPackageSourceRepository packageSourceRepository = packageSourceRepositoryFactory.CreateRepository(downloadUri);
			IPackageSource packageSource = packageSourceRepository.FindPackage(packageId, packageVersion);
			packageSource.Copy(targetPath);

			NugetPackage installedPackage = new NugetPackage(targetPath);

            return installedPackage;
        }

        public NugetPackage ReadInstalledPackage(string packageId)
        {
            string archivedPath = GetArchivePathForPackage(packageId);
            if (!Directory.Exists(archivedPath))
            {
                return null;
            }
            string fileName = Directory.GetFiles(archivedPath, "*.nupkg", SearchOption.TopDirectoryOnly).Single();
            return new NugetPackage(fileName);
        }

        public void DeletePackage(string packageId)
        {
            string installedPath = GetArchivePathForPackage(packageId);
            Directory.Delete(installedPath, true);
        }

        public void ArchivePackage(string packageId)
        {
            string installedPath = GetInstallPathForPackage(packageId);
            if (!Directory.Exists(installedPath))
            {
                throw new InvalidOperationException($"Installed package {packageId} not found in '{installedPath}'");
            }

            string archivePath = GetArchivePathForPackage(packageId);

            if (Directory.Exists(archivePath))
            {
                Directory.Delete(archivePath, true);
            }

            Directory.Move(installedPath, archivePath);
        }

        private string GetInstallPathForPackage(string packageId)
        {
            return Path.Combine(packageRepositoryConfig.PackagesInstallDirectory.FullName, packageId);
        }

        private string GetArchivePathForPackage(string packageId)
        {
            return Path.Combine(packageRepositoryConfig.PackagesLibraryDirectory.FullName, packageId);
        }
    }
}