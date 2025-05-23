using System.Collections;
using JetBrains.Annotations;
using Managers;
using UnityEngine;

namespace Interactables
{
    /// <summary>
    /// Class to move an object from A -> B over time, obscuring obstacles.
    /// </summary>
    /// <remarks>
    /// The actual obstacle obscuring is done by the sprite lmao.
    /// </remarks>
    [DisallowMultipleComponent]
    public class ClothObstacleHider : MonoBehaviour
    {
        [SerializeField, Tooltip("Position curve over time, in [0, 1]")]
        private AnimationCurve motionCurve;

        [SerializeField, Min(0), Tooltip("Motion time (seconds)")]
        private float motionTime = 1;

        /// <remarks>
        /// Has to be public to allow the editor to modify this without reflection.
        /// </remarks>
        [Tooltip("First position, local coordinates")]
        public Vector2 firstPosition;

        /// <remarks>
        /// Has to be public to allow the editor to modify this without reflection.
        /// </remarks>
        [Tooltip("Second position, local coordinates")]
        public Vector2 secondPosition;

        /// <remarks>
        /// I know the new reset logic hasn't been merged in yet,
        /// but we need to save a copy of the enumerator to reset the object later.
        /// </remarks>
        private IEnumerator _activeMotion;

        /// <summary>
        /// Returns the time of the last keyframe.
        /// </summary>
        /// <remarks>
        /// Should be 1, but just in case there's user error, we scale anyways.
        /// </remarks>
        private float LastKeyframeTime
        {
            get
            {
                Keyframe[] keys = motionCurve.keys;
                if (keys == null || keys.Length == 0) return 1;
                return keys[^1].time;
            }
        }

        /// <summary>
        /// Evaluates the position at a specific time since motion start.
        /// </summary>
        /// <param name="time">Time since motion start</param>
        /// <returns>Position at time</returns>
        private Vector2 EvaluateAt(float time)
        {
            return Vector2.Lerp(firstPosition, secondPosition,
                motionCurve.Evaluate(LastKeyframeTime * time / motionTime));
        }

        /// <summary>
        /// Coroutine to move the object according to the motion curve.
        /// </summary>
        /// <returns></returns>
        private IEnumerator MotionCoroutine()
        {
            float time = 0;
            while (time < motionTime)
            {
                time += Time.deltaTime;
                transform.localPosition = EvaluateAt(time);
                yield return null;
            }

            transform.localPosition = secondPosition;
        }

        /// <summary>
        /// Called on reset: stops motion and resets to the start.
        /// </summary>
        private void OnReset()
        {
            transform.localPosition = firstPosition;
            if (_activeMotion != null) StopCoroutine(_activeMotion);
            _activeMotion = null;
        }

        private void Awake()
        {
            GameManager.Instance.Reset += OnReset;
        }

        private void OnDisable()
        {
            GameManager.Instance.Reset -= OnReset;
        }

        /// <summary>
        /// Starts moving from the first position.
        /// </summary>
        [UsedImplicitly]
        public void StartMotion()
        {
            if (_activeMotion == null)
            {
                StartCoroutine(_activeMotion = MotionCoroutine());
            }
        }

        private void OnValidate()
        {
            foreach (Keyframe key in motionCurve.keys)
            {
                if (key.value is < 0 or > 1)
                {
                    Debug.LogWarning($"Motion curve keyframe is out of range: {key.time}, {key.value}");
                }
            }

            if (!Mathf.Approximately(LastKeyframeTime, 1))
            {
                Debug.LogWarning($"Motion curve last keyframe time is not 1; time will be scaled: {LastKeyframeTime}");
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.TransformPoint(firstPosition), 1);
            Gizmos.color = Color.white;
            // no specific reason for choosing this
            const int evalPoints = 5;
            for (int i = 1; i < evalPoints; i++)
            {
                Vector2 localPoint = EvaluateAt(i * motionTime / evalPoints);
                Gizmos.DrawWireSphere(transform.TransformPoint(localPoint), 1);
            }

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.TransformPoint(secondPosition), 1);
        }
    }
}
