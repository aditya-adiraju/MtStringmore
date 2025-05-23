using System.Collections;
using JetBrains.Annotations;
using UnityEngine;
using Yarn.Unity;

namespace StringmoreCamera
{
    [RequireComponent(typeof(UnityEngine.Camera))]
    public class CutsceneCamera : MonoBehaviour
    {
        [CanBeNull] private static GameObject _targetObject;
        private static Vector2 _target;
        private static Vector2 _lead;
        private static UnityEngine.Camera _camera;
        private static float _cameraResizeSpeed;
        private static float _cameraSize;
        private static float _smoothTime;

        [SerializeField] private float defaultSmoothTime = 0.5f;
        private Vector2 _velocity;

        private void Awake()
        {
            _camera = GetComponent<UnityEngine.Camera>();
        }

        private void LateUpdate()
        {
            if (_targetObject)
                _target = new Vector2(_targetObject.transform.position.x, _targetObject.transform.position.y) + _lead;

            // apply smoothing to the camera
            Vector3 camPosition = transform.position;
            if (_smoothTime < 0)
                _smoothTime = defaultSmoothTime;
            Vector2 smoothed = Vector2.SmoothDamp(camPosition, _target, ref _velocity, _smoothTime);
            transform.position = new Vector3(smoothed.x, smoothed.y, camPosition.z);

            // resize camera
            _camera.orthographicSize =
                Mathf.MoveTowards(_camera.orthographicSize, _cameraSize, Time.deltaTime * _cameraResizeSpeed);
        }


        [YarnCommand("follow_object")]
        public static void FollowObject(GameObject target, float x = 0, float y = 0)
        {
            _targetObject = target;
            _target = target.transform.position;
            _lead = new Vector2(x, y);
        }

        [YarnCommand("fix_coords")]
        public static void FixCoords(float x, float y, float panTime = -1)
        {
            _targetObject = null;
            _lead = Vector2.zero;
            _target = new Vector2(x, y);
            _smoothTime = panTime;
        }

        [YarnCommand("fix_object")]
        public static void FixObject(GameObject obj, float x = 0, float y = 0)
        {
            FixCoords(obj.transform.position.x, obj.transform.position.y);
            _lead = new Vector2(x, y);
        }

        [YarnCommand("resize_camera")]
        public static void ResizeCamera(float size, float speed)
        {
            _cameraSize = size;
            _cameraResizeSpeed = speed;
        }

        [YarnCommand("resize_camera_block")]
        public static IEnumerator ResizeCameraBlock(float size, float speed)
        {
            ResizeCamera(size, speed);
            while (!Mathf.Approximately(_camera.orthographicSize, size)) yield return null;
        }
    }
}
