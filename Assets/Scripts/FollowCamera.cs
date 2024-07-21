using UnityEngine;

// [ExecuteInEditMode]
public class FollowCamera : MonoBehaviour
{
    [SerializeField] private float lookaheadDistance;
    [SerializeField] private float yOffset;
    [SerializeField] private float smoothTime;
    
    private PlayerController _playerController;
    private Transform _playerTransform;
    private Vector2 _target;
    private Vector2 _velocity = Vector2.zero;
    private float _cameraZ;
    
    private void Awake()
    {
        GameObject player = GameObject.FindWithTag("Player");
        _playerTransform = player.transform;
        _playerController = player.GetComponent<PlayerController>();
        _cameraZ = transform.position.z;
    }

    private void LateUpdate()
    {
        _target.x = _playerTransform.position.x + lookaheadDistance * _playerController.Direction;
        _target.y = _playerController.Grounded ? _playerTransform.position.y + yOffset : _target.y;
        Vector3 newPos = Vector2.SmoothDamp(transform.position, _target, ref _velocity, smoothTime);
        newPos.z = _cameraZ;
        transform.position = newPos;
    }
}
