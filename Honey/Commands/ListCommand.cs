using HoneyLibrary.Controller;
using HoneyLibrary.PackageLists;
using MyApplicationExample;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Honey.Commands
{
	public class ListCommand
	{
		public ListCommand()
		{
		}

		public void Execute(string[] args)
		{
			// no logger needed..it needs 50-70ms

			IPackageListRepository packageListRepository = new PackageListRepository(new HoneyInstallLocation());
			IListController deploymentController = new ListController(packageListRepository);
			if(args.Length < 2)
			{
				throw new ArgumentException("Please add the specific id of the package you search", "packageId");
			}
			var packageId = args[1];

			// limitOutput is more for programms, programms can easy add arguments, dont bother people to do that
			ListMode listMode = ListMode.Full;
			if(args.Length > 2)
			{
				listMode = ListMode.LimitOutput;
			}
			var packageInfo = deploymentController.GetPackageInfo(packageId, listMode);
			if (packageInfo != null)
			{
				string output = null;
				switch (listMode)
				{
					case ListMode.Full:
						output = $@"{nameof(packageInfo.PackageId)}: {packageInfo.PackageId}
{nameof(packageInfo.PackageVersion)}: {packageInfo.PackageVersion}
{nameof(packageInfo.LockedByAction)}: {packageInfo.LockedByAction}
{nameof(packageInfo.LockedByProcess)}: {packageInfo.LockedByProcess}
{nameof(packageInfo.Created)}: {packageInfo.Created}
{nameof(packageInfo.LastUpdated)}: {packageInfo.LastUpdated}";
						break;
					case ListMode.LimitOutput:
						output = $"{packageInfo.PackageId}|{packageInfo.PackageVersion}";
						break;
					default:
						throw new ArgumentOutOfRangeException(nameof(listMode));
				}
				Console.WriteLine(output);
			}
			else
			{
				Console.WriteLine($"no package with id '{packageId}' found");
			}
		}
	}
}
