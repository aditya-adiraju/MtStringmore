using System.IO;
using UnityEngine;

namespace DevConsole
{
    /// <summary>
    /// Flips the player's velocity.
    /// </summary>
    public class FlipCommand : IDevCommand
    {
        private readonly SimpleVelocityEffector _flipVelocityEffector = new(velocity => new Vector2(-velocity.x,velocity.y));
        /// <inheritdoc />
        public string Name => "flip";

        /// <inheritdoc />
        public void Run(string[] args, StringWriter sw)
        {
            PlayerController pc = Object.FindObjectOfType<PlayerController>();
            if (!pc)
            {
                sw.WriteLine(IDevCommand.Color("Player not found.", "red"));
                return;
            }

            pc.AddPlayerVelocityEffector(_flipVelocityEffector, true);
        }

        /// <inheritdoc />
        public void PrintUsage(StringWriter sw, string color = "red")
        {
            sw.WriteLine(IDevCommand.Color($"Usage: {Name}", color));
        }
    }
}
