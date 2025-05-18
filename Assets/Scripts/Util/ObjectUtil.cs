using UnityEngine;

namespace Util
{
    /// <summary>
    /// Utility class for Unity objects.
    /// </summary>
    public static class ObjectUtil
    {
        /// <summary>
        /// Ensures a transform has exactly the specified number of children, instantiating prefabs if necessary.
        /// </summary>
        /// <param name="transform">Transform to check</param>
        /// <param name="len">Exact number of objects</param>
        /// <param name="prefab">Prefab to instantiate</param>
        public static void EnsureLength(Transform transform, int len, Object prefab)
        {
            if (transform.childCount == len) return;
            for (int i = transform.childCount; i < len; i++)
            {
                Object.Instantiate(prefab, transform);
            }

            for (int i = transform.childCount-1; i >= len; i--)
            {
                Object.Destroy(transform.GetChild(i).gameObject);
            }
        }
    }
}
