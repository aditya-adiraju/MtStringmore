using Player;
using UnityEngine;
using Util;

namespace Interactables
{
    /// <summary>
    /// Class represents a bouncy platform that is a 2D collider
    /// </summary>
    [DisallowMultipleComponent, RequireComponent(typeof(Collider2D), typeof(Animator), typeof(AudioSource))]
    public class BouncyPlatform : MonoBehaviour, IPlayerVelocityEffector
    {
        private static readonly int BounceHash = Animator.StringToHash("Bounce");

        #region Serialized Private Fields

        [Header("Bouncing")]
        [SerializeField] private float yBounceForce;
        [SerializeField] private float xBounceForce;
        [Header("Sounds")] [SerializeField] private AudioClip[] bounceSounds;

        #endregion

        private PlayerController _player;
        private Collision2D _bounceArea;
        private Animator _animator;
        private AudioSource _audioSource;

        /// <inheritdoc />
        public Vector2 ApplyVelocity(Vector2 velocity)
        {
            return new Vector2(xBounceForce, yBounceForce);
        }

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _audioSource = GetComponent<AudioSource>();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.gameObject.TryGetComponent(out _player)) return;
            _player.AddPlayerVelocityEffector(this, true);
            _player.CanDash = true;
            _player.ForceCancelEarlyRelease();
            if (_player.PlayerState == PlayerController.PlayerStateEnum.Dash)
            {
                _player.ForceCancelDash();
            }
            _animator.SetTrigger(BounceHash);
            _audioSource.clip = RandomUtil.SelectRandom(bounceSounds);
            _audioSource.Play();
        }
    
        private void OnTriggerExit2D(Collider2D other)
        {
            if (!other.TryGetComponent(out PlayerController _)) return;
            _animator.ResetTrigger(BounceHash);
        }
    }
}
