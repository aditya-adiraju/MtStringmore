using System.IO;

namespace DevConsole
{
    /// <summary>
    /// Command to enable cheats. Yes, that command looks very familiar.
    /// </summary>
    public class EnableCheatsCommand : IDevCommand
    {
        /// <inheritdoc />
        /// <remarks>
        /// Need this to not require cheats as otherwise the player can't enable it lol
        /// </remarks>
        public bool RequiresCheats => false;

        /// <inheritdoc />
        public string Name => "sv_cheats";

        private readonly DevConsoleMenu _devConsole;

        /// <summary>
        /// Constructor to provide DevConsole to toggle cheats on.
        /// </summary>
        /// <param name="menu">DevConsole to change cheat settings</param>
        public EnableCheatsCommand(DevConsoleMenu menu)
        {
            _devConsole = menu;
        }

        /// <inheritdoc />
        public void Run(string[] args, StringWriter sw)
        {
            if (args.Length != 1 || !IDevCommand.TryParseBool(args[0], out bool arg))
            {
                PrintUsage(sw);
                return;
            }

            _devConsole.cheatsEnabled = arg;
        }

        /// <inheritdoc />
        public void PrintUsage(StringWriter sw, string color = "red")
        {
            sw.WriteLine(IDevCommand.Color($"Usage: {Name} <1/0>", color));
        }
    }
}