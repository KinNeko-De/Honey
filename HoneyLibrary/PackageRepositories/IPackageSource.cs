using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HoneyLibrary.PackageRepositories
{
	public interface IPackageSource
	{
		void Copy(string targetFile);
	}
}
