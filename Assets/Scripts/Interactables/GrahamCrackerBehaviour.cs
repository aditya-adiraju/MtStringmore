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

        [SerializeField]
        private AudioSource warningSound;

        [SerializeField, Min(0), Tooltip("Time (sec) prior to closing to play warning")]
        private float warningSoundTime = 1;
        [SerializeField]
        private AudioSource closingSound;
        [SerializeField, Min(0), Tooltip("Time(sec) after closing starts to play closing sound")]
        private float closingSoundDelay = 0.2f;
        [SerializeField] private bool alwaysActive;
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
        public void RegisterCollision(bool isBottom, PlayerController player)
        {
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
            bottomPart.OnGameReset();
            topPart.OnGameReset();
        }

        /// <summary>
        /// Coroutine to wait at the top and then start moving down.
        /// </summary>
        /// <returns>Coroutine</returns>
        private IEnumerator SlamRoutine()
        {
            yield return new WaitForSeconds(timeStayUp - warningSoundTime);
            warningSound.Play();
            yield return new WaitForSeconds(warningSoundTime);
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
