using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DevConsole
{
    /// <summary>
    /// Command to print out all commands and their usages.
    /// </summary>
    public class HelpCommand : IDevCommand
    {
        /// <inheritdoc />
        /// <remarks>
        /// Imagine if I made the help command require cheats lol
        /// </remarks>
        public bool RequiresCheats => false;

        /// <inheritdoc />
        public string Name => "help";

        private readonly DevConsoleMenu _console;

        /// <summary>
        /// Initializes the HelpCommand with a reference to the console (with the list of registered commands).
        /// </summary>
        /// <param name="console">DevConsoleMenu with the commands</param>
        public HelpCommand(DevConsoleMenu console)
        {
            _console = console;
        }

        /// <inheritdoc />
        public void Run(string[] args, StringWriter sw)
        {
            IEnumerable<IDevCommand> commands = _console.Commands;
            switch (args.Length)
            {
                case > 1:
                    PrintUsage(sw);
                    return;
                case 1:
                {
                    string cmd = args[0].ToLower();
                    IDevCommand requested = commands.FirstOrDefault(c => c.Name == cmd);
                    if (requested == null)
                    {
                        sw.WriteLine(IDevCommand.Color($"Command {cmd} not found.", "red"));
                        return;
                    }

                    requested.PrintUsage(sw, "white");
                    return;
                }
                case 0:
                {
                    sw.WriteLine("Commands:");
                    foreach (IDevCommand command in commands)
                    {
                        sw.Write(" - " + command.Name);
                        if (command.RequiresCheats)
                            sw.WriteLine(" (requires sv_cheats 1)");
                        else
                            sw.WriteLine();
                    }

                    sw.WriteLine("For more information about a command, use help <command>");
                    return;
                }
            }
        }

        /// <inheritdoc />
        public void PrintUsage(StringWriter sw, string color = "red")
        {
            sw.WriteLine(IDevCommand.Color($"Usage: {Name}", color));
            sw.WriteLine(IDevCommand.Color($"       {Name} <command>", color));
        }
    }
}