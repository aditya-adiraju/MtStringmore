using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Singleton class for global game settings. Persists between scenes and reloads.
/// </summary>
public class GameManager : MonoBehaviour {
    public static GameManager Instance { get; private set; }

    /// <summary>
    /// Last checkpoint position. The player should respawn here if they die
    /// </summary>
    [SerializeField] public Vector2 CheckPointPos;

    private void Awake() {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        } else {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Reloads the current scene
    /// </summary>
    public void Respawn()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}