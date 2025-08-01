using Player;
using UnityEngine;
using Util;

namespace Interactables
{
    /// <summary>
    /// Class represents a bouncy platform that is a 2D collider
    /// </summary>
    [DisallowMultipleComponent, RequireComponent(typeof(Collider2D), typeof(Animator), typeof(AudioSource))]
    public class BouncyPlatform : AbstractPlayerInteractable
    {
        private static readonly int BOUNCE_HASH = Animator.StringToHash("Bounce");
        private static readonly int STEP_HASH = Animator.StringToHash("Step");
        private static readonly int STEP_OFF_HASH = Animator.StringToHash("Step Off");

        #region Serialized Private Fields

        [Header("Bouncing")]
        [SerializeField] private float yBounceForce;
        [SerializeField] private float xBounceForce;
        [Header("Sounds")] [SerializeField] private AudioClip[] bounceSounds;

        #endregion
        
        private Animator _animator;
        private AudioSource _audioSource;
        private bool _launched;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _audioSource = GetComponent<AudioSource>();
        }
        
        /// <inheritdoc />
        public override void OnPlayerEnter(PlayerController player)
        {
            _animator.SetTrigger(STEP_HASH);
            _launched = false;
        }

        /// <inheritdoc />
        public override void OnPlayerExit(PlayerController player)
        {
            if (!_launched)
            {
                _animator.SetTrigger(STEP_OFF_HASH);
            }
            
            if (player.CurrentInteractableArea == this)
            {
                player.StopInteraction(this);
            }
            player.CurrentInteractableArea = null;
        }

        public override Vector2 ApplyVelocity(Vector2 velocity)
        {
            return new Vector2(xBounceForce, yBounceForce);
        }

        public override void StartInteract(PlayerController player)
        {
            player.CanDash = true;
            player.ForceCancelEarlyRelease();
            player.AddPlayerVelocityEffector(this, true);
            player.StopInteraction(this);
            player.CurrentInteractableArea = null;
            
            _animator.SetTrigger(BOUNCE_HASH);
            _launched = true;
            _audioSource.clip = RandomUtil.SelectRandom(bounceSounds);
            _audioSource.Play();
        }

        public override void EndInteract(PlayerController player)
        {
        }
    }
}
