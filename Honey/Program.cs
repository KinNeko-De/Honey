using Honey.Commands;
using System;

namespace Honey
{
    class Program
    {
        static int Main(string[] args)
        {
            if (args == null) { throw new ArgumentNullException(nameof(args)); }
            ProgramExitCode exitCode = ProgramExitCode.Ok;
            try
            {
				switch(args[0])
				{
					case "upgrade":
						UpgradeCommand upgradeCommand = new UpgradeCommand();
						upgradeCommand.Execute(args);
						break;
					case "list":
						ListCommand listCommand = new ListCommand();
						listCommand.Execute(args);
						break;
					default:
						throw new ArgumentOutOfRangeException("command", args[0], "Unknown command");

				}
            }
            catch(Exception e)
            {
                Console.Error.WriteLine(e.ToString());
                exitCode = ProgramExitCode.GeneralError;
				Console.ReadKey();
			}


            return (int)exitCode;
        }
    }
}
