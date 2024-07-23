using UnityEngine;
using static PlayerController;

// [ExecuteInEditMode]
public class FollowCamera : MonoBehaviour
{
    [SerializeField] private float lookaheadDistance;
    [SerializeField] private float xSmoothTime;
    [SerializeField] private float ySmoothTime;
    [SerializeField] private float yOffset;
    [SerializeField] private float upYThreshold;
    
    private PlayerController _playerController;
    private Transform _playerTransform;
    private Vector2 _target;
    private float _xVelocity;
    private float _yVelocity;
    private float _cameraZ;
    private float _lastGroundedY;
    
    private void Awake()
    {
        GameObject player = GameObject.FindWithTag("Player");
        _playerTransform = player.transform;
        _playerController = player.GetComponent<PlayerController>();
        _cameraZ = transform.position.z;
    }

    private void LateUpdate()
    {
        if (_playerController.PlayerState == PlayerStateEnum.Run) _lastGroundedY = _playerTransform.position.y;
        float playerY = _playerTransform.position.y;
        _target.y = playerY > _lastGroundedY + upYThreshold || playerY < _lastGroundedY
            ? playerY + yOffset
            : _lastGroundedY + yOffset;
        _target.x = _playerTransform.position.x + lookaheadDistance * _playerController.Direction;
        
        var position = transform.position;
        float smoothedX = Mathf.SmoothDamp(position.x, _target.x, ref _xVelocity, xSmoothTime);
        float smoothedY = Mathf.SmoothDamp(position.y, _target.y, ref _yVelocity, ySmoothTime);
        position = new Vector3(smoothedX, smoothedY, _cameraZ);
        transform.position = position;
    }
}
