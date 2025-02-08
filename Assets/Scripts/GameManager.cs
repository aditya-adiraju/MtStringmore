using UnityEngine;
using UnityEngine.SceneManagement;
using Yarn.Unity;

/// <summary>
///     Singleton class for global game settings. Persists between scenes and reloads.
/// </summary>
public class GameManager : MonoBehaviour
{
    /// <summary>
    ///     Used between scene transitions. Set to false for respawns, true for transition between levels
    /// </summary>
    private bool _newLevel;

    public static GameManager Instance { get; private set; }

    /// <summary>
    ///     Last checkpoint position. The player should respawn here if they die.
    /// </summary>
    public Vector2 CheckPointPos { get; set; }

    private void Awake()
    {
        if (Instance == null)
        {
            _newLevel = true;
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

    /// <summary>
    ///     On scene load, put player in the right spawn/respawn point
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Time.timeScale = 1f;
        var player = GameObject.FindGameObjectWithTag("Player");
        var knitby = GameObject.FindGameObjectWithTag("Knitby");
        if (!player || !knitby) return;

        if (_newLevel == false)
        {
            var spawnPos = new Vector3(CheckPointPos.x, CheckPointPos.y, player.transform.position.z);
            player.transform.position = spawnPos;
            knitby.transform.position = spawnPos;
        }
        else
        {
            CheckPointPos = player.transform.position;
        }

        var cam = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<FollowCamera>();
        if (cam is null) return;
        var playerTarget = cam.GetPlayerTarget();
        cam.transform.position = new Vector3(playerTarget.x, playerTarget.y, cam.transform.position.z);
    }

    public void Respawn()
    {
        _newLevel = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    [YarnCommand("load_scene")]
    public static void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}