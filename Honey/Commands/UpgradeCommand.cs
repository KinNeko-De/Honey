using MyApplicationExample;
using HoneyLibrary.Controller;
using HoneyLibrary.PackageDeployment;
using HoneyLibrary.PackageLists;
using HoneyLibrary.PackageRepositories;
using HoneyLibrary.PackageSources;
using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Honey.Commands
{
    public class UpgradeCommand
    {
        public void Execute(string[] args)
        {
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Restart();
			ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
			log.Info("It just works");
			stopwatch.Stop();
			Console.WriteLine($"logger and logging 1 message (file and console) needs {stopwatch.ElapsedMilliseconds} ms.");

			string packageId;
			Version packageVersion;
			string packageSource;
			string applicationInstallationDir;

			switch (args[1])
			{
				case "--filepath":
					packageSource = Path.GetDirectoryName(args[2]);
					string nugetPackage = Path.GetFileNameWithoutExtension(args[2]);
					var match = System.Text.RegularExpressions.Regex.Match(nugetPackage, @"(.*)\.([0-9]+\.[0-9]+\.[0-9]+\.[0-9]+)");
					packageId = match.Groups[1].Value;
					packageVersion = new Version(match.Groups[2].Value);

					try
					{
						applicationInstallationDir = args[4];
					}
					catch (IndexOutOfRangeException e)
					{
						throw new InvalidOperationException("Target directory was not specified. it will be read from registry later but now you have to add an argument for it :)", e);
					}
					break;
				default:
					packageId = args[1];
					packageVersion = new Version(args[2]);
					packageSource = args[3];
					try
					{
						applicationInstallationDir = args[4];
					}
					catch (IndexOutOfRangeException e)
					{
						throw new InvalidOperationException("Target directory was not specified. it will be read from registry later but now you have to add an argument for it :)", e);
					}
					break;
			}
			HoneyInstallLocation honeyInstallLocation = new HoneyInstallLocation();
			IDeploymentComponentFactory deploymentComponentFactory = new DeploymentComponentFactory(honeyInstallLocation, new PathInstallLocation(applicationInstallationDir));

			INugetPackageRepository nugetPackageRepository = new NugetPackageRepository(new NugetPackageRepositoryConfig(honeyInstallLocation), new PackageSourceRepositoryFactory());
			IPackageListRepository packageListRepository = new PackageListRepository(new HoneyInstallLocation());
			IDeploymentController deploymentController = new DeploymentController(deploymentComponentFactory, nugetPackageRepository, packageListRepository);

            deploymentController.Upgrade(packageId, packageVersion, packageSource);
        }

		// only for demo because we do not really want to install
		private class PathInstallLocation : IInstallLocation
		{
			private readonly string path;

			public PathInstallLocation(string path)
			{
				this.path = path;
			}

			public string GetInstallLocation()
			{
				return path;
			}
		}
	}
}
