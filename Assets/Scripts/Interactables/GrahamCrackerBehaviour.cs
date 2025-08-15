using System.Collections;
using Managers;
using Player;
using UnityEngine;

namespace Interactables
{
    /// <summary>
    /// Behaviour of an object that controls two parts that move towards with each other,
    /// collides with the player or the bottom object, then moves back, repeatedly.
    ///
    /// Kills the player if it gets crushed.
    /// </summary>
    public class GrahamCrackerBehaviour : MonoBehaviour
    {
        [Header("General settings")]
        
        [SerializeField] private GrahamCrackerPart bottomPart;
        [SerializeField] private GrahamCrackerPart topPart;
        [SerializeField, Min(0), Tooltip("Time in up state, seconds")]
        private float timeStayUp = 4;

        [SerializeField, Min(0), Tooltip("Velocity down, m/s")]
        private float velocityDown = 10;

        [SerializeField, Min(0), Tooltip("Velocity up, m/s")]
        private float velocityUp = 5;

        [SerializeField, Min(0), Tooltip("Time staying down, seconds")]
        private float timeStayDown;
        
        [SerializeField] private bool alwaysActive;
        
        [Header("Shake behaviour")]
        
        [SerializeField, Min(0), Tooltip("Time (sec) prior to closing when cracker should start shaking")]
        private float shakeTime = 1;
        
        [SerializeField, Tooltip("Delay between shakes"), Range(0f, 0.1f)] 
        private float shakeDelay = 0.01f;
        
        [SerializeField, Tooltip("Furthest distance from original position when shaking"), Range(0f, 2f)]
        private float shakeDistance = 0.2f;
        
        [SerializeField]
        private AudioSource shakeSound;
        
        
        [Header("Closing behaviour")]
        [SerializeField]
        private AudioSource closingSound;
        [SerializeField, Min(0), Tooltip("Time(sec) after closing starts to play closing sound")]
        private float closingSoundDelay = 0.2f;
        
        private bool _active;
        private bool _bottomCollide;
        private bool _topCollide;
        private bool _bottomHasReset;
        private bool _topHasReset;

        /// <summary>
        /// Whether the graham cracker can close.
        /// </summary>
        public bool Active
        {
            private get => _active;
            set
            {
                bool newValue = value || alwaysActive;
                if (_active == newValue) return;
                _active = newValue;
                if (_active) StartCoroutine(SlamRoutine());
            }
        }

        /// <summary>
        /// Called when a part has registered a collision with an object.
        /// </summary>
        /// <param name="isBottom">Whether this is the bottom part</param>
        /// <param name="player">PlayerController attached to the object if present</param>
        /// <param name="part">Part of the Graham Cracker we're colliding with if present</param>
        public void RegisterCollision(bool isBottom, PlayerController player, GrahamCrackerPart part)
        {
            // sometimes it collides with terrain, ignore that
            if (!player && !part) return;
            if (isBottom) _bottomCollide = true;
            else _topCollide = true;
            if (_bottomCollide && _topCollide)
            {
                if (player)
                {
                    player.TryKill();
                    bottomPart.StopMotion();
                    topPart.StopMotion();
                }
                else
                {
                    StartCoroutine(RecoverCoroutine());
                }
            }
        }

        /// <summary>
        /// Called when a part has deregistered a collision with an object.
        /// </summary>
        /// <param name="isBottom">Whether this is the bottom part</param>
        /// <param name="player">PlayerController attached to the object if present</param>
        /// <param name="part">Part of the Graham Cracker we're NOT colliding with if present</param>
        public void DeregisterCollision(bool isBottom, PlayerController player, GrahamCrackerPart part)
        {
            // sometimes it collides with terrain, ignore that
            if (!player && !part) return;
            if (isBottom) _bottomCollide = false;
            else _topCollide = false;
        }

        /// <summary>
        /// Called to signal that the corresponding graham cracker part has reset.
        /// </summary>
        /// <param name="isBottom">Whether it's the bottom that's reset or the top</param>
        public void RegisterReset(bool isBottom)
        {
            if (isBottom) _bottomHasReset = true;
            else _topHasReset = true;

            if (_bottomHasReset && _topHasReset)
            {
                StopAllCoroutines();
                if (Active) StartCoroutine(SlamRoutine());
                _bottomHasReset = false;
                _topHasReset = false;
            }
        }

        private void Awake()
        {
            Active = alwaysActive;
            if (Active) StartCoroutine(SlamRoutine());
            GameManager.Instance.Reset += OnReset;
        }

        private void OnDestroy()
        {
            GameManager.Instance.Reset -= OnReset;
        }

        /// <summary>
        /// Called on reset: stops coroutines and resets to be at the top.
        /// </summary>
        private void OnReset()
        {
            StopAllCoroutines();
            Active = alwaysActive;
            if (Active) StartCoroutine(SlamRoutine());
            _bottomCollide = false;
            _topCollide = false;
            _bottomHasReset = false;
            _topHasReset = false;
            bottomPart.OnGameReset();
            topPart.OnGameReset();
        }

        /// <summary>
        /// Coroutine to wait at the top and then start moving down.
        /// </summary>
        /// <returns>Coroutine</returns>
        private IEnumerator SlamRoutine()
        {
            _bottomCollide = false;
            _topCollide = false;
            if (timeStayUp - shakeTime > 0)
                yield return new WaitForSeconds(timeStayUp - shakeTime);
            shakeSound.Play();
            bottomPart.HandleShake(shakeTime, shakeDelay, shakeDistance);
            topPart.HandleShake(shakeTime, shakeDelay, shakeDistance);
            if (shakeTime > 0)
                yield return new WaitForSeconds(shakeTime);
            bottomPart.StartMotion(velocityDown);
            topPart.StartMotion(velocityDown);
            closingSound.PlayDelayed(closingSoundDelay);
        }

        /// <summary>
        /// Coroutine to wait at the bottom and then start moving up.
        /// </summary>
        /// <returns>Coroutine</returns>
        private IEnumerator RecoverCoroutine()
        {
            bottomPart.StopMotion();
            topPart.StopMotion();
            yield return new WaitForSeconds(timeStayDown);
            bottomPart.MoveAway(velocityUp);
            topPart.MoveAway(velocityUp);
        }
    }
}
