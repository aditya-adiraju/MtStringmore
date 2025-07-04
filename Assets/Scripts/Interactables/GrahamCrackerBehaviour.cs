using System.Collections;
using Managers;
using Player;
using UnityEngine;

namespace Interactables
{
    /// <summary>
    /// Behaviour of an object that moves down, collides with a bottom object (or player), then moves up, repeatedly.
    ///
    /// Kills the player if it gets crushed.
    /// </summary>
    /// <remarks>
    /// There's probably better ways to implement this than constantly checking for collision with the bottom collider
    /// since I use a kinematic rigidbody, but I cba.
    /// </remarks>
    [RequireComponent(typeof(Rigidbody2D))]
    public class GrahamCrackerBehaviour : MonoBehaviour
    {
        [SerializeField, Min(0), Tooltip("Time in up state, seconds")]
        private float timeStayUp = 4;

        [SerializeField, Min(0), Tooltip("Velocity down, m/s")]
        private float velocityDown = 10;

        [SerializeField, Min(0), Tooltip("Velocity up, m/s")]
        private float velocityUp = 5;

        [SerializeField, Min(0), Tooltip("Time staying down, seconds")]
        private float timeStayDown;

        [SerializeField, Tooltip("Bottom collider")]
        private Collider2D bottomCollider;

        [SerializeField]
        private AudioSource warningSound;

        [SerializeField, Min(0), Tooltip("Time (sec) prior to closing to play warning")]
        private float warningSoundTime = 1;
        [SerializeField]
        private AudioSource closingSound;
        [SerializeField, Min(0), Tooltip("Time(sec) after closing starts to play closing sound")]
        private float closingSoundDelay = 0.2f;
        [SerializeField] private bool alwaysActive;
        private Rigidbody2D _rigidbody2D;
        private float _startingDistance;
        private State _state;
        private bool _active;

        /// <summary>
        /// Whether the graham cracker can close.
        /// </summary>
        public bool Active
        {
            private get => _active;
            set
            {
                _active = value;
                if (_active) StartCoroutine(SlamRoutine());
            }
        }

        private void Awake()
        {
            _rigidbody2D = GetComponent<Rigidbody2D>();
            _startingDistance = Vector2.Distance(transform.position, bottomCollider.transform.position);
            _state = State.WaitTop;
            Active = alwaysActive;
            if (Active) StartCoroutine(SlamRoutine());
            GameManager.Instance.Reset += OnReset;
        }

        private void OnDestroy()
        {
            GameManager.Instance.Reset -= OnReset;
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            if (_state != State.MoveDown || Vector2.Dot(other.GetContact(0).normal, _rigidbody2D.velocity) > 0) return;
            if (other.collider.TryGetComponent(out PlayerController playerController))
            {
                playerController.TryKill();
                _rigidbody2D.bodyType = RigidbodyType2D.Static;
            } else StartCoroutine(RecoverCoroutine());
        }

        private void FixedUpdate()
        {
            if (_state == State.MoveDown && _rigidbody2D.IsTouching(bottomCollider))
            {
                StartCoroutine(RecoverCoroutine());
            }
        }

        /// <summary>
        /// Called on reset: stops coroutines and resets to be at the top.
        /// </summary>
        private void OnReset()
        {
            StopAllCoroutines();
            _rigidbody2D.bodyType = RigidbodyType2D.Kinematic;
            Vector2 dir = transform.position - bottomCollider.transform.position;
            _rigidbody2D.position = (Vector2)bottomCollider.transform.position + dir.normalized * _startingDistance;
            _rigidbody2D.velocity = Vector2.zero;
            Active = alwaysActive;
            if (Active) StartCoroutine(SlamRoutine());
        }

        /// <summary>
        /// Coroutine to wait at the top and then start moving down.
        /// </summary>
        /// <returns>Coroutine</returns>
        private IEnumerator SlamRoutine()
        {
            _state = State.WaitTop;
            yield return new WaitForSeconds(timeStayUp - warningSoundTime);
            warningSound.Play();
            yield return new WaitForSeconds(warningSoundTime);
            _rigidbody2D.velocity = velocityDown * -(Vector2)transform.up;
            _state = State.MoveDown;
            closingSound.PlayDelayed(closingSoundDelay);
        }

        /// <summary>
        /// Coroutine to wait at the bottom and then start moving up.
        /// </summary>
        /// <returns>Coroutine</returns>
        private IEnumerator RecoverCoroutine()
        {
            _state = State.WaitBottom;
            _rigidbody2D.velocity = Vector2.zero;
            yield return new WaitForSeconds(timeStayDown);
            _state = State.MoveUp;
            _rigidbody2D.velocity = velocityUp * (Vector2) transform.up;
            while (Vector2.Distance(transform.position, bottomCollider.transform.position) < _startingDistance)
            {
                yield return new WaitForFixedUpdate();
            }

            _rigidbody2D.velocity = Vector2.zero;
            if (Active) StartCoroutine(SlamRoutine());
        }

        /// <summary>
        /// Internal states.
        /// </summary>
        private enum State
        {
            /// <summary>
            /// Currently waiting at the top.
            /// </summary>
            WaitTop,

            /// <summary>
            /// Currently waiting at the bottom.
            /// </summary>
            WaitBottom,

            /// <summary>
            /// Currently moving down.
            /// </summary>
            MoveDown,

            /// <summary>
            /// Currently moving up.
            /// </summary>
            MoveUp
        }
    }
}
