using System.Collections;
using UnityEngine;

namespace Interactables.Balloon
{
    /// <summary>
    /// Main balloon visual that handles warnings (increase and decrease the size of the visual when enabled)
    /// and also animation.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer), typeof(Animator))]
    public class BalloonVisual : MonoBehaviour
    {
        private static readonly int AnimatorResetHash = Animator.StringToHash("Reset");
        private static readonly int AnimatorSpeedHash = Animator.StringToHash("Speed");

        [SerializeField, Tooltip("New size of the object when warning is playing")]
        private float warningSize = 1.2f;

        [SerializeField, Tooltip("Time with new size (sec)"), Min(0)]
        private float timeInWarning = 0.3f;

        /// <summary>
        /// Whether the sprite renderer is active.
        /// </summary>
        public bool IsDisplaying => _spriteRenderer.enabled;

        /// <summary>
        /// Whether the warning is playing.
        /// </summary>
        public bool WarningPlaying { get; private set; }

        /// <summary>
        /// Whether the balloon can inflate.
        /// </summary>
        public bool CanInflate
        {
            get => _animator.GetFloat(AnimatorSpeedHash) > 0;
            set => _animator.SetFloat(AnimatorSpeedHash, value ? 1f : 0f);
        }

        private SpriteRenderer _spriteRenderer;
        private Animator _animator;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _animator = GetComponent<Animator>();
            CanInflate = false;
        }

        /// <summary>
        /// Called when balloon resets to initial position.
        /// </summary>
        public void ResetToStart()
        {
            WarningPlaying = false;
            _spriteRenderer.enabled = true;
            _animator.SetTrigger(AnimatorResetHash);
        }

        /// <summary>
        /// Starts the coroutine to change the size repeatedly before popping.
        /// </summary>
        public void StartWarning()
        {
            StartCoroutine(WarningCoroutine());
        }

        /// <summary>
        /// Called on balloon pop — hide the sprite renderer.
        /// </summary>
        public void Pop()
        {
            StopAllCoroutines();
            transform.localScale = Vector3.one;
            WarningPlaying = false;
            _spriteRenderer.enabled = false;
        }

        /// <summary>
        /// Coroutine to adjust the size of this object every <see cref="timeInWarning"/> seconds.
        /// </summary>
        /// <returns>Coroutine</returns>
        private IEnumerator WarningCoroutine()
        {
            WarningPlaying = true;
            WaitForSeconds wait = new(timeInWarning);
            while (WarningPlaying)
            {
                transform.localScale = Vector3.one * warningSize;
                yield return wait;
                transform.localScale = Vector3.one;
                yield return wait;
            }
        }
    }
}
