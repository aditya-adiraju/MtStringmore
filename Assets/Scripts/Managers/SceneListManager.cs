using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Managers
{
    /// <summary>
    /// Main source of truth for scene information.
    /// </summary>
    public class SceneListManager : MonoBehaviour
    {
        private static SceneListManager _instance;

        /// <summary>
        /// "Singleton" instance of this scene list manager
        /// </summary>
        public static SceneListManager Instance
        {
            get
            {
                if (!_instance) _instance = FindAnyObjectByType<SceneListManager>();
                return _instance;
            }
        }

        [SerializeField] private string mainMenuSceneName;
        [SerializeField] private LevelInfo[] levels;

        private readonly List<string> _levelLoadScenes = new();
        private readonly Dictionary<string, int> _levelLookup = new();
        private readonly HashSet<string> _cutscenes = new();

        /// <summary>
        /// Returns the list of level loading scenes.
        /// </summary>
        public IReadOnlyList<string> LevelLoadScenes => _levelLoadScenes;

        /// <summary>
        /// Returns the level 1 cutscene scene name.
        /// </summary>
        public string Level1IntroSceneName => levels[0].preLevelCutscene;

        /// <summary>
        /// Whether the current scene is the main menu.
        /// </summary>
        public bool InMainMenu => mainMenuSceneName == SceneManager.GetActiveScene().name;

        /// <summary>
        /// Whether the current scene is a cutscene.
        /// </summary>
        public bool InCutscene => IsSceneCutscene(SceneManager.GetActiveScene().name);

        /// <summary>
        /// Current level number (1-indexed).
        /// </summary>
        public int LevelNumber
        {
            get
            {
                InitializeLookupsIfNotPresent();
                string currentSceneName = SceneManager.GetActiveScene().name;
                return _levelLookup.GetValueOrDefault(currentSceneName, -1);
            }
        }

        /// <summary>
        /// Whether a given scene is the main menu scene.
        /// </summary>
        /// <param name="sceneName">Scene name</param>
        /// <returns>True if it's the main menu</returns>
        public bool IsMainMenu(string sceneName)
        {
            return sceneName == mainMenuSceneName;
        }

        /// <summary>
        /// Loads the main menu.
        /// </summary>
        public void LoadMainMenu()
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }

        /// <summary>
        /// Determines whether a given scene is a cutscene.
        /// </summary>
        /// <param name="sceneName">Given scene name</param>
        /// <returns>Whether it's a cutscene</returns>
        public bool IsSceneCutscene(string sceneName)
        {
            InitializeLookupsIfNotPresent();
            return _cutscenes.Contains(sceneName);
        }

        /// <summary>
        /// Initializes level and cutscene lookup if not present.
        /// </summary>
        private void InitializeLookupsIfNotPresent()
        {
            if (_levelLookup.Count != 0) return;
            _levelLookup.Clear();
            _cutscenes.Clear();
            _levelLoadScenes.Clear();
            for (int i = 0; i < levels.Length; i++)
            {
                _levelLookup[levels[i].levelName] = i + 1;
                if (!string.IsNullOrWhiteSpace(levels[i].preLevelCutscene))
                {
                    _cutscenes.Add(levels[i].preLevelCutscene);
                    _levelLookup[levels[i].preLevelCutscene] = i + 1;
                    _levelLoadScenes.Add(levels[i].preLevelCutscene);
                }
                else
                {
                    _levelLoadScenes.Add(levels[i].levelName);
                }

                if (!string.IsNullOrWhiteSpace(levels[i].postLevelCutscene))
                {
                    _cutscenes.Add(levels[i].postLevelCutscene);
                    _levelLookup[levels[i].postLevelCutscene] = i + 1;
                }
            }
        }

        /// <summary>
        /// Each level's scene name information (pre cutscene/level/post ctuscene).
        /// </summary>
        [Serializable]
        private struct LevelInfo
        {
            public string preLevelCutscene;
            public string levelName;
            public string postLevelCutscene;
        }
    }
}
