using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    private static bool _gameIsPaused;
    [SerializeField] private GameObject pauseMenuUI;
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    private float _prevTimescale;

    private void Start()
    {
        _prevTimescale = Time.timeScale;
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

    private void Resume()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = _prevTimescale;
        _gameIsPaused = false;

        // Unmute audio using SoundManager
        SoundManager.Instance.SetMute(false);
    }

    private void Pause()
    {
        pauseMenuUI.SetActive(true);
        _prevTimescale = Time.timeScale;
        Time.timeScale = 0f;
        _gameIsPaused = true;

        // Mute audio using SoundManager
        SoundManager.Instance.SetMute(true);
    }

    public void LoadMenu()
    {
        // reset any changes made by pausing
        Resume();
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}