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

        private void Awake()
        {
            versionNumber.text = Application.version;
        }

        /// <summary>
        /// Quits the game. Called by the quit button.
        /// </summary>
        public void QuitGame()
        {
            Debug.Log("Quit!");
            Application.Quit();
        }
    }
}
