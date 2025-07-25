using Managers;
using Player;
using StringmoreCamera;
using UnityEngine;
using UnityEngine.Events;

namespace Interactables.LetterBlock
{
    /// <summary>
    /// Object that is 'destroyed' on dash. And by destroy, I mean disable the collider.
    ///
    /// If there's a trigger, it also detects if the player enters the trigger.
    /// </summary>
    [RequireComponent(typeof(Collider2D), typeof(AudioSource))]
    public class DashDestructibleObject : MonoBehaviour
    {
        [SerializeField] private bool destroyed;
        [SerializeField] private UnityEvent onDestroyed;
        [Header("Camera Shake")]
        [SerializeField] private float shakeDuration;
        [SerializeField] private float shakeIntensity;
        [SerializeField, Tooltip("Set to true for shake along x-axis")] private bool xShake;
        [SerializeField, Tooltip("Set to true for shake along y-axis")] private bool yShake;
        [SerializeField, Tooltip("Set to true if object is destructible")] private bool destructibleObject;
        [SerializeField] private Collider2D solidCollider;
        private AudioSource _audioSource;
        private ShakeCamera _shake;
        
        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            GameManager.Instance.Reset += OnReset;
            _shake = FindObjectOfType<ShakeCamera>(); 
        }

        /// <summary>
        /// Called on reset: re-enables the collider.
        /// </summary>
        private void OnReset()
        {
            destroyed = false;
            solidCollider.enabled = true;
        }

        private void OnDisable()
        {
            GameManager.Instance.Reset -= OnReset;
        }

        /// <summary>
        /// Determines whether the player should 'destroy' this object (is dashing or was recently dashing).
        /// </summary>
        /// <param name="player">Player in question</param>
        /// <returns>True if the player is/was recently dashing.</returns>
        /// <remarks>
        /// To avoid the player destroying this object while walking after having just dashed,
        /// we check for whether they're in the air as well.
        /// </remarks>
        private static bool ShouldPlayerDestroy(PlayerController player)
        {
            return player.PlayerState switch
            {
                PlayerController.PlayerStateEnum.Dash => true,
                PlayerController.PlayerStateEnum.Air => Time.time - player.TimeDashEnded <= player.DashEndTolerance,
                _ => false
            };
        }

        /// <summary>
        /// "Destroys" this object.
        /// </summary>
        private void DestroyObject()
        {
            _shake?.Shake(shakeDuration, shakeIntensity, xShake, yShake,destructibleObject);
            destroyed = true;
            solidCollider.enabled = false;
            _audioSource.Play();
            onDestroyed?.Invoke();
        }

        /// <summary>
        /// Called when the player collides with this object.
        /// </summary>
        /// <param name="player">Colliding player</param>
        public void OnPlayerCollide(PlayerController player)
        {
            if (destroyed) return;
            if (ShouldPlayerDestroy(player))
            {
                DestroyObject();
            }
        }

        /// <summary>
        /// Destroys this object when there's player motion in the trigger.
        /// </summary>
        /// <param name="other">Other collider</param>
        private void TriggerDestructionLogic(Collider2D other)
        {
            if (destroyed) return;
            if (other.TryGetComponent(out PlayerController playerController) &&
                // because in some wacky cases you move in trigger while moving in the opposite direction
                Vector2.Dot(transform.position - playerController.transform.position, playerController.Velocity) > 0 &&
                ShouldPlayerDestroy(playerController))
            {
            
                DestroyObject();
            }
        }
        
        // hack to allow tolerance without collisions
        private void OnTriggerEnter2D(Collider2D other)
        {
            TriggerDestructionLogic(other);
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            TriggerDestructionLogic(other);
        }
    }
}
