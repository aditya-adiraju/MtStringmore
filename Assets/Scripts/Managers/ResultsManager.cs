using Save;
using UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Managers
{
    public class ResultsManager : MonoBehaviour
    {
        public static bool isResultsPageOpen;

        [Header("Results Page")]
        [SerializeField]
        private ResultsWindow resultsWindow;
        
        private GameManager _gameManager;
        private LastCheckpoint _lastCheckpoint;

        private void Start()
        {
            _gameManager = GameManager.Instance;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (_lastCheckpoint)
            {
                _lastCheckpoint.AttachedCheckpoint.onCheckpointReached.RemoveListener(HandleFinalCheckpointHit);
                _lastCheckpoint = null;
            }

            _lastCheckpoint = FindAnyObjectByType<LastCheckpoint>();
            if (_lastCheckpoint)
            {
                _lastCheckpoint.AttachedCheckpoint.onCheckpointReached.AddListener(HandleFinalCheckpointHit);
            }
            
            resultsWindow.gameObject.SetActive(false);
            isResultsPageOpen = false;
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        
        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            if (_lastCheckpoint) _lastCheckpoint.AttachedCheckpoint.onCheckpointReached.RemoveListener(HandleFinalCheckpointHit);
        }

        private void HandleFinalCheckpointHit()
        {
            Time.timeScale = 0;
            if (_lastCheckpoint) _lastCheckpoint.UpdateLevelAccess();
            _gameManager.ThisLevelTime = TimerManager.ElapsedLevelTime;
            _gameManager.SaveGame();
            resultsWindow.gameObject.SetActive(true);
            resultsWindow.UpdateDisplay();
            isResultsPageOpen = true;
            FindAnyObjectByType<OnSceneButtons>().SetRestartButtonState(false);
            SaveDataManager.SaveFile();
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
            _lastCheckpoint.AttachedCheckpoint.StartConversation();
        }
    }
}
