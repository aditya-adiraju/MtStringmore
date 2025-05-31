using System.IO;
using Managers;

namespace DevConsole
{
    /// <summary>
    /// Command to reload the scene.
    /// </summary>
    /// <remarks>
    /// I don't think this is necessary since pressing 'r' reloads the scene anyways,
    /// and the command has an 'r' in it LOL
    /// </remarks>
    public class ResetCommand : IDevCommand
    {
        /// <inheritdoc />
        public string Name => "reset";

        /// <inheritdoc />
        public string[] Aliases => new[] { "r" };

        /// <inheritdoc />
        public void Run(string[] args, StringWriter sw)
        {
            GameManager.Instance.Respawn();
        }

        /// <inheritdoc />
        public void PrintUsage(StringWriter sw, string color = "red")
        {
            sw.WriteLine(IDevCommand.Color($"Usage: {Name}", color));
        }
    }
}
