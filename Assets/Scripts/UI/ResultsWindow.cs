using Managers;
using TMPro;
using UnityEngine;

namespace UI
{
    /// <summary>
    /// Main behaviour for the results window.
    /// </summary>
    public class ResultsWindow : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI levelHeaderText;
        [SerializeField] private TextMeshProUGUI timerText;
        [SerializeField] private CountUpText collectableResultsText;
        [SerializeField] private CountUpText deathsText;
        
        /// <summary>
        /// Updates the displayed values.
        /// </summary>
        public void UpdateDisplay()
        {
            levelHeaderText.text = $"Level {SceneListManager.Instance.LevelNumber} Complete!";
            collectableResultsText.RunCountUpForCollectible(GameManager.Instance.NumCollectablesCollected,
                GameManager.Instance.MaxCollectablesCount);
            
            int deaths = GameManager.Instance.thisLevelDeaths;
            //if uninstantiated and still null value
            if (deaths == -1)
            {
                deaths = 0;
                Debug.LogWarning("GameManager death count uninstantiated?");
            }
            deathsText.RunCountUpWithRate(deaths);
            timerText.text = TimerManager.ElapsedLevelTimeString;
        }
    }
}
