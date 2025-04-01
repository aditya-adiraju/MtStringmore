using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Yarn.Unity;
using Action = System.Action;

/// <summary>
/// Singleton class for global game settings. Persists between scenes and reloads.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    /// <summary>
    /// Last checkpoint position. The player should respawn here if they die.
    /// </summary>
    public Vector2 CheckPointPos { get; set; }

    /// <summary>
    /// If true, the player faces left when they respawn
    /// </summary>
    public bool RespawnFacingLeft { get; set; }

    /// <summary>
    /// Action that gets invoked when level reloads, e.g. respawns
    /// </summary>
    public event Action Reset;

    /// <summary>
    /// Canvas to fade in/out when transitioning between scenes
    /// </summary>
    [SerializeField] private FadeEffects sceneTransitionCanvas;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
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
