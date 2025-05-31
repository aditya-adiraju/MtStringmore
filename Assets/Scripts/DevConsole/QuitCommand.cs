using System.IO;
using UnityEngine;

namespace DevConsole
{
    /// <summary>
    /// Command to quit the game.
    /// </summary>
    public class QuitCommand : IDevCommand
    {
        /// <inheritdoc />
        public bool RequiresCheats => false;

        /// <inheritdoc />
        public string Name => "quit";

        /// <inheritdoc />
        public string[] Aliases => new[] { "q" };

        /// <inheritdoc />
        public void Run(string[] args, StringWriter sw)
        {
            Application.Quit();
        }

        /// <inheritdoc />
        public void PrintUsage(StringWriter sw, string color = "red")
        {
            sw.WriteLine(IDevCommand.Color($"Usage: {Name}", color));
        }
    }
}
