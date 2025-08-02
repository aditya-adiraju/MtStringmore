using Managers;
using Save;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    ///     Behaviour for the Pause menu element.
    ///     It's always present, even in the main menu.
    /// </summary>
    public class PauseMenu : MonoBehaviour
    {
        private static PauseMenu _instance;

        private static bool _gameIsPaused;

        /// <summary>
        /// Whether the pause menu is forced-disabled (e.g. on end of level UI).
        /// </summary>
        public static bool IsPauseDisabled { get; set; }

        [SerializeField] private GameObject pauseMenuUI;
        [SerializeField] private Button pauseButton;
        [SerializeField] private TextMeshProUGUI versionNumber;
        private float _prevTimescale;
        private SaveDataManager _saveDataManager;
        
        /// <summary>
        /// Whether we can open pause menu: i.e. not main menu and not force disabled
        /// </summary>
        private static bool CanOpenPauseMenu => !SceneListManager.Instance.InMainMenu && !IsPauseDisabled;

        /// <summary>
        ///     Gets the "singleton" instance of the pause menu.
        /// </summary>
        public static PauseMenu Instance
        {
            get
            {
                if (!_instance) _instance = FindObjectOfType<PauseMenu>();
                return _instance;
            }
        }

        /// <summary>
        ///     Gets the pause button's rect transform.
        /// </summary>
        public RectTransform PauseButtonTransform => pauseButton.transform as RectTransform;

        private void Awake()
        {
            _prevTimescale = Time.timeScale;
            _saveDataManager = FindObjectOfType<SaveDataManager>();
            versionNumber.text = Application.version;
            Resume();
            SceneManager.activeSceneChanged += OnSceneChanged;
        }

        private void Update()
        {
            if (!Input.GetKeyDown(KeyCode.Escape) || !CanOpenPauseMenu) return;
            if (_gameIsPaused)
                Resume();
            else
                Pause();
        }

        private void OnDestroy()
        {
            SceneManager.activeSceneChanged -= OnSceneChanged;
        }

        private void OnApplicationPause(bool paused)
        {
            if (!_gameIsPaused && paused && CanOpenPauseMenu) Pause();
        }

        private void OnSceneChanged(Scene current, Scene next)
        {
            pauseButton.gameObject.SetActive(!SceneListManager.Instance.IsMainMenu(next.name)) ;
        }

        /// <summary>
        ///     Resumes the game by hiding the UI and resetting the timescale.
        ///     Called by both the resume button and when the player hits the Escape key.
        /// </summary>
        private void Resume()
        {
            pauseMenuUI.SetActive(false);
            pauseButton.gameObject.SetActive(true);
            Time.timeScale = _prevTimescale;
            _gameIsPaused = false;

            // Unmute audio using SoundManager
            SoundManager.Instance.SetMute(false);
        }

        /// <summary>
        ///     Pauses the game: zeroes the timescale and enables the UI.
        /// </summary>
        private void Pause()
        {
            pauseMenuUI.SetActive(true);
            pauseButton.gameObject.SetActive(false);
            _prevTimescale = Time.timeScale;
            Time.timeScale = 0f;
            _gameIsPaused = true;

            // Mute audio using SoundManager
            SoundManager.Instance.SetMute(true);
        }

        /// <summary>
        ///     Called on reset button press.
        /// </summary>
        public void ResetButtonPressed()
        {
            Resume();
            GameManager.Instance.Respawn();
        }

        /// <summary>
        ///     Called by the UI element to go to the main menu.
        ///     Saves and loads the main menu.
        /// </summary>
        public void LoadMenu()
        {
            _saveDataManager?.SaveFile();
            // reset any changes made by pausing
            Resume();
            SceneListManager.Instance.LoadMainMenu();
        }

        /// <summary>
        ///     Called by the UI element to quit the game.
        ///     Saves and quits.
        /// </summary>
        public void QuitGame()
        {
            _saveDataManager?.SaveFile();
            Application.Quit();
        }
    }
}
