using JetBrains.Annotations;
using UnityEngine;
using Yarn.Unity;

public class CutsceneCamera : MonoBehaviour
{
    [CanBeNull] private static GameObject _targetObject;
    private static Vector2 _target;

    [SerializeField] private float xSmoothTime;
    [SerializeField] private float ySmoothTime;
    private float _xVelocity;
    private float _yVelocity;

    private void LateUpdate()
    {
        if (_targetObject) _target = _targetObject.transform.position;

        // apply smoothing to the camera
        var camPosition = transform.position;
        var smoothedX = Mathf.SmoothDamp(camPosition.x, _target.x, ref _xVelocity, xSmoothTime);
        var smoothedY = Mathf.SmoothDamp(camPosition.y, _target.y, ref _yVelocity, ySmoothTime);
        transform.position = new Vector3(smoothedX, smoothedY, camPosition.z);
    }


    [YarnCommand("follow_object")]
    public static void FollowObject(GameObject target)
    {
        _targetObject = target;
        _target = target.transform.position;
    }

    [YarnCommand("fix_coords")]
    public static void FixCoords(float x, float y)
    {
        _targetObject = null;
        _target = new Vector2(x, y);
    }

    [YarnCommand("fix_object")]
    public static void FixObject(GameObject obj)
    {
        FixCoords(obj.transform.position.x, obj.transform.position.y);
    }
}