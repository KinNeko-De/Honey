using HoneyLibrary.PackageRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyApplicationExampleTest
{
    public class PathInstallLocation : IInstallLocation
    {
        private readonly string path;

        public PathInstallLocation(string path)
        {
            this.path = path;
        }

        public string GetInstallLocation()
        {
            return path;
        }
    }
}
