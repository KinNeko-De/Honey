using Honey.Commands;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;
using MyApplicationExample;
using Serilog;
using System;
using System.Diagnostics;
using System.IO;

namespace Honey
{
	class Program
	{
		static int Main(string[] args)
		{
			if (args == null) { throw new ArgumentNullException(nameof(args)); }

			ProgramExitCode exitCode = ProgramExitCode.Ok;
			Microsoft.Extensions.Logging.ILogger logger = null;

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

		private static Microsoft.Extensions.Logging.ILogger EnsureLoggerExists(Microsoft.Extensions.Logging.ILogger logger)
		{
			if(logger == null)
			{
				HoneyInstallLocation honeyInstallLocation = new HoneyInstallLocation();
				string logFile = Path.Combine(honeyInstallLocation.GetInstallLocation(), "honey.log");

				Stopwatch stopwatch = Stopwatch.StartNew();
				Serilog.Log.Logger = new Serilog.LoggerConfiguration()
					.MinimumLevel.Information() // because filtering over loggerfactory does not work
					.MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning) // because filtering over loggerfactory does not work
					.MinimumLevel.Override("System", Serilog.Events.LogEventLevel.Warning) // because filtering over loggerfactory does not work
					.MinimumLevel.Override("Honey", Serilog.Events.LogEventLevel.Debug) // because filtering over loggerfactory does not work
					.Enrich.FromLogContext()
					.WriteTo.File(logFile)
					.CreateLogger();

				var loggerFactory = LoggerFactory.Create(builder =>
				{
					builder
						.SetMinimumLevel(LogLevel.Debug) // does not work for serilog
						.AddSerilog(dispose: true)
						.AddConsole()
						.AddFilter("Microsoft", LogLevel.Warning) // does not work for serilog
						.AddFilter("System", LogLevel.Warning) // does not work for serilog
						.AddFilter("Honey", LogLevel.Debug) // does not work for serilog
						.AddFilter<ConsoleLoggerProvider>("Honey", LogLevel.Information)
						;
				});
				logger = loggerFactory.CreateLogger<Program>();
				stopwatch.Stop();
				logger.LogDebug("Creating of Logger needed {0} ms.", stopwatch.ElapsedMilliseconds);
			}

			return logger;
		}
	}
}
