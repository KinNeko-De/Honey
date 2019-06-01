using HoneyLibrary.PackageDeployment;
using HoneyLibrary.PackageLists;
using HoneyLibrary.PackageRepositories;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HoneyLibrary.Controller
{
    public class DeploymentController: IDeploymentController
    {
        private readonly IDeploymentComponentFactory deploymentComponentFactory;
        private INugetPackageRepository nugetPackageRepository;
        private readonly IPackageListRepository packageListRepository;

        public DeploymentController(
            IDeploymentComponentFactory deploymentComponentFactory,
            INugetPackageRepository nugetPackageRepository,
            IPackageListRepository packageListRepository)
        {
            this.deploymentComponentFactory = deploymentComponentFactory;
            this.nugetPackageRepository = nugetPackageRepository;
            this.packageListRepository = packageListRepository;
        }

		/*
		* In case of error during uprading return a modified nuget package
		* assume that that everything that should be removed is still there
		* in case of upgrade it might be harder because we can not be sure what happend before.
		* but the deployment action must handle this!
		*/
		public void Upgrade(string packageId, Version packageVersion, string packageDownloadUri)
        {
            Stopwatch stopwatch = new Stopwatch();

			stopwatch.Restart();
			packageListRepository.StartActionOnPackage(packageId, packageVersion);
			stopwatch.Stop();
			Console.WriteLine($"starting action on xml list needs {stopwatch.ElapsedMilliseconds} ms.");

			stopwatch.Restart();
            NugetPackage upgradePackage = nugetPackageRepository.InstallPackage(packageId, packageVersion, packageDownloadUri);
            stopwatch.Stop();
            Console.WriteLine($"installing new package needs {stopwatch.ElapsedMilliseconds} ms.");

            stopwatch.Restart();
            NugetPackage installedPackage = nugetPackageRepository.ReadInstalledPackage(packageId);
            stopwatch.Stop();
            Console.WriteLine($"reading installed package needs {stopwatch.ElapsedMilliseconds} ms.");

            stopwatch.Restart();
            upgradePackage.Upgrade(deploymentComponentFactory, installedPackage);
            stopwatch.Stop();
            Console.WriteLine($"upgrade needs {stopwatch.ElapsedMilliseconds} ms.");

            stopwatch.Restart();
            nugetPackageRepository.ArchivePackage(packageId);
            stopwatch.Stop();
            Console.WriteLine($"archive needs {stopwatch.ElapsedMilliseconds} ms.");

            stopwatch.Restart();
            packageListRepository.EndActionOnPackage(packageId, packageVersion);
            stopwatch.Stop();
            Console.WriteLine($"finishing action on xml list needs {stopwatch.ElapsedMilliseconds} ms.");
        }
    }
}
