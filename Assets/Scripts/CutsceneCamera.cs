using System.Collections;
using JetBrains.Annotations;
using UnityEngine;
using Yarn.Unity;

[RequireComponent(typeof(Camera))]
public class CutsceneCamera : MonoBehaviour
{
    [CanBeNull] private static GameObject _targetObject;
    private static Vector2 _target;
    private static Vector2 _lead;
    private static Camera _camera;
    private static float _cameraResizeSpeed;
    private static float _cameraSize;

    [SerializeField] private float smoothTime = 0.5f;
    private Vector2 _velocity;

    private void Awake()
    {
        _camera = GetComponent<Camera>();
    }

    private void LateUpdate()
    {
        if (_targetObject)
            _target = new Vector2(_targetObject.transform.position.x, _targetObject.transform.position.y) + _lead;

        // apply smoothing to the camera
        Vector3 camPosition = transform.position;
        Vector2 smoothed = Vector2.SmoothDamp(camPosition, _target, ref _velocity, smoothTime);
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
    public static void FixCoords(float x, float y)
    {
        _targetObject = null;
        _lead = Vector2.zero;
        _target = new Vector2(x, y);
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
