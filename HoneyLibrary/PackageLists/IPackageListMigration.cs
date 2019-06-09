using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace HoneyLibrary.PackageLists
{
	public interface IPackageListMigration
	{
		void Migrate(XDocument packageList);
	}
}
