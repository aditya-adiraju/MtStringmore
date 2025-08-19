using Managers;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UI
{
    public class OnSceneButtons : MonoBehaviour
    {
        [SerializeField] private Button pauseButton;
        [SerializeField] private Button restartButton;
    
        private void Awake()
        {
            SceneManager.activeSceneChanged += OnSceneChanged;
        }
    
        private void OnDestroy()
        {
            SceneManager.activeSceneChanged -= OnSceneChanged;
        }

        /// <summary>
        /// Sets whether the restart button is active and enabled.
        /// </summary>
        /// <param name="active">Whether the restart button should be active</param>
        public void SetRestartButtonState(bool active)
        {
            restartButton.gameObject.SetActive(active);
        }
    
        private void OnSceneChanged(Scene current, Scene next)
        {
            bool isMainMenu = SceneListManager.Instance.InMainMenu;
            bool isCutscene = SceneListManager.Instance.InCutscene;

            pauseButton.gameObject.SetActive(!isMainMenu);
            SetRestartButtonState(!isMainMenu && !isCutscene);
        }
    }
}
