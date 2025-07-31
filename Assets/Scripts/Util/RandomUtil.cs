using System.Collections;
using UnityEngine;

namespace Util
{
    /// <summary>
    /// Utility class to select random objects.
    /// </summary>
    public static class RandomUtil
    {
        /// <summary>
        /// Gets the random object from an array.
        /// </summary>
        /// <param name="array">Array to get objects from</param>
        /// <typeparam name="T">Type of objects in array</typeparam>
        /// <returns>Random object</returns>
        public static T SelectRandom<T>(T[] array)
        {
            if (array == null || array.Length == 0)
            {
                Debug.LogWarning("Attempted to randomly select from an empty array.");
                return default;
            }
            int index = Random.Range(0, array.Length);
            return array[index];
        }

        /// <summary>
        /// Coroutine to randomly jitter a transform object over time.
        ///
        /// Applies a random movement on the unit circle.
        /// </summary>
        /// <param name="transform">Transform object to jitter over time</param>
        /// <param name="duration">Duration of jitter</param>
        /// <param name="jitterIterationTime">Delay between jitter iterations</param>
        /// <param name="jitterMagnitude">Jitter magnitude</param>
        /// <param name="initialPosition">Initial position if provided (defaults to transform's position)</param>
        /// <returns>Coroutine</returns>
        public static IEnumerator RandomJitterRoutine(Transform transform, float duration, float jitterIterationTime,
            float jitterMagnitude, Vector3? initialPosition = null)
        {
            Vector3 startPos = initialPosition ?? transform.position;
            WaitForSeconds wait = jitterIterationTime > 0 ? new WaitForSeconds(jitterIterationTime) : null;
            for (float timer = 0; timer < duration; timer += Mathf.Max(jitterIterationTime, Time.deltaTime))
            {
                transform.position = startPos + (Vector3)Random.insideUnitCircle * jitterMagnitude;
                yield return wait;
            }

            transform.position = startPos;
        }
    }
}
