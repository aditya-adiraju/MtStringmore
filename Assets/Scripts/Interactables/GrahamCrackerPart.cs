using System.Collections;
using Player;
using UnityEngine;
using Util;

namespace Interactables
{
    /// <summary>
    /// Represents a single moving part of the graham cracker.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(SpriteRenderer))]
    public class GrahamCrackerPart : MonoBehaviour
    {
        private SpriteRenderer _spriteRenderer;
        private Rigidbody2D _rigidbody2D;
        private bool _isBottom;
        private Vector2 _dir;
        private Vector2 _initialPosition;
        private GrahamCrackerBehaviour _crackerBehaviour;
        private bool _isMovingToKill;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _rigidbody2D = GetComponent<Rigidbody2D>();
            _crackerBehaviour = GetComponentInParent<GrahamCrackerBehaviour>();
            _isBottom = _spriteRenderer.flipY; // yes, this may cause problems
            _initialPosition = _rigidbody2D.position;
            _dir = (_isBottom ? 1 : -1) * transform.up;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (!_isMovingToKill || Vector2.Dot(collision.GetContact(0).normal, _rigidbody2D.velocity) > 0) return;
            Collider2D other = collision.collider;
            _crackerBehaviour.RegisterCollision(_isBottom, other.GetComponent<PlayerController>(),
                other.GetComponent<GrahamCrackerPart>());
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            if (!_isMovingToKill) return;
            Collider2D other = collision.collider;
            _crackerBehaviour.DeregisterCollision(_isBottom, other.GetComponent<PlayerController>(),
                other.GetComponent<GrahamCrackerPart>());
        }

        /// <summary>
        /// Stops moving in place.
        /// </summary>
        public void StopMotion()
        {
            _rigidbody2D.velocity = Vector2.zero;
            _rigidbody2D.bodyType = RigidbodyType2D.Static;
            _isMovingToKill = false;
        }

        /// <summary>
        /// Starts moving towards the other graham cracker part.
        /// </summary>
        /// <param name="speed">Speed (m/s)</param>
        public void StartMotion(float speed)
        {
            _rigidbody2D.bodyType = RigidbodyType2D.Kinematic;
            _rigidbody2D.velocity = _dir * speed;
            _isMovingToKill = true;
        }

        /// <summary>
        /// Starts moving away from the other graham cracker part.
        /// </summary>
        /// <param name="speed">Speed (m/s)</param>
        public void MoveAway(float speed)
        {
            _rigidbody2D.bodyType = RigidbodyType2D.Kinematic;
            _rigidbody2D.velocity = -_dir * speed;
            _isMovingToKill = false;
            StartCoroutine(QueryForInitialPosition(_rigidbody2D.position));
        }

        /// <summary>
        /// Shakes in place to warn that cracker is about to shut.
        /// </summary>
        /// <param name="duration">Duration of the shake.</param>
        /// <param name="shakeDelay">Delay between shakes.</param>
        /// <param name="distance">Furthest distance from original position when shaking.</param>
        public void HandleShake(float duration, float shakeDelay, float distance) =>
            StartCoroutine(RandomUtil.RandomJitterRoutine(transform, duration, shakeDelay, distance, _initialPosition));

        /// <summary>
        /// Coroutine to check whether we've reached the initial position after we've started moving away,
        /// and become static if so.
        /// </summary>
        /// <param name="pointAtMotion">Initial point along the path of motion</param>
        /// <returns>Coroutine</returns>
        private IEnumerator QueryForInitialPosition(Vector2 pointAtMotion)
        {
            WaitForFixedUpdate waitForFixedUpdate = new();
            float distToTravel = Vector2.Distance(pointAtMotion, _initialPosition);
            while (VectorUtil.DistanceAlongPath(pointAtMotion, _initialPosition, _rigidbody2D.position) < distToTravel)
            {
                yield return waitForFixedUpdate;
            }

            _rigidbody2D.position = _initialPosition;
            _rigidbody2D.bodyType = RigidbodyType2D.Static;
            _crackerBehaviour.RegisterReset(_isBottom);
        }

        /// <summary>
        /// Called on game reset - resets to initial position.
        /// </summary>
        public void OnGameReset()
        {
            StopAllCoroutines();
            _rigidbody2D.position = _initialPosition;
            _isMovingToKill = false;
            _rigidbody2D.bodyType = RigidbodyType2D.Static;
        }
    }
}
