using HoneyLibrary.PackageRepositories;
using HoneyLibrary.DeploymentActivities.Files;
using MyApplicationExample.Examples.VersionMigration;
using HoneyLibrary.PackageDeployment;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZipArchivExtensions;

namespace MyApplicationExample
{
    public class DeploymentComponentFactory : IDeploymentComponentFactory
    {
        private List<IDeploymentComponentStrategy> deploymentComponentStrategies = new List<IDeploymentComponentStrategy>();

        public DeploymentComponentFactory(IInstallLocation honeyInstallLocation, IInstallLocation applicationInstallLocation)
        {
            deploymentComponentStrategies.Add(new FileDeploymentComponentStrategy(applicationInstallLocation.GetInstallLocation(), "honeypackage/honeyfiles"));
			deploymentComponentStrategies.Add(new VersionExampleComponentStrategy(applicationInstallLocation.GetInstallLocation(), "honeypackage/versionexample"));
		}

        public IReadOnlyCollection<IDeploymentComponent> CreateDeploymentComponents(NugetPackage newPackage, NugetPackage installedPackage, IReadOnlyCollection<ParallelReadableZipArchiveEntry> installedEntries, IReadOnlyCollection<ParallelReadableZipArchiveEntry> newEntries)
        {
			foreach (var deploymentComponentStrategy in deploymentComponentStrategies)
			{
				deploymentComponentStrategy.BuildDelta(installedEntries, newEntries);
			}

			List<IDeploymentComponent> deploymentComponents = new List<IDeploymentComponent>();
			foreach (var deploymentComponentStrategy in deploymentComponentStrategies)
            {
                deploymentComponents.AddRange(deploymentComponentStrategy.CreateDeploymentComponents());
            }

            return deploymentComponents;
        }
        
    }
}
