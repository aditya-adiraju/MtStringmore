using Player;
using UnityEngine;

namespace Interactables
{
    /// <summary>
    /// Class represents a trampoline that is a trigger
    /// </summary>
    [DisallowMultipleComponent, RequireComponent(typeof(Collider2D))]
    public class Trampoline : MonoBehaviour, IPlayerVelocityEffector
    {
        #region Serialized Public Fields

        [Header("Bouncing")]
        [SerializeField] public float yBounceForce;
        [SerializeField] public float xBounceForce;

        #endregion

        private PlayerController _player;

        /// <inheritdoc />
        /// <remarks>
        /// Temporarily "turn off gravity" for auto trampoline bounce
        /// </remarks>
        public bool IgnoreGravity => true;

        /// <inheritdoc />
        public Vector2 ApplyVelocity(Vector2 velocity)
        {
            return new Vector2(xBounceForce * Mathf.Sign(velocity.x), yBounceForce);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.TryGetComponent(out _player)) return;
            _player.AddPlayerVelocityEffector(this);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!other.TryGetComponent(out PlayerController _)) return;
            _player.RemovePlayerVelocityEffector(this);
        }
    }
}
