using UnityEngine;

namespace Util
{
    /// <summary>
    /// Utility class for various vector operations.
    /// </summary>
    public static class VectorUtil
    {
        /// <summary>
        /// Returns the distance along a line from <see cref="from"/> to <see cref="to"/> that <see cref="position"/> is on.
        /// </summary>
        /// <param name="from">Initial position of the line</param>
        /// <param name="to">Final position of the line or ray</param>
        /// <param name="position">Position (not necessarily on the line)</param>
        /// <returns>Distance along the line (can be negative)</returns>
        public static float DistanceAlongPath(Vector2 from, Vector2 to, Vector2 position)
        {
            Vector2 direction = to - from;
            Vector2 travelled = position - from;
            return Vector2.Dot(direction, travelled) / direction.magnitude;
        }
    }
}
