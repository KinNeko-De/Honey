using HoneyLibrary.PackageLists;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HoneyLibrary.Controller
{
	public interface IListController
	{
		IReadOnlyCollection<IPackageInfo> GetPackageInfo(string searchPattern, ListMode listMode, MatchMode matchMode);
	}
}
