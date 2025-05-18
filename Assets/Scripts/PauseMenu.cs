using Save;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Behaviour for the Pause menu element.
///
/// It's always present, even in the main menu.
/// </summary>
public class PauseMenu : MonoBehaviour
{
    private static bool _gameIsPaused;
    [SerializeField] private GameObject pauseMenuUI;
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    private float _prevTimescale;
    private SaveDataManager _saveDataManager;

    private void Start()
    {
        _prevTimescale = Time.timeScale;
        _saveDataManager = FindObjectOfType<SaveDataManager>();
        Resume();
    }

    private void Update()
    {
        if (!Input.GetKeyDown(KeyCode.Escape) || SceneManager.GetActiveScene().name == mainMenuSceneName) return;
        if (_gameIsPaused)
            Resume();
        else
            Pause();
    }

    /// <summary>
    /// Resumes the game by hiding the UI and resetting the timescale.
    ///
    /// Called by both the resume button and when the player hits the Escape key.
    /// </summary>
    private void Resume()
    {
        pauseMenuUI.SetActive(false);
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
        _prevTimescale = Time.timeScale;
        Time.timeScale = 0f;
        _gameIsPaused = true;

        // Mute audio using SoundManager
        SoundManager.Instance.SetMute(true);
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
