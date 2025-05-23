using System.Collections;
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

        private Rigidbody2D _rigidbody2D;
        private float _startingY;
        private State _state;

        private void Awake()
        {
            _rigidbody2D = GetComponent<Rigidbody2D>();
            _startingY = _rigidbody2D.position.y;
            _state = State.WaitTop;
            StartCoroutine(SlamRoutine());
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            if (_state != State.MoveDown || Vector2.Dot(other.GetContact(0).normal, Vector2.up) < 0) return;
            if (other.collider.TryGetComponent(out PlayerController playerController))
            {
                playerController.ForceKill();
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
        /// Coroutine to wait at the top and then start moving down.
        /// </summary>
        /// <returns>Coroutine</returns>
        private IEnumerator SlamRoutine()
        {
            _state = State.WaitTop;
            yield return new WaitForSeconds(timeStayUp);
            _rigidbody2D.velocity = new Vector2(0, -velocityDown);
            _state = State.MoveDown;
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
            _rigidbody2D.velocity = new Vector2(0, velocityUp);
            while (_rigidbody2D.position.y < _startingY)
            {
                yield return new WaitForFixedUpdate();
            }

            _rigidbody2D.velocity = Vector2.zero;
            StartCoroutine(SlamRoutine());
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
