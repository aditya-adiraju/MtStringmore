using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// Behaviour to display the pressed key.
    /// </summary>
    public class TutorialMenuKeyDisplay : MonoBehaviour
    {
        [SerializeField] private Image image;
        [SerializeField] private Sprite[] desktopKeySprites;
        [SerializeField] private Sprite[] mobileKeySprites;

        private void Awake()
        {
            image.sprite = SystemInfo.deviceType == DeviceType.Desktop ? desktopKeySprites[0] : mobileKeySprites[0];
        }

        /// <summary>
        /// Sets the displayed sprite via index.
        ///
        /// Called by an Animation Event.
        /// </summary>
        /// <param name="index">Index of the sprite</param>
        public void SetPressed(int index)
        {
            image.sprite = SystemInfo.deviceType == DeviceType.Desktop ? desktopKeySprites[index] : mobileKeySprites[index];
        }
    }
}
