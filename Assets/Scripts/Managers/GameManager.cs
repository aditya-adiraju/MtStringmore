using System.Collections;
using System.Collections.Generic;
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
        public bool RespawnFacingLeft { get; private set; }
    
        /// <summary>
        /// Number of checkpoints reached.
        /// </summary>
        public List<Vector2> CheckpointsReached { get; } = new();
    
        /// <summary>
        /// The number of collectables collected.
        /// Should be reset to 0 after being displayed (e.g. after a end-of-level cutscene).
        /// </summary>
        public int NumCollected { get; set; }

        /// <summary>
        /// Action that gets invoked when level reloads, e.g. respawns
        /// </summary>
        public event Action Reset;

        /// <summary>
        /// Action invoked on game data changed.
        /// </summary>
        public event Action GameDataChanged;

        /// <summary>
        /// Canvas to fade in/out when transitioning between scenes
        /// </summary>
        [SerializeField] private FadeEffects sceneTransitionCanvas;
    
        private readonly HashSet<Vector2> _prevCheckpoints = new();
    
        /// <summary>
        /// So it turns out that onSceneChanged happens after modifying game data on save.
        ///
        /// So we need to check that we're doing that LOL.
        /// </summary>
        private bool _dontClearDataOnSceneChanged;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
        
            _instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void Update()
        {
            if (Input.GetButtonDown("Debug Reset")) Respawn();
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            sceneTransitionCanvas.InvokeFadeOut();
            Time.timeScale = 1f;
            if (!_dontClearDataOnSceneChanged)
            {
                CheckpointsReached.Clear();
                _prevCheckpoints.Clear();
                GameDataChanged?.Invoke();
            }
            _dontClearDataOnSceneChanged = false;
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
            CheckpointsReached.Add(newCheckpointLocation);
            GameDataChanged?.Invoke();
        }

        /// <summary>
        /// Sets checkpoint location/data from save data.
        /// </summary>
        /// <param name="shouldFaceLeft">Whether respawn should face left</param>
        /// <param name="checkpointsReached">List of previous checkpoints reached</param>
        public void UpdateFromSaveData(bool shouldFaceLeft, Vector2[] checkpointsReached)
        {
            if (checkpointsReached.Length > 0) CheckPointPos = checkpointsReached[^1];
            RespawnFacingLeft = shouldFaceLeft;
            CheckpointsReached.Clear();
            _prevCheckpoints.Clear();
            CheckpointsReached.AddRange(checkpointsReached);
            foreach (Vector2 checkpointReached in checkpointsReached)
                _prevCheckpoints.Add(checkpointReached);
            GameDataChanged?.Invoke();
            _dontClearDataOnSceneChanged = true;
        }

        /// <summary>
        /// Resets core components like player, Knitby, camera, etc to the state at
        /// the last checkpoint
        /// </summary>
        public void Respawn()
        {
            sceneTransitionCanvas.FadeIn += OnFadeIn;
            sceneTransitionCanvas.InvokeFadeInAndOut();
        }

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
    }
}
