using Player;
using UnityEngine;
namespace Interactables
{
    [RequireComponent(typeof(BoxCollider2D), typeof(ParticleSystem), typeof(AudioSource))]
    [ExecuteAlways]
    public class WindController : MonoBehaviour, IPlayerVelocityEffector
    {
        #region Serialized Private Fields

        [Header("Wind Settings")]
        [SerializeField, Tooltip("Player speed moving with wind")] private float tailwindSpeed;
        [SerializeField, Tooltip("Player speed moving against wind")] private float headwindSpeed;
        [SerializeField, Tooltip("Player deceleration in headwind")] private float headwindDecl = 20f;
        [SerializeField, Tooltip("Player acceleration in tailwind")] private float tailwindAccel = 50f;
        [SerializeField, Tooltip("Updates both collider2D size and particle region size")] private Vector2 windZoneSize;  
        [SerializeField] private ParticleSystem windParticles;
        [SerializeField] private BoxCollider2D boxCollider;
        [SerializeField] private ParticleSystem childParticleSystem;
        [Header("Sounds")] 
        [SerializeField, Tooltip("How quickly the wind audio fades")] private float fadeSpeed;
        [SerializeField] private AudioSource audioSource;
        #endregion
       
        #region Private Properties

        private PlayerController _player;
        private bool _playerInside;
        private bool _shouldFadeOut;
        private Vector2 _windDirNormalized => transform.right.normalized;
        #endregion

        // Called automatically in editor when a serialized field changes
        private void OnValidate()
        {

            UpdateParticles();
        }

        private void Awake()
        {
            UpdateParticles();
        }

        private void Update()
        {
            if (audioSource == null) return;

            float targetVolume = _playerInside ? 1f : 0f;
            audioSource.volume = Mathf.MoveTowards(audioSource.volume, targetVolume, fadeSpeed * Time.deltaTime);

            // Stop audio when fully faded out
            if (!_playerInside && _shouldFadeOut && Mathf.Approximately(audioSource.volume, 0f))
            {
                audioSource.Stop();
                _shouldFadeOut = false; 
            }
        }

        /// <inheritdoc />
        /// <remarks>
        /// Wind will not change player velocity if on swing or trampoline etc
        /// </remarks>
        public bool IgnoreOtherEffectors => false;
        
        /// <inheritdoc />
        public bool EffectPlayerWalkSpeed => true;
        public bool AllowPlayerDashing => true;

        /// <inheritdoc />
        public Vector2 ApplyVelocity(Vector2 velocity)
        {
            // Don't apply wind for specified player states
            if (_player == null || _player.PlayerState == PlayerController.PlayerStateEnum.Swing || _player.PlayerState == PlayerController.PlayerStateEnum.OnObject)
            {
                return velocity;
            }
            
            float dotProduct = Vector2.Dot(velocity.normalized, _windDirNormalized);
            float windSpeed;
            float windRoc;
            
            if (dotProduct <= 0) // Moving against the wind
            {
                windSpeed = headwindSpeed;
                windRoc = headwindDecl;
            }
            else // Moving with the wind
            {
                windSpeed = tailwindSpeed;
                windRoc = tailwindAccel;
            }
            
            Vector2 target = _windDirNormalized * windSpeed;
            velocity = (_windDirNormalized.y == 0)
                ? new Vector2(Mathf.MoveTowards(velocity.x, target.x, Time.fixedDeltaTime * windRoc), velocity.y) 
                : Vector2.MoveTowards(velocity, target, Time.fixedDeltaTime * windRoc); 
            return velocity;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.TryGetComponent(out PlayerController player)) return;
            _playerInside = true;
            audioSource.Play();
            _player = player; 
            _player.AddPlayerVelocityEffector(this);
            _player.CanDash = true;
            _player.ForceCancelEarlyRelease();
            if (_player.PlayerState == PlayerController.PlayerStateEnum.Dash)
            {
                _player.ForceCancelDash();
            }
        }
        
        private void OnTriggerExit2D(Collider2D other)
        {
            if (!other.TryGetComponent(out PlayerController _player)) return;
            _player.RemovePlayerVelocityEffector(this);
            _playerInside = false;
            _shouldFadeOut = true;
        }


        // Adjust the particle system based on wind direction and speed
        private void UpdateParticles()
        {
            if (windParticles == null || boxCollider == null ) return;

            // Ensure box collider and particle systems shape match up
            Vector2 newScale = new Vector2(windZoneSize.x, windZoneSize.y); 
            var parentShape = windParticles.shape;
            parentShape.scale = newScale;

            var childShape = childParticleSystem.shape;
            childShape.scale = newScale;

            boxCollider.size =  newScale;
        }
    }
}
