using System;
using UnityEngine;

namespace Parallax
{
    /// <summary>
    /// Creates tiling background and moves them according to their parallax factors as the camera moves
    /// </summary>
    [DisallowMultipleComponent, RequireComponent(typeof(Camera))]
    public class ParallaxCamera : MonoBehaviour
    {
        private Vector2 _prev;
        private float _screenWidth;

        /// <summary>
        /// Parameter for camera motion event.
        /// </summary>
        /// <param name="delta">Change in position</param>
        /// <param name="position">Actual position</param>
        /// <param name="screenWidth">Orthographic camera view width</param>
        public record CameraMotionData(Vector2 delta, Vector2 position, float screenWidth);

        /// <summary>
        /// Fired when camera moves.
        /// </summary>
        public event Action<CameraMotionData> Moved;

        private void Awake()
        {
            Camera cam = GetComponent<Camera>();
            float height = 2f * cam.orthographicSize;
            _screenWidth = height * cam.aspect;
        }

        private void LateUpdate()
        {
            Vector2 pos = transform.position;
            // hey so turns out Vector2 operator== tests for approximate equality:
            // see https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Vector2-operator_eq.html
            // for absolute equality you'd do Vector2.Equals()
            if (_prev == pos) return;
            Moved?.Invoke(new CameraMotionData(_prev - pos, pos, _screenWidth));
            _prev = pos;
        }
    }
}
