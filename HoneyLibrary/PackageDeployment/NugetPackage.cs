using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using ZipArchivExtensions;

namespace HoneyLibrary.PackageDeployment
{
    public class NugetPackage
    {
        public static string GetPackageFileName(string id, Version version)
        {
            return $"{id}.{version}.nupkg";
        }

		private readonly string packagePathAndFileName;

        public NugetPackage(string packagePathAndFileName)
        {
            this.packagePathAndFileName = packagePathAndFileName;
        }

        public void Upgrade(IDeploymentComponentFactory deploymentComponentFactory, NugetPackage installedPackage)
		{
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();
			using (ParallelReadableZipArchive upgradeZipArchive = Open(packagePathAndFileName))
			{
				if(upgradeZipArchive == null)
				{
					throw new InvalidOperationException($"Source archive {packagePathAndFileName} not found.");
				}

				using (ParallelReadableZipArchive installedzipArchive = installedPackage?.Open())
				{
					IReadOnlyCollection<ParallelReadableZipArchiveEntry> installedEntries = installedzipArchive?.Entries;
					IReadOnlyCollection<ParallelReadableZipArchiveEntry> newEntries = upgradeZipArchive.Entries;

					IReadOnlyCollection<IDeploymentComponent> deploymentComponents = deploymentComponentFactory.CreateDeploymentComponents(this, installedPackage, installedEntries, newEntries);

			stopwatch.Stop();
			Console.WriteLine($"\tOpening, Calculating and planing needs { stopwatch.ElapsedMilliseconds } ms.");

			stopwatch.Restart();
					UpgradeDeploymentComponents(deploymentComponents, newEntries);
			stopwatch.Stop();
			Console.WriteLine($"\tExecution of deployment activities needs { stopwatch.ElapsedMilliseconds } ms.");
				}
			}
		}

		private void UpgradeDeploymentComponents(IReadOnlyCollection<IDeploymentComponent> deploymentComponents, IReadOnlyCollection<ParallelReadableZipArchiveEntry> newEntries)
		{
			// do it only parallel if there are enought entries so that it really makes sense to do it parallel, creating a new ziparchivereader needs around 80ms
			if(newEntries.Count < 10)
			{
				foreach (var deploymentComponent in deploymentComponents)
				{
					deploymentComponent.Upgrade();
				}
			}
			else {
				ParallelLoopResult parallelLopResult = Parallel.ForEach(deploymentComponents, (deploymentComponent) =>
				{
					deploymentComponent.Upgrade();
				});
			}
			
		}

		internal ParallelReadableZipArchive Open()
        {
            return Open(packagePathAndFileName);
        }

        private ParallelReadableZipArchive Open(string packagePathAndFileName)
        {
            if (string.IsNullOrWhiteSpace(packagePathAndFileName) | !File.Exists(packagePathAndFileName))
            {
                // TODO return null object for better handling later
                return null;
            }

            return new ParallelReadableZipArchive(packagePathAndFileName);
        }
    }
}
