using System;
using System.Diagnostics;
using System.IO;
using MyApplicationExample;
using HoneyLibrary.PackageDeployment;
using NUnit.Framework;
using Assert = NUnit.Framework.Assert;

namespace MyApplicationExampleTest.Examples.PackageDeployment
{
    public class PackageDeploymentTest
    {
		const string bigFilesPackages = "honeypackagefiles.1.0.0.nupkg";
		const string bigFilesPackages2 = "honeypackagefiles.1.0.1.nupkg";

		private string sourcePath;
		private string targetPath;

		private readonly string namespaceForEmbbededResources = typeof(PackageDeploymentTest).Namespace + ".TestData";

		[SetUp]
		public void CreateTestDirectory()
		{
			sourcePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			targetPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
			Directory.CreateDirectory(sourcePath);
			Directory.CreateDirectory(targetPath);
		}

		[TearDown]
		public void DeleteTestDirectory()
		{
			Directory.Delete(sourcePath, true);
			Directory.Delete(targetPath, true);
		}

        [Test]
        public void PackageUpgrade_PackageIsNotInstalled()
        {
			NugetPackage installedPackage = null;
			NugetPackage newPackage = new NugetPackage(Path.Combine(sourcePath, bigFilesPackages));

			EmbeddedResourceExtensions.CopyEmbeddedResource(namespaceForEmbbededResources, bigFilesPackages, sourcePath);

			Stopwatch stopwatch = Stopwatch.StartNew();
			newPackage.Upgrade(new DeploymentComponentFactory(new HoneyInstallLocation(), new PathInstallLocation(targetPath)), installedPackage);
            stopwatch.Stop();
            Console.WriteLine($"Install takes {stopwatch.ElapsedMilliseconds } ms.");

			Assert.That(File.Exists(Path.Combine(targetPath, "dhbx2w3k.3tc")), Is.True);
        }

		[Test]
		public void PackageUpgrade_PackageIsNotChanged()
		{
			NugetPackage installedPackage = null;
			NugetPackage newPackage = new NugetPackage(Path.Combine(sourcePath, bigFilesPackages));

			EmbeddedResourceExtensions.CopyEmbeddedResource(namespaceForEmbbededResources, bigFilesPackages, sourcePath);

			newPackage.Upgrade(new DeploymentComponentFactory(new HoneyInstallLocation(), new PathInstallLocation(targetPath)), installedPackage);
			installedPackage = newPackage;

			Console.WriteLine("Begin of performance calculation");
			Stopwatch stopwatch = Stopwatch.StartNew();
			newPackage.Upgrade(new DeploymentComponentFactory(new HoneyInstallLocation(), new PathInstallLocation(targetPath)), installedPackage);
			stopwatch.Stop();
			Console.WriteLine($"Upgrade with nothing has changed takes {stopwatch.ElapsedMilliseconds } ms.");

			Assert.That(File.Exists(Path.Combine(targetPath, "dhbx2w3k.3tc")), Is.True);
		}

		[Test]
		public void PackageUpgrade_PackageIsChanged()
		{
			NugetPackage installedPackage = null;
			NugetPackage newPackage = new NugetPackage(Path.Combine(sourcePath, bigFilesPackages));

			EmbeddedResourceExtensions.CopyEmbeddedResource(namespaceForEmbbededResources, bigFilesPackages, sourcePath);
			EmbeddedResourceExtensions.CopyEmbeddedResource(namespaceForEmbbededResources, bigFilesPackages2, sourcePath);

			newPackage.Upgrade(new DeploymentComponentFactory(new HoneyInstallLocation(), new PathInstallLocation(targetPath)), installedPackage);
			installedPackage = newPackage;

			NugetPackage newPackage2 = new NugetPackage(Path.Combine(sourcePath, bigFilesPackages2));
			Console.WriteLine("Begin of performance calculation");
			Stopwatch stopwatch = Stopwatch.StartNew();
			newPackage2.Upgrade(new DeploymentComponentFactory(new HoneyInstallLocation(), new PathInstallLocation(targetPath)), installedPackage);
			stopwatch.Stop();
			Console.WriteLine($"Upgrade with one file removed and one added takes {stopwatch.ElapsedMilliseconds } ms.");

			Assert.That(File.Exists(Path.Combine(targetPath, "dhbx2w3k.3tc")), Is.False);
			Assert.That(File.Exists(Path.Combine(targetPath, "dhbx2w3k.4tc")), Is.True);
		}
	}
}
