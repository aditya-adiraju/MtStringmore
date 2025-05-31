using System.IO;
using Player;
using UnityEngine;

namespace DevConsole
{
    /// <summary>
    /// Toggles player invincibility.
    /// </summary>
    public class InvincibilityCommand : IDevCommand
    {
        /// <summary>
        /// Internal variable to persist the setting across scene transitions.
        ///
        /// Required as we need to re-set invincibility after a scene transition as it defaults to false.
        /// </summary>
        private bool _isInvincible;

        /// <inheritdoc />
        public string Name => "sv_invincibility";

        /// <inheritdoc />
        public string[] Aliases => new[] { "invincibility", "god" };

        /// <summary>
        /// Constructor - initializes the scene change listener.
        /// </summary>
        public InvincibilityCommand(DevConsoleMenu console)
        {
            console.OnSceneLoad += OnSceneLoad;
        }

        /// <summary>
        /// Sets player invincibility and logs any errors to the StringWriter.
        /// </summary>
        /// <param name="arg">New player invincibility parameter</param>
        /// <param name="sw">StringWriter to log errors to</param>
        private void SetInvincibility(bool arg, StringWriter sw)
        {
            PlayerController pc = Object.FindObjectOfType<PlayerController>();
            if (!pc)
            {
                sw.WriteLine(IDevCommand.Color("Player not found.", "red"));
                return;
            }

            _isInvincible = arg;
            pc.DebugIgnoreDeath = _isInvincible;
        }

        /// <summary>
        /// Listener on scene changes to set player invincibility on scene load.
        /// </summary>
        /// <param name="sw">StringWriter to log any messages to the console.</param>
        private void OnSceneLoad(StringWriter sw)
        {
            if (!_isInvincible) return; // player defaults to false so we don't need to call it
            SetInvincibility(true, sw);
        }

        /// <inheritdoc />
        public void Run(string[] args, StringWriter sw)
        {
            if (args.Length != 1 || !IDevCommand.TryParseBool(args[0], out bool arg))
            {
                PrintUsage(sw);
                return;
            }

            SetInvincibility(arg, sw);
        }

        /// <inheritdoc />
        public void PrintUsage(StringWriter sw, string color = "red")
        {
            sw.WriteLine(IDevCommand.Color($"Usage: {Name} <1/0>", color));
        }
    }
}
