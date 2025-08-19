using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Interactables;
using Player;
using Save;
using UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using Yarn.Unity;
using Action = System.Action;

namespace Managers
{
    /// <summary>
    /// Singleton class for global game settings. Persists between scenes and reloads.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        private static GameManager _instance;

        /// <summary>
        /// Finds the singleton in the scene if present.
        /// </summary>
        /// <remarks>
        /// Bypasses the lifetime check since it's DontDestroyOnLoad.
        /// </remarks>
        public static GameManager Instance => _instance ??= FindObjectOfType<GameManager>();

        /// <summary>
        /// Last checkpoint position. The player should respawn here if they die.
        /// </summary>
        public Vector2 CheckPointPos { get; private set; }

        /// <summary>
        /// If true, the player faces left when they respawn
        /// </summary>
        public bool RespawnFacingLeft { get; set; }

        /// <summary>
        /// Number of checkpoints reached.
        /// </summary>
        public string[] LevelsAccessed => _levelsAccessed.ToArray();

        /// <summary>
        /// Accessor of existing level data.
        /// </summary>
        public ReadOnlyCollection<LevelData> AllLevelData => Array.AsReadOnly(_levelData);

        /// <summary>
        /// The number of collectables collected.
        /// Should be reset to 0 after being displayed (e.g. after a end-of-level cutscene).
        /// </summary>
        public int NumCollectablesCollected { get; private set; }

        /// <summary>
        /// Max number of known collectables.
        /// </summary>
        public int MaxCollectablesCount { get; private set; }

        /// <summary>
        /// Action that gets invoked when level reloads, e.g. respawns
        /// </summary>
        public event Action Reset;

        /// <summary>
        /// Action invoked when interactables are now enabled.
        /// </summary>
        public event Action OnInteractablesEnabledChanged;

        /// <summary>
        /// Whether interactables in this scene are "enabled".
        /// If disabled, sprite is changed and they can't be used by the player.
        /// </summary>
        public bool AreInteractablesEnabled
        {
            get => _areInteractablesEnabled;
            set
            {
                if (_areInteractablesEnabled == value) return;
                _areInteractablesEnabled = value;
                OnInteractablesEnabledChanged?.Invoke();
            }
        }

        /// <summary>
        /// Canvas to fade in/out when transitioning between scenes
        /// </summary>
        [SerializeField] private FadeEffects sceneTransitionCanvas;
#if UNITY_EDITOR
        /// <summary>
        /// Specific <see cref="Application.targetFrameRate"/> to force.
        /// </summary>
        /// <remarks>
        /// To the devs: don't EVER use this outside of #if UNITY_EDITOR or else i'll have to painfully debug your mess
        /// at 2 am and be able to use this story for my next behavioural interview
        /// </remarks>
        [SerializeField, Tooltip("Specific frame rate to force (0 or-1 is uncapped)")]
        private int forceFrameRate;
#endif
        /// <summary>
        /// Number of times Marshmallow dies in a level
        /// called by results manager and level select to display stats
        /// </summary>
        public int ThisLevelDeaths { get; private set; }

        /// <summary>
        /// The time it took for player to beat a level in seconds.
        /// </summary>
        /// <remarks>
        /// Called by results manager and level select to display stats
        /// </remarks>
        public float ThisLevelTime { get; set; } = float.NaN;

        private readonly HashSet<Vector2> _prevCheckpoints = new();
        private readonly HashSet<string> _levelsAccessed = new();
        private readonly LevelData[] _levelData = new LevelData[4];

        private bool _areInteractablesEnabled = true;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            ThisLevelTime = float.NaN;
            for (int i = 0; i < _levelData.Length; i++)
            {
                _levelData[i] = new LevelData();
            }

            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
            if (SystemInfo.deviceType == DeviceType.Handheld)
            {
                // TODO make maxFrameRate a setting
                Application.targetFrameRate =
                    Mathf.RoundToInt((float)Screen.resolutions.Max(res => res.refreshRateRatio.value));
            }
            else
            {
#if UNITY_EDITOR
                // only inform dev folks that the frame rate is being set if non-capped (i.e. not 0 or -1)
                if (forceFrameRate > 0)
                    Debug.LogWarning($"Forcing frame rate to be {forceFrameRate}");
                Application.targetFrameRate = forceFrameRate;
#endif
            }

            Debug.Log("Application version: " + Application.version);
        }

        private void Update()
        {
            if (Input.GetButtonDown("Debug Reset") && !ResultsManager.isResultsPageOpen) Respawn();
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;

            PlayerController player = FindObjectOfType<PlayerController>();
            if (player)
            {
                player.Death -= OnPlayerDeath;
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            sceneTransitionCanvas.InvokeFadeOut();
            Time.timeScale = 1f;
            // We are resetting all stats (collectables etc.) each time we load scene
            // We need make sure we are not clearing stats when we are loading the results after a cutscene
            // Therefore we need a list of all cutscenes that show results after the cutscene
            // Results Manager also uses this to avoid issues
            bool isInCutscene = SceneListManager.Instance.IsSceneCutscene(scene.name);

            if (!isInCutscene)
            {
                PlayerController player = FindObjectOfType<PlayerController>();
                if (player)
                {
                    player.Death += OnPlayerDeath;
                    CheckPointPos = player.transform.position;
                    Debug.Log("Hopefully set checkpoint position to be player's position: " + CheckPointPos);
                }

                _prevCheckpoints.Clear();
                NumCollectablesCollected = 0;
                ThisLevelDeaths = -1;
                SaveDataManager.SaveFile();

                Collectable[] collectables = FindObjectsByType<Collectable>(FindObjectsSortMode.None);
                MaxCollectablesCount = collectables.Length;
                Debug.Log("GameManager loaded " + MaxCollectablesCount + " collectables");
            }
            else
            {
                Debug.Log("Skipping collectable count in cutscene. Using previous value: " + MaxCollectablesCount);
                Debug.Log("Skipping ThisLevelTime in cutscene. Using previous value: " + ThisLevelTime);
            }
        }

        private void OnPlayerDeath()
        {
            //brings the deaths from a negative sentinel value to 1
            if (ThisLevelDeaths == -1)
            {
                ThisLevelDeaths += 2;
            }
            else
            {
                ThisLevelDeaths++;
            }
        }

        /// <summary>
        /// Saves level data - called by resultsManager once last checkpoint is reached.
        /// </summary>
        public void SaveGame()
        {
            SaveLevelDataToGameManager();
            SaveDataManager.SaveFile();
        }

        /// <summary>
        /// saves all of the current game stats (thisLevelDeaths, thisLevelCandies, thisLevelTime)
        /// to the game manager level data variables
        /// </summary>
        private void SaveLevelDataToGameManager()
        {
            int index = SceneListManager.Instance.LevelNumber - 1;
            if (index < 0)
            {
                Debug.Log("GameManager could not determine what level we are currently in");
                return;
            }
            Debug.Assert(index < _levelData.Length, "Invalid level index when saving game data?");
            LevelData updatedLevelData = _levelData[index];
            // Candies
            updatedLevelData.mostCandiesCollected =
                Mathf.Max(updatedLevelData.mostCandiesCollected, NumCollectablesCollected);
            updatedLevelData.totalCandiesInLevel = MaxCollectablesCount;
            // Deaths
            int cleanedLevelDeaths = Mathf.Max(0, ThisLevelDeaths);
            updatedLevelData.leastDeaths = updatedLevelData.leastDeaths == -1
                ? cleanedLevelDeaths
                : Mathf.Min(updatedLevelData.leastDeaths, cleanedLevelDeaths);

            // Time
            updatedLevelData.bestTime = float.IsNaN(updatedLevelData.bestTime) ? ThisLevelTime :
                Math.Min(updatedLevelData.bestTime, ThisLevelTime);
            _levelData[index] = updatedLevelData;
        }

        /// <summary>
        /// Clears all checkpoint data.
        /// </summary>
        public void ClearCheckpointData()
        {
            _prevCheckpoints.Clear();
        }

        /// <summary>
        /// Sets the checkpoint location and left facing spawn point.
        /// 
        /// Checks if we've already visited this checkpoint before setting spawn (to avoid backtracking).
        /// </summary>
        /// <param name="newCheckpointLocation">New checkpoint location</param>
        /// <param name="shouldFaceLeft">Whether respawn should face left</param>
        public void UpdateCheckpointData(Vector2 newCheckpointLocation, bool shouldFaceLeft = false)
        {
            if (!_prevCheckpoints.Add(newCheckpointLocation))
            {
                Debug.Log("Reached previous checkpoint.");
                return;
            }

            CheckPointPos = newCheckpointLocation;
            RespawnFacingLeft = shouldFaceLeft;
        }

        /// <summary>
        /// Sets game information from save data.
        /// </summary>
        public bool UpdateFromSaveData()
        {
            SaveFileData? optionalData = SaveDataManager.ReadExistingSave();
            if (optionalData == null) return false;
            SaveData saveData = optionalData.Value.saveData;
            _prevCheckpoints.Clear();
            NumCollectablesCollected = 0;
            _levelsAccessed.UnionWith(saveData.levelsAccessed);

            _levelData[0] = saveData.level1Data;
            _levelData[1] = saveData.level2Data;
            _levelData[2] = saveData.level3Data;
            _levelData[3] = saveData.level4Data;

            SaveDataManager.SaveFile();
            return true;
        }

        /// <summary>
        /// Adds a level to the level accessed list.
        /// </summary>
        /// <param name="level">New level</param>
        /// <returns>
        /// Whether the add was successful (i.e. not present).
        /// </returns>
        public bool AddLevelAccessed(string level) => _levelsAccessed.Add(level);

        /// <summary>
        /// Increments the number of candy collected.
        /// </summary>
        public void CollectCollectable(Collectable collectable)
        {
            NumCollectablesCollected++;
        }

        /// <summary>
        /// Resets the number of candy collected.
        /// </summary>
        public void ResetCandyCollected()
        {
            NumCollectablesCollected = 0;
        }

        // ReSharper disable Unity.PerformanceAnalysis
        /// <summary>
        /// Resets core components like player, Knitby, camera, etc to the state at
        /// the last checkpoint
        /// </summary>
        public void Respawn()
        {
            sceneTransitionCanvas.FadeIn += OnFadeIn;
            sceneTransitionCanvas.InvokeFadeInAndOut();
        }

        // ReSharper disable Unity.PerformanceAnalysis
        private void OnFadeIn()
        {
            Reset?.Invoke();
            sceneTransitionCanvas.FadeIn -= OnFadeIn;
        }

        [YarnCommand("load_scene_nonblock")]
        public void InvokeLoadScene(string sceneName, float duration = 0)
        {
            StartCoroutine(LoadScene(sceneName, duration));
        }

        [YarnCommand("load_scene")]
        public static IEnumerator LoadScene(string sceneName, float duration = 0)
        {
            yield return new WaitForSecondsRealtime(duration);
            SceneManager.LoadScene(sceneName);
        }

        [YarnCommand("set_interactables_enabled")]
        public static void SetInteractablesEnabled(bool isEnabled)
        {
            Instance.AreInteractablesEnabled = isEnabled;
            Instance.OnInteractablesEnabledChanged?.Invoke();
        }

        /// <summary>
        /// This is probably a bad way to do this, whatever. Need to move the camera and Knitby too.
        /// </summary>
        [YarnCommand("move_player")]
        public static void ForceMovePlayer(float x, float y)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            player.transform.position = new Vector3(x, y);
        }
    }
}
