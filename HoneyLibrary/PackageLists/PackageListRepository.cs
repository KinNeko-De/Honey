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

		private IReadOnlyCollection<IPackageListMigration> packageListMigrations { get; }

		public PackageListRepository(IInstallLocation honeyInstallLocation, IReadOnlyCollection<IPackageListMigration> packageListMigrations)
		{
			this.honeyInstallLocation = honeyInstallLocation;
			this.packageListMigrations = packageListMigrations;
		}

		// TODO For MatchMode.All the searchPattern is useless
		public IReadOnlyCollection<IPackageInfo> GetPackageInfo(string searchPattern, ListMode listMode, MatchMode matchMode)
		{
			try
			{
				using (FileStream fileStream = OpenPackageListForReadOperation())
				{
					PackageList packageList = LoadPackageList(fileStream);

					var packageInfos = packageList.FetchPackageListInfo(searchPattern, listMode, matchMode);

					return packageInfos;
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
					PackageList packageList = LoadPackageList(fileStream);

					IPackageInfo packageInfo = packageList.FetchSinglePackageListInfo(packageId, listMode);

					return packageInfo;
				}
			}
			catch (FileNotFoundException)
			{
				return null;
			}
		}

		private PackageList LoadPackageList(FileStream fileStream)
		{
			PackageList packageList;
			try
			{
				packageList = PackageList.Load(fileStream, packageListMigrations);
			}
			catch (Exception e)
			{
				throw new InvalidOperationException($"Reading of package list failed: {e.Message}", e);
			}

			return packageList;
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
				PackageInfo packageInfo = packageList.FetchSinglePackageListInfo(packageId);
				if (packageInfo == null)
				{
					packageInfo = packageList.AddPackageListInfo(packageId, version, action, processId);
				}
				else
				{
					AbortIfLockedByProcess(packageInfo);
					packageList.UpdatePackageListInfo(version, action, processId, packageInfo);
				}

			});
		}

		private void AbortIfLockedByProcess(IPackageInfo packageInfo)
		{
			var lockedbyProcess = packageInfo.LockedByProcess;
			if (!string.IsNullOrEmpty(lockedbyProcess) && lockedbyProcess != Process.GetCurrentProcess().Id.ToString())
			{
				// Throw honey specific exception and handle this different than unexpected exception (reduced output to non stacktrace and return specific exit code)
				throw new InvalidOperationException($"Package {packageInfo.PackageId} can not be locked. Action '{packageInfo.LockedByAction}' is on progress by process '{ packageInfo.LockedByProcess }'");
			}
		}

		private void WriteToPackageList(Func<FileStream> packageListOpenStream, Action<PackageList> actionToPerformOnPackageList)
		{
			FileStream fileStream;
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
				PackageList packageList;
				if (fileExists)
				{
					packageList = LoadPackageList(fileStream);
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
