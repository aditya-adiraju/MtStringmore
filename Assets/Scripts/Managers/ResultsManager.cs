using Interactables;
using Save;
using UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Managers
{
    public class ResultsManager : MonoBehaviour
    {
        [SerializeField] private Checkpoint finalCheckpoint;

        [Header("Results Page")]
        [SerializeField]
        private ResultsWindow resultsWindow;
        
        private SaveDataManager _saveDataManager;
        private GameManager _gameManager;

        public static bool isResultsPageOpen;

        private void Start()
        {
            _saveDataManager = FindObjectOfType<SaveDataManager>();
            _gameManager = GameManager.Instance;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (finalCheckpoint)
            {
                finalCheckpoint.onCheckpointReached.RemoveListener(HandleFinalCheckpointHit);
                finalCheckpoint = null;
            }
            
            GameObject checkpointObj = GameObject.FindWithTag("FinalCheckpoint");
            if (checkpointObj)
            {
                finalCheckpoint = checkpointObj.GetComponent<Checkpoint>();
                if (finalCheckpoint != null) finalCheckpoint.onCheckpointReached.AddListener(HandleFinalCheckpointHit);
            }
            else finalCheckpoint = null;
            
            resultsWindow.gameObject.SetActive(false);
            isResultsPageOpen = false;
            _saveDataManager = FindObjectOfType<SaveDataManager>();
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        
        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            if (finalCheckpoint != null) finalCheckpoint.onCheckpointReached.RemoveListener(HandleFinalCheckpointHit);
        }

        private void HandleFinalCheckpointHit()
        {
            Time.timeScale = 0;
            FindObjectOfType<LastCheckpoint>()?.UpdateLevelAccess();
            _gameManager.ThisLevelTime = TimerManager.ElapsedLevelTimeString;
            _gameManager.SaveGame();
            resultsWindow.gameObject.SetActive(true);
            resultsWindow.UpdateDisplay();
            isResultsPageOpen = true;
            _saveDataManager.SaveFile();
            PauseMenu.IsPauseDisabled = true;
        }

        public void RestartLevel() 
        {
            Time.timeScale = 1f;
            _gameManager.ResetCandyCollected();
            isResultsPageOpen = false;
            PauseMenu.IsPauseDisabled = false;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        
        public void LoadMainMenu()
        {
            Time.timeScale = 1f;
            PauseMenu.IsPauseDisabled = false;
            SceneListManager.Instance.LoadMainMenu();
        }

        public void LoadNextLevel()
        {
            resultsWindow.gameObject.SetActive(false);
            isResultsPageOpen = false;
            Time.timeScale = 1f;
            PauseMenu.IsPauseDisabled = false;
            _gameManager.ResetCandyCollected();
            finalCheckpoint.StartConversation();
        }
    }
}
