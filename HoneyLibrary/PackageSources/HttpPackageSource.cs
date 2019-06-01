using HoneyLibrary.PackageRepositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace HoneyLibrary.PackageSources
{
	public class HttpPackageSource : IPackageSource
	{
		private readonly string downloadUrl;
		private readonly HttpClient httpClient;

		public HttpPackageSource(string downloadUrl, HttpClient httpClient)
		{
			this.downloadUrl = downloadUrl;
			this.httpClient = httpClient;
		}

		public void Copy(string targetFile)
		{
			using (FileStream targetStream = new FileStream(targetFile, FileMode.Create))
			{
				httpClient.GetStreamAsync(downloadUrl).Result.CopyTo(targetStream);
			}
		}
	}
}
