using HoneyLibrary.Controller;
using HoneyLibrary.PackageLists;
using Microsoft.Extensions.Logging;
using MyApplicationExample;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Honey.Commands
{
	public class ListCommand
	{
		private readonly ILogger logger;

		public ListCommand(ILogger logger)
		{
			this.logger = logger;
		}

		public void Execute(string[] args)
		{
			// no logger needed

			IPackageListRepository packageListRepository = new PackageListRepository(new HoneyInstallLocation(), null);
			IListController listController = new ListController(packageListRepository);

			// limitOutput is more for programms, programms can easy add arguments, dont bother people to do that
			ListMode listMode = ListMode.Full;
			// TODO implement limitOutput option
			if (args.Any(x => x.Equals("--LimitOutput", StringComparison.OrdinalIgnoreCase)))
			{
				listMode = ListMode.LimitOutput;
			}

			MatchMode matchMode = MatchMode.All;
			string searchPattern = null;
			if (args.Length > 1)
			{
				if(!args[1].StartsWith("--"))
				{
					// TODO implement correct parameter parsing here :)
					matchMode = MatchMode.IdContains;
					searchPattern = args[1];
				}
			}

			var packageInfos = listController.GetPackageInfo(searchPattern, listMode, matchMode);
			if (packageInfos != null)
			{
				StringBuilder output = new StringBuilder();
				

				switch (listMode)
				{
					case ListMode.Full:
						var first = true;
						foreach (var packageInfo in packageInfos)
						{
							if(!first)
							{
								output.AppendLine();
							}
							output.AppendLine($@"{nameof(packageInfo.PackageId)}: {packageInfo.PackageId}
{nameof(packageInfo.PackageVersion)}: {packageInfo.PackageVersion}
{nameof(packageInfo.LockedByAction)}: {packageInfo.LockedByAction}
{nameof(packageInfo.LockedByProcess)}: {packageInfo.LockedByProcess}
{nameof(packageInfo.Created)}: {packageInfo.Created}
{nameof(packageInfo.LastUpdated)}: {packageInfo.LastUpdated}");
							first = false;
						}

						break;
					case ListMode.LimitOutput:
						foreach (var packageInfo in packageInfos)
						{
							output.AppendLine($"{packageInfo.PackageId}|{packageInfo.PackageVersion}");
						}
						break;
					default:
						throw new ArgumentOutOfRangeException(nameof(listMode));
				}

				Console.Write(output);
			}
			else
			{
				// TODO Exception
				Console.WriteLine($"No package where id contains '{searchPattern}' found");
			}
		}
	}
}
