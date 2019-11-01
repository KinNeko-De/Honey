using HoneyLibrary.Logging;
using HoneyLibrary.PackageDeployment;
using HoneyLibrary.PackageLists;
using HoneyLibrary.PackageRepositories;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HoneyLibrary.Controller
{
	public class DeploymentController : IDeploymentController
	{
		private readonly ILogger logger;
		private readonly IDeploymentComponentFactory deploymentComponentFactory;
		private INugetPackageRepository nugetPackageRepository;
		private readonly IPackageListRepository packageListRepository;

		public DeploymentController(
			ILogger logger,
			IDeploymentComponentFactory deploymentComponentFactory,
			INugetPackageRepository nugetPackageRepository,
			IPackageListRepository packageListRepository)
		{
			this.logger = logger;
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
			var loggerstate = new Dictionary<string, object>()
			{
				{ nameof(packageId), packageId },
				{ nameof(packageVersion), packageVersion },
				{ nameof(packageDownloadUri), packageDownloadUri },
			};

			logger.LogInformation("Upgrading package '{0}' to version '{1}'", packageId, packageVersion);

			using (logger.BeginScope(loggerstate))
			{
				PerformanceLogger performanceLogger = new PerformanceLogger(logger);

				logger.LogInformation("Starting action on package xml list.");
				performanceLogger.Restart("Starting action on package xml list.");
				packageListRepository.StartActionOnPackage(packageId, packageVersion);
				performanceLogger.Stop();

				logger.LogInformation("Installing new package.");
				performanceLogger.Restart("Installing new package.");
				NugetPackage upgradePackage = nugetPackageRepository.InstallPackage(packageId, packageVersion, packageDownloadUri);
				performanceLogger.Stop();

				logger.LogInformation("Reading installed package.");
				performanceLogger.Restart("Reading installed package.");
				NugetPackage installedPackage = nugetPackageRepository.ReadInstalledPackage(packageId);
				logger.LogInformation("Installed Package: {0}", installedPackage?.PackageIdentifier);
				performanceLogger.Stop();

				logger.LogInformation("Upgrading package.");
				performanceLogger.Restart("Upgrading package.");
				upgradePackage.Upgrade(logger, deploymentComponentFactory, installedPackage);
				performanceLogger.Stop();

				logger.LogInformation("Archiving package.");
				performanceLogger.Restart("Archiving package.");
				nugetPackageRepository.ArchivePackage(packageId);
				performanceLogger.Stop();

				logger.LogInformation("Ending action on package xml list.");
				performanceLogger.Restart("Ending action on package xml list.");
				packageListRepository.EndActionOnPackage(packageId, packageVersion);
				performanceLogger.Stop();
			}
		}
	}
}
