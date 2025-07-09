using Save;
using TMPro;
using UnityEngine;

namespace UI
{
    /// <summary>
    /// Main menu canvas logic.
    /// </summary>
    public class MainMenu : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI versionNumber;
        [SerializeField] private string levelSelectScene;

        private SaveDataManager _saveDataManager;
        
        private void Start()
        {
            _saveDataManager = FindObjectOfType<SaveDataManager>();
            if (_saveDataManager == null)
            {
                Debug.LogError("SaveDataManager not found in scene!");
            }
        }

        private void Awake()
        {
            versionNumber.text = Application.version;
#if UNITY_WEBGL
            Debug.LogWarning("Saving and loading isn't supported on WebGL yet.");
#endif
        }
        public void QuitGame()
        {
            Debug.Log("Quit!");
            Application.Quit();
        }
    }
}
