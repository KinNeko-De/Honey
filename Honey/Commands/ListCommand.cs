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

			IPackageListRepository packageListRepository = new PackageListRepository(new HoneyInstallLocation(), null);
			IListController listController = new ListController(packageListRepository);

			// limitOutput is more for programms, programms can easy add arguments, dont bother people to do that
			ListMode listMode = ListMode.Full;

			MatchMode matchMode = MatchMode.All;
			string searchPattern = null;
			if (args.Length > 1)
			{
				// TODO implement correct parameter parsing here :)
				matchMode = MatchMode.IdContains;
				searchPattern = args[1];
			}

			var packageInfos = listController.GetPackageInfo(searchPattern, listMode, matchMode);
			if (packageInfos != null)
			{
				string output = null;
				switch (listMode)
				{
					case ListMode.Full:
						foreach (var packageInfo in packageInfos)
						{
							output += $@"{nameof(packageInfo.PackageId)}: {packageInfo.PackageId}
{nameof(packageInfo.PackageVersion)}: {packageInfo.PackageVersion}
{nameof(packageInfo.LockedByAction)}: {packageInfo.LockedByAction}
{nameof(packageInfo.LockedByProcess)}: {packageInfo.LockedByProcess}
{nameof(packageInfo.Created)}: {packageInfo.Created}
{nameof(packageInfo.LastUpdated)}: {packageInfo.LastUpdated}";
						}

						break;
					case ListMode.LimitOutput:
						foreach (var packageInfo in packageInfos)
						{
							output += $"{packageInfo.PackageId}|{packageInfo.PackageVersion}";
						}
						break;
					default:
						throw new ArgumentOutOfRangeException(nameof(listMode));
				}
				// TODO Switch to .Net Core for Async WriteLines
				Console.WriteLine(output);
			}
			else
			{
				Console.WriteLine($"no package where id contains '{searchPattern}' found");
			}
		}
	}
}
