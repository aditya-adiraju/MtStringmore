using UnityEngine;

// [ExecuteInEditMode]
public class FollowCamera : MonoBehaviour
{
    [SerializeField] private float lookaheadDistance;
    [SerializeField] private float xSmoothTime;
    [SerializeField] private float ySmoothTime;
    [SerializeField] private float yOffset;
    [SerializeField] private float upYThreshold;
    [SerializeField] private float downYThreshold;
    
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
        if (_playerController.Grounded) _lastGroundedY = _playerTransform.position.y;
        float playerY = _playerTransform.position.y;
        // if (playerY > _lastGroundedY + upYThreshold)
        
        _target.y = playerY > _lastGroundedY + upYThreshold || playerY < _lastGroundedY
            ? playerY + yOffset
            : _lastGroundedY + yOffset;
        // _target.y = _playerTransform.position.y + yOffset;
        _target.x = _playerTransform.position.x + lookaheadDistance * _playerController.Direction;
        float smoothedX = Mathf.SmoothDamp(transform.position.x, _target.x, ref _xVelocity, xSmoothTime);
        float smoothedY = Mathf.SmoothDamp(transform.position.y, _target.y, ref _yVelocity, ySmoothTime);
        transform.position = new Vector3(smoothedX, smoothedY, _cameraZ);
    }
}
