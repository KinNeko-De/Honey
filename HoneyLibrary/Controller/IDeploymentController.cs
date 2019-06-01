using HoneyLibrary.PackageLists;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HoneyLibrary.Controller
{
    public interface IDeploymentController
    {
        void Upgrade(string packageId, Version packageVersion, string packageDownloadUri);
	}
}
