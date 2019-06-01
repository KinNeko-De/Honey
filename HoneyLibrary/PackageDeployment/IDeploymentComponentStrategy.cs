using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZipArchivExtensions;

namespace HoneyLibrary.PackageDeployment
{
    public interface IDeploymentComponentStrategy
    {
		void BuildDelta(IReadOnlyCollection<ParallelReadableZipArchiveEntry> installedEntries, IReadOnlyCollection<ParallelReadableZipArchiveEntry> newEntries);

        IReadOnlyCollection<IDeploymentComponent> CreateDeploymentComponents();
    }
}
