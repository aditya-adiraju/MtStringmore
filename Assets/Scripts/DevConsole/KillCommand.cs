using System.IO;
using UnityEngine;

namespace DevConsole
{
    /// <summary>
    /// Command to kill the player.
    /// </summary>
    public class KillCommand : IDevCommand
    {
        /// <inheritdoc />
        public string Name => "kill";

        /// <inheritdoc />
        public void Run(string[] args, StringWriter sw)
        {
            PlayerController pc = Object.FindObjectOfType<PlayerController>();
            if (!pc)
            {
                sw.WriteLine(IDevCommand.Color("Player not found.", "red"));
                return;
            }

            pc.SendMessage("HandleDeath");
        }

        /// <inheritdoc />
        public void PrintUsage(StringWriter sw, string color = "red")
        {
            sw.WriteLine(IDevCommand.Color($"Usage: {Name}", color));
        }
    }
}