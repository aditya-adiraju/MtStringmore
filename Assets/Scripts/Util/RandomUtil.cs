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
    }
}
