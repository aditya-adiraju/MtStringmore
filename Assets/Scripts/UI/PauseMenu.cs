using Managers;
using Save;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// Behaviour for the Pause menu element.
    ///
    /// It's always present, even in the main menu.
    /// </summary>
    public class PauseMenu : MonoBehaviour
    {
        private static PauseMenu _instance;

        /// <summary>
        /// Gets the "singleton" instance of the pause menu.
        /// </summary>
        /// <remarks>
        /// Maybe I should do a lifetime check?
        /// </remarks>
        public static PauseMenu Instance => _instance ??= FindObjectOfType<PauseMenu>();

        private static bool _gameIsPaused;
        [SerializeField] private GameObject pauseMenuUI;
        [SerializeField] private Button pauseButton;
        [SerializeField] private TextMeshProUGUI versionNumber;
        [SerializeField] private string mainMenuSceneName = "MainMenu";
        private float _prevTimescale;
        private SaveDataManager _saveDataManager;

        /// <summary>
        /// Gets the pause button's rect transform.
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

        private void OnDestroy()
        {
            SceneManager.activeSceneChanged -= OnSceneChanged;
        }

        private void Update()
        {
            if (!Input.GetKeyDown(KeyCode.Escape) || SceneManager.GetActiveScene().name == mainMenuSceneName) return;
            if (_gameIsPaused)
                Resume();
            else
                Pause();
        }

        private void OnApplicationPause(bool paused)
        {
            if (!_gameIsPaused && paused && SceneManager.GetActiveScene().name != mainMenuSceneName)
            {
                Pause();
            }
        }

        private void OnSceneChanged(Scene current, Scene next)
        {
            pauseButton.gameObject.SetActive(next.name != mainMenuSceneName);
        }

        /// <summary>
        /// Resumes the game by hiding the UI and resetting the timescale.
        ///
        /// Called by both the resume button and when the player hits the Escape key.
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
        /// Pauses the game: zeroes the timescale and enables the UI.
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
        /// Called on reset button press.
        /// </summary>
        public void ResetButtonPressed()
        {
            Resume();
            GameManager.Instance.Respawn();
        }

        /// <summary>
        /// Called by the UI element to go to the main menu.
        ///
        /// Saves and loads the main menu.
        /// </summary>
        public void LoadMenu()
        {
            _saveDataManager?.SaveFile();
            // reset any changes made by pausing
            Resume();
            SceneManager.LoadScene(mainMenuSceneName);
        }

        /// <summary>
        /// Called by the UI element to quit the game.
        ///
        /// Saves and quits.
        /// </summary>
        public void QuitGame()
        {
            _saveDataManager?.SaveFile();
            Application.Quit();
        }
    }
}
