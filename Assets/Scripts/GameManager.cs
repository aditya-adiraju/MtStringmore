using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Singleton class for global game settings. Persists between scenes and reloads.
/// </summary>
public class GameManager : MonoBehaviour {
    /// <summary>
    /// Returns an instance of the Game Manager singleton.
    /// </summary>
    public static GameManager Instance
    {
        get
        {
            if (!_instance)
                _instance = FindObjectOfType(typeof(GameManager)) as GameManager;
            DontDestroyOnLoad(_instance?.gameObject);
            return _instance;
        }
    }

    private static GameManager _instance;

    /// <summary>
    /// Last checkpoint position. The player should respawn here if they die
    /// </summary>
    [SerializeField] public Vector2 CheckPointPos;

    /// <summary>
    /// Reloads the current scene
    /// </summary>
    public void Respawn()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}