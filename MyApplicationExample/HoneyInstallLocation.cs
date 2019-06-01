using HoneyLibrary.PackageRepositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyApplicationExample
{
    public class HoneyInstallLocation : IInstallLocation
    {
        // TODO determine the path different
        public string GetInstallLocation()
        {
            return @"C:\ProgramData\Honey";
        }
    }
}
