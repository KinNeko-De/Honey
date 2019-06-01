using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HoneyLibraryTest.PackageList.TestData
{
	internal class PackagesBuilder
	{
		const string placeholderPackagesVersion = "{{PackagesVersion}}";
		const string placeholderPackageXml = "{{PackageXml}}";

		private readonly string content = $@"<?xml version=""1.0"" encoding=""utf-8"" standalone=""yes""?>
<Packages{placeholderPackagesVersion}>
{placeholderPackageXml}
</Packages>";
		private string packagesVersion = @" Version=""1.0""";
		private readonly List<string> packageXml = new List<string>();

		public PackagesBuilder SetPackagesVersion(string packagesVersion)
		{
			if(!string.IsNullOrEmpty(packagesVersion))
			{
				packagesVersion = $@" Version = ""{packagesVersion}""";
			}
			this.packagesVersion = packagesVersion;
			return this;
		}

		public PackagesBuilder AddPackage(PackageBuilder packageBuilder)
		{
			this.packageXml.Add(packageBuilder.Build());
			return this;
		}

		public string Build()
		{
			StringBuilder stringBuilder = new StringBuilder(content);
			stringBuilder.Replace(placeholderPackagesVersion, packagesVersion);
			stringBuilder.Replace(placeholderPackageXml, string.Join(Environment.NewLine, packageXml));
			return stringBuilder.ToString();
		}
	}
}
