using Honey.Commands;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Diagnostics;

namespace Honey
{
	class Program
	{
		static int Main(string[] args)
		{
			if (args == null) { throw new ArgumentNullException(nameof(args)); }

			ProgramExitCode exitCode = ProgramExitCode.Ok;
			ILogger logger = null;

			if (args == null)
			{
				throw new ArgumentNullException(nameof(args), "Provide at least one command.");
			}

			try
			{
				switch (args[0])
				{
					case "upgrade":
						logger = EnsureLoggerExists(logger);
						UpgradeCommand upgradeCommand = new UpgradeCommand(logger);
						upgradeCommand.Execute(args);
						break;
					case "list":
						ListCommand listCommand = new ListCommand(NullLogger.Instance);
						listCommand.Execute(args);
						break;
					default:
						throw new ArgumentOutOfRangeException("command", args[0], "Unknown command");

				}
			}
			catch (Exception e)
			{
				logger = EnsureLoggerExists(logger);
				logger.LogError(e.ToString());
				exitCode = ProgramExitCode.GeneralError;
			}

			return (int)exitCode;
		}

		private static ILogger EnsureLoggerExists(ILogger logger)
		{
			if(logger == null)
			{
				Stopwatch stopwatch = Stopwatch.StartNew();
				var loggerFactory = LoggerFactory.Create(builder =>
				{
					builder
						.AddConsole()
						.AddFilter("Microsoft", LogLevel.Warning)
						.AddFilter("System", LogLevel.Warning)
						.AddFilter("Honey", LogLevel.Debug);
				});
				logger = loggerFactory.CreateLogger<Program>();
				stopwatch.Stop();
				logger.LogDebug("Creating of Logger needed {0} ms.", stopwatch.ElapsedMilliseconds);
			}

			return logger;
		}
	}
}
