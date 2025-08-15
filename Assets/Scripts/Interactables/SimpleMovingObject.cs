using Managers;
using UnityEngine;
using Util;

namespace Interactables
{
    /// <summary>
    /// A simple moving object utility that moves to a specific point from the start (or unbounded in a ray).
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class SimpleMovingObject : MonoBehaviour
    {
        [SerializeField, Tooltip("Whether the motion is unbounded")]
        private bool motionUnbounded;
        [SerializeField, Tooltip("End position or direction (if motion is unbounded)")]
        private Vector2 endPosition;

        [SerializeField, Min(0), Tooltip("Speed (m/s)")]
        private float velocity = 1;
        [SerializeField, Tooltip("Whether motion is present on start")]
        private bool activateOnStart;

        private Vector2 _startPosition;
        private Rigidbody2D _rigidbody2D;
        private bool _reachedEnd;

        /// <summary>
        /// Returns the distance along the path.
        /// </summary>
        private float DistanceAlongPath =>
            VectorUtil.DistanceAlongPath(_startPosition, endPosition, _rigidbody2D.position);

        private void Awake()
        {
            _rigidbody2D = GetComponent<Rigidbody2D>();
            // if someone changes marshmellow to kinematic there will be ~~blood~~ bugs
            _rigidbody2D.bodyType = RigidbodyType2D.Kinematic;
            _startPosition = transform.position;
            _reachedEnd = false;
            OnReset();

            GameManager.Instance.Reset += OnReset;
        }

        private void OnDestroy()
        {
            GameManager.Instance.Reset -= OnReset;
        }

        private void OnEnable()
        {
            if (activateOnStart) StartMotion();
        }

        private void OnDisable()
        {
            _rigidbody2D.velocity = Vector2.zero;
        }

        /// <summary>
        /// Starts motion.
        /// </summary>
        public void StartMotion()
        {
            if (_rigidbody2D.velocity != Vector2.zero) return;
            if (_reachedEnd) return;
            _startPosition = transform.position;
            Vector2 direction = endPosition - _startPosition;
            _rigidbody2D.velocity = direction.normalized * velocity;
        }

        private void FixedUpdate()
        {
            if (_reachedEnd) return;
            if (!motionUnbounded && DistanceAlongPath >= Vector2.Distance(_startPosition, endPosition))
            {
                _reachedEnd = true;
                _rigidbody2D.velocity = Vector2.zero;
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, 1);
            if (motionUnbounded)
            {
                Gizmos.color = Color.yellow;
                Vector2 direction =  endPosition - (Vector2) transform.position;
                Gizmos.DrawRay(transform.position, direction);
            }
            else
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(transform.position, endPosition);
                Gizmos.DrawWireSphere(endPosition, 1);
            }
        }

        /// <summary>
        /// Called on reset: resets position.
        /// </summary>
        private void OnReset()
        {
            _rigidbody2D.position = _startPosition;
            _reachedEnd = false;
            if (activateOnStart && enabled)
            {
                Vector2 direction = endPosition - _startPosition;
                _rigidbody2D.velocity = direction.normalized * velocity;
            }
            else
            {
                _rigidbody2D.velocity = Vector2.zero;
            }
        }
    }
}
