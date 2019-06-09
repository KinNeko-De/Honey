using HoneyLibrary.PackageLists;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HoneyLibrary.Controller
{
	public class ListController : IListController
	{
		private readonly IPackageListRepository packageListRepository;

		public ListController(
			IPackageListRepository packageListRepository)
		{
			this.packageListRepository = packageListRepository;
		}

		public IReadOnlyCollection<IPackageInfo> GetPackageInfo(string searchPattern, ListMode listMode, MatchMode matchMode)
		{
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Restart();
			var result = packageListRepository.GetPackageInfo(searchPattern, listMode, matchMode);
			stopwatch.Stop();
			Console.WriteLine($"Querying xml list needs {stopwatch.ElapsedMilliseconds} ms.");
			return result;
		}
	}
}
