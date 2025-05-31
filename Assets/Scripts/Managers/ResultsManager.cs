using System;
using Interactables;
using Save;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Managers
{
    public class ResultsManager : MonoBehaviour
    {
        [SerializeField] private Checkpoint finalCheckpoint;
        
        [Header("Results Page")]
        [SerializeField] private GameObject resultsPane;
        
        [SerializeField] private TextMeshProUGUI levelHeaderText;

        [SerializeField] private TextMeshProUGUI collectableResultsText;

        private int maxCount;
        
        private SaveDataManager _saveDataManager;
        
        public static bool isResultsPageOpen = false;
        
        private void Start()
        {
            maxCount = FindObjectsOfType<Collectable>().Length;
            levelHeaderText.text = "Level " + SceneManager.GetActiveScene().buildIndex / 2 + " Complete!";
            resultsPane.SetActive(false);
            _saveDataManager = FindObjectOfType<SaveDataManager>();
        }

        private void OnEnable()
        {
            finalCheckpoint.OnCheckpointHit += HandleFinalCheckpointHit;
        }
        
        private void OnDisable()
        {
            finalCheckpoint.OnCheckpointHit -= HandleFinalCheckpointHit;
        }

        private void HandleFinalCheckpointHit()
        {
            UpdateCollectableCount();
            EndLevel();
        }

        private void UpdateCollectableCount()
        {
            int collectedCount = GameManager.Instance.NumCollected;
            collectableResultsText.text = collectedCount + " / " + maxCount;
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
            GameManager.Instance.NumCollected = 0;
            isResultsPageOpen = false;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        
        public void LoadMainMenu()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene("MainMenu");
        }

        public void LoadNextLevel()
        {
            resultsPane.SetActive(false);
            isResultsPageOpen = false;
            Time.timeScale = 1f;
            GameManager.Instance.NumCollected = 0;
            finalCheckpoint.StartConversation();
        }
    }
}
