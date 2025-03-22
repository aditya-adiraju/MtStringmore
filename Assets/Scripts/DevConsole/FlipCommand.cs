using System.IO;
using UnityEngine;

namespace DevConsole
{
    /// <summary>
    /// Flips the player's velocity.
    /// </summary>
    public class FlipCommand : IDevCommand
    {
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

            pc.ActiveVelocityEffector = new FlipVelocityEffector(pc);
        }

        /// <inheritdoc />
        public void PrintUsage(StringWriter sw, string color = "red")
        {
            sw.WriteLine(IDevCommand.Color($"Usage: {Name}", color));
        }

        /// <summary>
        /// Velocity effector to flip the player's horizontal velocity.
        /// </summary>
        private class FlipVelocityEffector : IPlayerVelocityEffector
        {
            private readonly PlayerController _player;

            /// <summary>
            /// Constructor to specify the player.
            /// </summary>
            /// <param name="player">Player to flip horizontal velocity of</param>
            public FlipVelocityEffector(PlayerController player)
            {
                _player = player;
            }

            /// <inheritdoc />
            public Vector2 ApplyVelocity(Vector2 velocity)
            {
                Vector2 returnValue = new(-velocity.x, velocity.y);
                if (_player.ActiveVelocityEffector == this) _player.ActiveVelocityEffector = null;
                return returnValue;
            }
        }
    }
}
