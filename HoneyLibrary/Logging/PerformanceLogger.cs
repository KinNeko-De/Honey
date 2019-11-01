using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace HoneyLibrary.Logging
{
	internal class PerformanceLogger
	{
		private readonly ILogger logger;
		private Stopwatch stopwatch;
		private string state;

		public PerformanceLogger(ILogger logger)
		{
			this.logger = logger;
		}

		public void Restart(string state)
		{
			if (logger.IsEnabled(LogLevel.Debug))
			{
				if (stopwatch == null)
				{
					stopwatch = new Stopwatch();
				}
				logger.LogDebug("Begin of performance tracing for '{0}'", state);
				this.state = state; 
				stopwatch.Restart();
			}
		}

		public void Stop()
		{
			if (logger.IsEnabled(LogLevel.Debug))
			{
				stopwatch.Stop();
				logger.LogDebug("End of performance tracing for '{0}'. Execution needed {1} ms.", state, stopwatch.ElapsedMilliseconds);
			}
		}
	}
}
