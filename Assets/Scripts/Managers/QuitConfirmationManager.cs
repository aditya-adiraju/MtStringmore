using UI;
using UnityEngine;

namespace Managers
{
    public class QuitConfirmationManager : MonoBehaviour
    {
        [SerializeField, Tooltip("The quit confirmation canvas")]
        private GameObject confirmationCanvas;
        [SerializeField, Tooltip("The pause menu game object")] 
        private PauseMenu pauseMenu;
        private static QuitConfirmationManager _instance;
    
        /// <summary>
        ///     Gets the "singleton" instance of the pause menu.
        /// </summary>
        public static QuitConfirmationManager Instance
        {
            get
            {
                if (!_instance) _instance = FindObjectOfType<QuitConfirmationManager>();
                return _instance;
            }
        }
    
        /// <summary>
        /// Show the confirmation panel
        /// </summary>
        public void ShowConfirmation()
        {
            confirmationCanvas.SetActive(true);
        }

        /// <summary>
        /// Quitting out of the game
        /// </summary>
        public void ConfirmQuitGame()
        {
            confirmationCanvas.SetActive(false);
            //Resets the state of the pause menu (i.e. inactive)
            pauseMenu.Resume();
            Debug.Log("User is going to main menu!"); 
            SceneListManager.Instance.LoadMainMenu();
        }

        /// <summary>
        /// Hides the confirmation panel when user pressed "No"
        /// </summary>
        public void CancelQuit()
        {
            confirmationCanvas.SetActive(false);
        }
    }
}
