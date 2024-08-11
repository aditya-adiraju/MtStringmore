using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Singleton class for global game settings. Persists between scenes and reloads.
/// </summary>
public class GameManager : MonoBehaviour {
    public static GameManager Instance { get; private set; }

    /// <summary>
    /// Last checkpoint position. The player should respawn here if they die.
    /// </summary>
    public Vector2 CheckPointPos;

    /// <summary>
    /// Used between scene transitions. Set to false for respawns, true for transition between levels
    /// </summary>
    private bool _newLevel;

    private void Awake() {
        if (Instance == null)
        {
            _newLevel = true;
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded; 
        } else {
            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    /// <summary>
    /// On scene load, put player in the right spawn/respawn point
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (_newLevel == false)
        {
            Vector3 spawnPos = new Vector3(CheckPointPos.x, CheckPointPos.y, player.transform.position.z);
            player.transform.position = spawnPos;
        }
        else
        {
            CheckPointPos = player.transform.position;
        }
        
        var cam = GameObject.FindGameObjectWithTag("MainCamera");
        Vector3 camPos = new Vector3(CheckPointPos.x, CheckPointPos.y, cam.transform.position.z);
        cam.transform.position = camPos;
    }

    public void Respawn()
    {
        _newLevel = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}