using System.IO;
using UnityEngine;

namespace DevConsole
{
    /// <summary>
    /// Command for teleportation to a specific position.
    /// </summary>
    public class TeleportCommand : IDevCommand
    {
        /// <inheritdoc />
        public string Name => "tp";

        /// <inheritdoc />
        public void Run(string[] args, StringWriter sw)
        {
            if (args.Length is < 1 or > 2)
            {
                PrintUsage(sw);
                return;
            }

            bool result = IDevCommand.TryGetPosOrCheckpointPos(args, sw, out Vector2 pos);
            // error message is already printed, return
            if (!result) return;
            PlayerController pc = Object.FindObjectOfType<PlayerController>();
            if (!pc)
            {
                sw.WriteLine(IDevCommand.Color("Player not found.", "red"));
                return;
            }

            pc.transform.position = pos;
        }

        /// <inheritdoc />
        public void PrintUsage(StringWriter sw, string color = "red")
        {
            sw.WriteLine(IDevCommand.Color($"Usage: {Name} <checkpoint no>", color));
            sw.WriteLine(IDevCommand.Color($"       {Name} <x> <y>", color));
        }
    }
}