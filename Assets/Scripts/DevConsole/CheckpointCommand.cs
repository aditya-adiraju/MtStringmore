using System.IO;
using Interactables;
using Managers;
using UnityEngine;

namespace DevConsole
{
    /// <summary>
    /// Command to list checkpoints or set player checkpoint position.
    /// </summary>
    public class CheckpointCommand : IDevCommand
    {
        /// <inheritdoc />
        public string Name => "checkpoint";

        /// <inheritdoc />
        public string[] Aliases => new[] { "cp" };

        /// <inheritdoc />
        public void Run(string[] args, StringWriter sw)
        {
            if (args.Length is < 1 or > 2)
            {
                PrintUsage(sw);
                return;
            }

            if (args.Length == 1 && args[0].ToLower() is "list" or "l")
            {
                Checkpoint[] checkpoints = Object.FindObjectsOfType<Checkpoint>();

                for (int i = 0; i < checkpoints.Length; i++)
                {
                    string isEnd = checkpoints[i].HasConversation ? " (end)" : "";
                    sw.WriteLine($"Checkpoint {i} ({checkpoints[i].name}): {checkpoints[i].transform.position}{isEnd}");
                }

                return;
            }

            bool result = IDevCommand.TryGetPosOrCheckpointPos(args, sw, out Vector2 pos);
            if (!result) return;
            GameManager.Instance.UpdateCheckpointData(pos);
        }

        /// <inheritdoc />
        public void PrintUsage(StringWriter sw, string color = "red")
        {
            sw.WriteLine(IDevCommand.Color($"Usage: {Name} <checkpoint no>", color));
            sw.WriteLine(IDevCommand.Color($"       {Name} <list/l>", color));
            sw.WriteLine(IDevCommand.Color($"       {Name} <x> <y>", color));
        }
    }
}
