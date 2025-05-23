using UnityEngine;

namespace StringmoreCamera
{
    /// <summary>
    /// Fixes the camera at a set position when the player enters the bounding box.
    /// Camera position is the centre of the box collider, plus an offset.
    /// Uses "MainCamera" and "Player" tags.
    /// </summary>
    [RequireComponent(typeof(BoxCollider2D))]
    public class FixCameraTrigger : MonoBehaviour
    {
        [SerializeField] private Vector2 targetOffset;
        public FixCameraType fixTypeX = FixCameraType.RequireEqual;
        public FixCameraType fixTypeY = FixCameraType.RequireEqual;

        private Vector2 _bound;
        private FollowCamera _cam;

        private void Awake()
        {
            _cam = GameObject.FindWithTag("MainCamera").GetComponent<FollowCamera>();
            Vector2 boxOffset = GetComponent<BoxCollider2D>().offset;
            _bound = (Vector2)transform.position + boxOffset + targetOffset;
        }

        /// <summary>
        /// Locks a specific field/value within the provided FixCameraType settings.
        /// </summary>
        /// <param name="original">Original value</param>
        /// <param name="bound">Bound for the fix type</param>
        /// <param name="fixType">FixCameraType lock type</param>
        /// <returns>Original field adjusted</returns>
        private static float AffectField(float original, float bound, FixCameraType fixType)
        {
            return fixType switch
            {
                FixCameraType.RequireEqual => bound,
                FixCameraType.None => original,
                FixCameraType.AllowGreater => Mathf.Max(original, bound),
                FixCameraType.AllowLess => Mathf.Min(original, bound),
                _ => original
            };
        }

        /// <summary>
        /// Locks the target camera viewpoint with this trigger's settings.
        /// </summary>
        /// <param name="target">Target camera viewpoint</param>
        /// <returns>Target adjusted for lock settings</returns>
        public Vector2 AffectTarget(Vector2 target)
        {
            target.x = AffectField(target.x, _bound.x, fixTypeX);
            target.y = AffectField(target.y, _bound.y, fixTypeY);
            return target;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                _cam.EnterFixCameraTrigger(this);
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                _cam.ExitFixCameraTrigger(this);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Awake();

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(_bound, Vector3.one);
            Gizmos.color = Color.white;
            UnityEngine.Camera cam = _cam.GetComponent<UnityEngine.Camera>();
            Vector3 cameraBounds = new(cam.orthographicSize * cam.aspect * 2, cam.orthographicSize * 2, 1);
            Gizmos.DrawWireCube(_bound, cameraBounds);
            Gizmos.color = Color.green;
            if (fixTypeX is FixCameraType.AllowGreater or FixCameraType.None)
            {
                Gizmos.DrawRay(_bound, Vector3.right);
            }

            if (fixTypeX is FixCameraType.AllowLess or FixCameraType.None)
            {
                Gizmos.DrawRay(_bound, Vector3.left);
            }

            Gizmos.color = Color.red;
            if (fixTypeY is FixCameraType.AllowGreater or FixCameraType.None)
            {
                Gizmos.DrawRay(_bound, Vector3.up);
            }

            if (fixTypeY is FixCameraType.AllowLess or FixCameraType.None)
            {
                Gizmos.DrawRay(_bound, Vector3.down);
            }
        }

        /// <summary>
        /// Enumeration of the types of camera locks.
        /// </summary>
        public enum FixCameraType
        {
            /// <summary>
            /// Fixes the target to a specific coordinate.
            /// </summary>
            RequireEqual,

            /// <summary>
            /// Allows the target value to be less than or equal to a specific coordinate.
            /// </summary>
            AllowLess,

            /// <summary>
            /// Allows the target value to be greater than or equal to a specific coordinate.
            /// </summary>
            AllowGreater,

            /// <summary>
            /// Does not perform locking at all.
            /// </summary>
            None
        }
    }
}
