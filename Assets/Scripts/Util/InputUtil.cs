using System.Linq;
using UI;
using UnityEngine;

namespace Util
{
    /// <summary>
    /// Input utility class.
    /// </summary>
    public static class InputUtil
    {
        /// <summary>
        /// Whether a given screen position is on the pause button transform.
        /// </summary>
        /// <param name="screenPosition">Screen position</param>
        /// <returns></returns>
        private static bool OnPauseButton(Vector2 screenPosition)
        {
            return PauseMenu.Instance?.PauseButtonTransform &&
                   RectTransformUtility.RectangleContainsScreenPoint(PauseMenu.Instance.PauseButtonTransform, screenPosition);
        }
        
        /// <summary>
        /// Returns true if the player starts the jump button or there's any new touches in the screen.
        /// </summary>
        /// <returns>True if player started to jump (press space bar/touch screen)</returns>
        public static bool StartJumpOrTouch()
        {
            return Input.GetButtonDown("Jump") ||
                   (Time.timeScale > 0 && (
                       (!OnPauseButton(Input.mousePosition) && Input.GetButtonDown("Fire1")) ||
                       (Input.touches.Any(touch => touch.phase == TouchPhase.Began && !OnPauseButton(touch.position)))));
        }

        /// <summary>
        /// Returns true if player's holding the jump action (i.e. holding space or holding touches).
        /// </summary>
        /// <returns>True if player holds the jump action</returns>
        public static bool HoldJumpOrTouch()
        {
            return Input.GetButton("Jump") || Input.GetButton("Fire1") || Input.touchCount > 0;
        }
    }
}
