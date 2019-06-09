using HoneyLibrary.PackageRepositories;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml.Linq;

namespace HoneyLibrary.PackageLists
{
	public class PackageListRepository : IPackageListRepository
	{
		public const string PackageListFileName = "packages.xml";
		private readonly IInstallLocation honeyInstallLocation;

		public PackageListRepository(IInstallLocation honeyInstallLocation)
		{
			this.honeyInstallLocation = honeyInstallLocation;
		}

		// TODO For MatchMode.All the searchPattern is useless
		public IReadOnlyCollection<IPackageInfo> GetPackageInfo(string searchPattern, ListMode listMode, MatchMode matchMode)
		{
			try
			{
				using (FileStream fileStream = OpenPackageListForReadOperation())
				{
					XDocument packageList;
					try
					{
						packageList = PackageList.Load(fileStream);
					}
					catch (Exception e)
					{
						throw new InvalidOperationException($"Reading of package list failed: {e.Message}", e);
					}

					IEnumerable<XElement> packageListInfoElement = PackageList.FetchPackageListInfo(searchPattern, packageList, matchMode);

					return packageListInfoElement.Select(x => PackageList.ReadPackageInfo(x, listMode)).ToArray();
				}
			}
			catch(FileNotFoundException)
			{
				return null;
			}
		}

		public IPackageInfo GetSinglePackageInfo(string packageId)
		{
			return GetSinglePackageInfo(packageId, ListMode.LimitOutput);
		}

		internal IPackageInfo GetSinglePackageInfo(string packageId, ListMode listMode)
		{
			try
			{
				using (FileStream fileStream = OpenPackageListForReadOperation())
				{
					XDocument packageList;
					try
					{
						packageList = PackageList.Load(fileStream);
					}
					catch (Exception e)
					{
						throw new InvalidOperationException($"Reading of package list failed: {e.Message}", e);
					}

					XElement packageListInfoElement = PackageList.FetchSinglePackageListInfo(packageId, packageList);

					return PackageList.ReadPackageInfo(packageListInfoElement, listMode);
				}
			}
			catch (FileNotFoundException)
			{
				return null;
			}
		}

		public void RemovePackage(string packageId)
		{
			throw new NotImplementedException();
		}

		public void StartActionOnPackage(string packageId, Version version)
		{
			string processId = Process.GetCurrentProcess().Id.ToString();
			var action = "Upgrade";
			WritePackageInfo(packageId, version, action, processId);
		}

		public void EndActionOnPackage(string packageId, Version version)
		{
			string processId = string.Empty;
			string action = string.Empty;
			WritePackageInfo(packageId, version, action, processId);
		}

		private void WritePackageInfo(string packageId, Version version, string action, string processId)
		{
			WriteToPackageList(OpenPackageListForWriteOperation, (packageList) =>
			{
				XElement packageListInfoElement = PackageList.FetchSinglePackageListInfo(packageId, packageList);
				if (packageListInfoElement == null)
				{
					packageListInfoElement = PackageList.CreatePackageListInfo(packageId, version, action, processId, packageList);
				}
				else
				{
					PackageList.AbortIfLockedByProcess(packageListInfoElement);
					PackageList.UpdatePackageListInfo(version, action, processId, packageListInfoElement);
				}

			});
		}

		private void WriteToPackageList(Func<FileStream> packageListOpenStream, Action<XDocument> actionToPerformOnPackageList)
		{
			FileStream fileStream = null;
			bool fileExists = true;
			try
			{
				fileStream = packageListOpenStream.Invoke();
			}
			catch (FileNotFoundException)
			{
				string packageListFileName = GetPackageListFilePath();
				fileStream = File.Open(packageListFileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
				fileExists = false;
			}

			using (fileStream)
			{
				XDocument packageList;
				if (fileExists)
				{
					try
					{
						packageList = PackageList.Load(fileStream);
					}
					catch (Exception e)
					{
						throw new InvalidOperationException($"Reading of package list failed: {e.Message}", e);
					}
				}
				else
				{
					packageList = PackageList.CreateNewPackageList();
				}

				actionToPerformOnPackageList.Invoke(packageList);
				fileStream.SetLength(0);
				packageList.Save(fileStream);
			}
		}

		internal FileStream OpenPackageListForWriteOperation()
		{
			string packageListFileName = GetPackageListFilePath();

			FileStream fileStream = null;

			RetryStrategy.StepRandomIncrement.Try(() => fileStream = File.Open(packageListFileName, FileMode.Open, FileAccess.ReadWrite, FileShare.Read),
				(exceptions, retryCount) => { throw new PackageFileCanNotBeAccessedException($"The package list file '{packageListFileName}' can not be accessed after {retryCount} retries.", exceptions); });

			return fileStream;
		}

		internal FileStream OpenPackageListForReadOperation()
		{
			string packageListFileName = GetPackageListFilePath();
			return File.Open(packageListFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
		}

		private string GetPackageListFilePath()
		{
			return Path.Combine(honeyInstallLocation.GetInstallLocation(), PackageListFileName);
		}

		private abstract class RetryStrategy
		{
			public static Retry StepRandomIncrement = new StepRandomIncrementRetryStrategy();

			private class StepRandomIncrementRetryStrategy : Retry
			{
				Random random = new Random();
				readonly int step = 10;

				public override bool ShouldHandleException(Exception exception)
				{
					return exception is IOException && !(exception is FileNotFoundException);
				}

				public override bool ShouldRetry(int retryCount)
				{
					return retryCount < 30;
				}

				public override int GetWaitTime(int retryCount)
				{
					var increment = random.Next(9, 13);

					int x = (int)Math.Pow(increment, (retryCount / step) + 1);
					int y = retryCount % step;
					var waitTime = Math.Max(increment, x * (y + 1));
					return waitTime;
				}
			}

			public abstract class Retry
			{
				public HashSet<Exception> exceptions = new HashSet<Exception>();

				public abstract bool ShouldHandleException(Exception exception);
				public abstract bool ShouldRetry(int retryCount);
				public abstract int GetWaitTime(int retryCount);

				public void Try(Action action, Action<HashSet<Exception>, int> OnRetryFailed)
				{
					int retryCount = 0;
					retryPoint:
					try
					{
						action();
						return;
					}
					catch (Exception exception)
					{
						if(ShouldHandleException(exception))
						{
							exceptions.Add(exception);

							if (ShouldRetry(retryCount))
							{
								var waitTime = GetWaitTime(retryCount);
								retryCount++;
								Thread.Sleep(waitTime);
								goto retryPoint;
							}
							else
							{
								OnRetryFailed(exceptions, retryCount);
							}
						}

						throw;
					}

				}
			}
			
		}
	}

}
