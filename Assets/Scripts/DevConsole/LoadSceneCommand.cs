using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine.SceneManagement;

namespace DevConsole
{
    /// <summary>
    /// Command to load an arbitrary scene.
    /// </summary>
    public class LoadSceneCommand : IDevCommand
    {
        /// <inheritdoc />
        public string Name => "scene";

        /// <inheritdoc />
        public void Run(string[] args, StringWriter sw)
        {
            if (args.Length != 1)
            {
                PrintUsage(sw);
                return;
            }

            List<string> scenes = new();
            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                // yes, we can't get the actual scene name, only the file path. Thanks Unity!
                scenes.Add(Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(i)));
            }

            // if one of y'all makes a scene called "list.unity" just FYI you will not be able to access it
            // but also, w h y
            if (args[0].ToLower() is "l" or "list")
            {
                foreach (string scene in scenes) sw.WriteLine($"Found Scene: {scene}");
                return;
            }

            if (scenes.Select(name => name.ToLower()).ToHashSet().Contains(args[0].ToLower()))
            {
                SceneManager.LoadScene(args[0]);
            }
            else
            {
                sw.WriteLine(IDevCommand.Color($"Scene {args[0]} not found.", "red"));
            }
        }

        /// <inheritdoc />
        public void PrintUsage(StringWriter sw, string color = "red")
        {
            sw.WriteLine(IDevCommand.Color($"Usage: {Name} <sceneName>", color));
            sw.WriteLine(IDevCommand.Color($"       {Name} list", color));
        }
    }
}