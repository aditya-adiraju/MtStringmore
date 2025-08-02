using Managers;
using Player;
using UnityEngine;

namespace Interactables
{
    /// <summary>
    /// Simple utility to have a collider/trigger that kills the player.
    ///
    /// Unlike the 'Death' tag, this can be toggled off via <see cref="MonoBehaviour.enabled"/>.
    /// </summary>
    /// <remarks>
    /// Unsure if there's cases where we want the collider to collide but not kill, since the tags don't support that.
    /// Whatever.
    /// </remarks>
    [RequireComponent(typeof(Collider2D))]
    public class KillingCollider : MonoBehaviour
    {
        [SerializeField, Tooltip("Whether to kill when enabled (default, false) or kill always (true)")]
        private bool ignoreEnabledCheck;

        private bool _startEnabled;

        /// <summary>
        /// Records the initial enabled state on start.
        /// </summary>
        private void Start()
        {
            _startEnabled = enabled;
            GameManager.Instance.Reset += OnReset;
        }

        private void OnDestroy()
        {
            GameManager.Instance.Reset -= OnReset;
        }

        /// <summary>
        /// Kills the player on (non-trigger) collision enter (if the object is a player).
        /// </summary>
        /// <param name="other">Collision information</param>
        private void OnCollisionEnter2D(Collision2D other)
        {
            KillCollider(other.collider);
        }
        
        /// <summary>
        /// Kills the player on them entering this trigger.
        /// </summary>
        /// <param name="other"></param>
        private void OnTriggerEnter2D(Collider2D other)
        {
            KillCollider(other);
        }

        /// <summary>
        /// Kills the player if they're on the provided collider and killing is enabled.
        /// </summary>
        /// <param name="other">Other collider</param>
        private void KillCollider(Collider2D other)
        {
            if (!enabled && !ignoreEnabledCheck) return;
            PlayerController playerController = other.GetComponent<PlayerController>();
            if (playerController == null) return;
            playerController.TryKill();
        }

        /// <summary>
        /// Resets the enabled state to what it was on start.
        /// </summary>
        private void OnReset()
        {
            enabled = _startEnabled;
        }
    }
}
