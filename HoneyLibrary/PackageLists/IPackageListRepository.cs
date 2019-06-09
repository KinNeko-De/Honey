using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HoneyLibrary.PackageLists
{
    public interface IPackageListRepository
    {
		void StartActionOnPackage(string packageId, Version version);

        void EndActionOnPackage(string packageId, Version version);

		/// <summary>
		/// Queries the infos to installed packages 
		/// </summary>
		/// <param name="searchPattern">The searchPattern, not relevant if you want to query all</param>
		/// <param name="listMode"></param>
		/// <param name="matchMode"></param>
		/// <returns></returns>
		IReadOnlyCollection<IPackageInfo> GetPackageInfo(string searchPattern, ListMode listMode, MatchMode matchMode);

		/// <summary>
		/// Fetches the info for a single package id for upgrading aspects.
		/// </summary>
		/// <param name="packageId">package id you want to edit</param>
		/// <returns>the package info if there exists one entry, null if no entry exist, and exception if contains more that one entry with that id</returns>
		IPackageInfo GetSinglePackageInfo(string packageId);
	}
}
