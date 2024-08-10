using UnityEngine;
using static PlayerController;

/// <summary>
/// Causes the camera to dynamically follow the player (GameObject with the Player tag).
/// </summary>
public class FollowCamera : MonoBehaviour
{
    #region Serialized Private Fields

    [SerializeField] private float lookaheadDistance;
    [SerializeField] private float xSmoothTime;
    [SerializeField] private float ySmoothTime;
    [SerializeField] private float yOffset;
    [SerializeField] private float upYThreshold;

    #endregion

    #region Private Properties

    private PlayerController _playerController;
    private Transform _playerTransform;
    private Vector2 _target;
    private float _xVelocity;
    private float _yVelocity;
    private float _cameraZ;
    private float _lastGroundedY;

    #endregion

    #region Unity Event Functions

    private void Awake()
    {
        GameObject player = GameObject.FindWithTag("Player");
        _playerTransform = player.transform;
        _playerController = player.GetComponent<PlayerController>();
        // always keep the camera Z 1 less than the player
        _cameraZ = transform.position.z;
    }

    private void LateUpdate()
    {
        if (_playerController.PlayerState == PlayerStateEnum.Run) _lastGroundedY = _playerTransform.position.y;
        float playerY = _playerTransform.position.y;
        Vector3 camPosition = transform.position;
        
        // when player is above their last grounded position + a threshold or below their last grounded position,
        // set the camera to the player's y level + offset
        // otherwise, set the camera to their last grounded position + offset
        _target.y = playerY > _lastGroundedY + upYThreshold || playerY < _lastGroundedY
            ? playerY + yOffset
            : _lastGroundedY + yOffset;
        // add a constant lookahead to the camera, based on the player's facing direction
        _target.x = _playerTransform.position.x + lookaheadDistance * _playerController.Direction;
        
        // apply smoothing to the camera
        float smoothedX = Mathf.SmoothDamp(camPosition.x, _target.x, ref _xVelocity, xSmoothTime);
        float smoothedY = Mathf.SmoothDamp(camPosition.y, _target.y, ref _yVelocity, ySmoothTime);
        transform.position = new Vector3(smoothedX, smoothedY, _cameraZ);
    }

    #endregion

    /// <summary>
    /// Instantly sets camera position to the given position
    /// </summary>
    /// <param name="position">x,y,z of the position the camera should be</param>
    public void SetCameraPosition(Vector3 position)
    {
        transform.position = position;
    }
}
