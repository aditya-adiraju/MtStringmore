using System.IO;
using UnityEngine.SceneManagement;

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
        public void Run(string[] args, StringWriter sw)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        /// <inheritdoc />
        public void PrintUsage(StringWriter sw, string color = "red")
        {
            sw.WriteLine(IDevCommand.Color($"Usage: {Name}", color));
        }
    }
}