using HoneyLibrary.PackageRepositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace HoneyLibrary.PackageSources
{
	public class DataServicePackageSourceRepository : IPackageSourceRepository
	{
		readonly private Uri uri;
		readonly private HttpClient downloadClient;

		public DataServicePackageSourceRepository(Uri uri)
		{
			this.uri = uri;
			downloadClient = new HttpClient();
		}

		public IPackageSource FindPackage(string packageId, Version packageVersion)
		{
			var downloadUrl = GetDownloadUrl(packageId, packageVersion, uri);

			return new HttpPackageSource(downloadUrl, downloadClient);
		}

		private string GetDownloadUrl(string packageId, Version version, Uri packageSource)
		{
			// replace with nuget code : https://blog.nuget.org/20130520/Play-with-packages.html
			throw new NotImplementedException();
		}
	}
}
