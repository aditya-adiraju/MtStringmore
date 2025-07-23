using Interactables;
using Save;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Managers
{
    public class ResultsManager : MonoBehaviour
    {
        [SerializeField] private Checkpoint finalCheckpoint;
        
        [Header("Results Page")]
        [SerializeField] private GameObject resultsPane;
        
        [SerializeField] private TextMeshProUGUI levelHeaderText;

        [SerializeField] private TextMeshProUGUI collectableResultsText, deathsText, timerText;

        
        private int maxCount;
        
        private SaveDataManager _saveDataManager;
        private GameManager _gameManager;

        public static bool isResultsPageOpen;

        private void Start()
        {
            _saveDataManager = FindObjectOfType<SaveDataManager>();
            _gameManager = GameManager.Instance;
            maxCount = _gameManager.MaxCollectablesCount;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (finalCheckpoint)
            {
                finalCheckpoint.OnCheckpointHit -= HandleFinalCheckpointHit;
                finalCheckpoint = null;
            }
            
            GameObject checkpointObj = GameObject.FindWithTag("FinalCheckpoint");
            if (checkpointObj)
            {
                finalCheckpoint = checkpointObj.GetComponent<Checkpoint>();
                if (finalCheckpoint != null) finalCheckpoint.OnCheckpointHit += HandleFinalCheckpointHit;
            }
            else finalCheckpoint = null;
            
            maxCount = FindObjectsOfType<Collectable>().Length;
            levelHeaderText.text = "Level " + SceneManager.GetActiveScene().buildIndex / 2 + " Complete!";
            resultsPane.SetActive(false);
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
            if (finalCheckpoint != null) finalCheckpoint.OnCheckpointHit -= HandleFinalCheckpointHit;
        }

        private void HandleFinalCheckpointHit()
        {
            FindObjectOfType<LastCheckpoint>()?.UpdateLevelAccess();
            UpdateCollectableCount();
            UpdateDeathsCount();
            UpdateTimerCount();
            SaveGame();
            EndLevel();
            PauseMenu.IsPauseDisabled = true;
        }

        private void SaveGame()
        {
            _gameManager.LevelCompleted();
        }

        private void UpdateCollectableCount()
        {
            int collectedCount = _gameManager.NumCollectablesCollected;
            collectableResultsText.text = collectedCount + " / " + maxCount;
        }
        
        private void UpdateDeathsCount()
        {
            int deaths = _gameManager.thisLevelDeaths;
            //if uninstantiated and still null value
            if (deaths == -1)
                deaths = 0;
            deathsText.text = deaths.ToString();
        }

        private void UpdateTimerCount()
        {
            string time = TimerManager.ElapsedLevelTimeString;
            _gameManager.ThisLevelTime = time;
            timerText.text = time;
            Debug.Log("Current GameManagerTimer: " + time);
        }

        private void EndLevel()
        {
            Time.timeScale = 0f;
            resultsPane.SetActive(true); 
            isResultsPageOpen = true;
            _saveDataManager.SaveFile();
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
            SceneManager.LoadScene("MainMenu");
        }

        public void LoadNextLevel()
        {
            resultsPane.SetActive(false);
            isResultsPageOpen = false;
            Time.timeScale = 1f;
            PauseMenu.IsPauseDisabled = false;
            _gameManager.ResetCandyCollected();
            finalCheckpoint.StartConversation();
        }
    }
}
