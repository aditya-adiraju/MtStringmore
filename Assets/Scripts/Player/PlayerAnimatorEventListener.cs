using JetBrains.Annotations;
using UnityEngine;

namespace Player
{
    /// <summary>
    /// Player sprite animator event listener.
    ///
    /// Listens to events (e.g. walk PlaySound events).
    /// </summary>
    public class PlayerAnimatorEventListener : MonoBehaviour
    {
        [SerializeField] private PlayerAnimator playerAnimator;

        /// <summary>
        /// Handles the event on the animation frame where the player steps.
        /// </summary>
        /// <remarks>
        /// Called implicitly by an animator event.
        /// </remarks>
        [UsedImplicitly]
        public void HandleStep() => playerAnimator.HandleStep();
    }
}
