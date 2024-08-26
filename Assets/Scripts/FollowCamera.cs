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

    #region Private Fields

    private PlayerController _playerController;
    private Transform _playerTransform;
    private Vector2 _target;
    private float _xVelocity;
    private float _yVelocity;
    private float _cameraZ;
    private float _lastGroundedY;
    private bool _fixed;

    #endregion

    #region Unity Event Functions

    private void Awake()
    {
        GameObject player = GameObject.FindWithTag("Player");
        _playerTransform = player.transform;
        _playerController = player.GetComponent<PlayerController>();
        _cameraZ = transform.position.z;
        _fixed = false;
    }

    private void LateUpdate()
    {
        if (!_fixed) _target = GetPlayerTarget();
        
        // apply smoothing to the camera
        Vector3 camPosition = transform.position;
        float smoothedX = Mathf.SmoothDamp(camPosition.x, _target.x, ref _xVelocity, xSmoothTime);
        float smoothedY = Mathf.SmoothDamp(camPosition.y, _target.y, ref _yVelocity, ySmoothTime);
        transform.position = new Vector3(smoothedX, smoothedY, _cameraZ);
    }

    #endregion
    
    #region Public Methods

    /// <summary>
    /// Fixes the target position of the camera.
    /// </summary>
    public void FixTarget(Vector2 fixedTarget)
    {
        _fixed = true;
        _target = fixedTarget;
    }

    /// <summary>
    /// Sets the camera to follow the player.
    /// </summary>
    public void FollowPlayer()
    {
        _fixed = false;
    }

    /// <summary>
    /// Returns the intended target of the camera based on the player's position.
    /// </summary>
    public Vector2 GetPlayerTarget()
    {
        if (_playerController.PlayerState == PlayerStateEnum.Run) _lastGroundedY = _playerTransform.position.y;
        float playerY = _playerTransform.position.y;
        
        // when player is above their last grounded position + a threshold or below their last grounded position,
        // set the camera to the player's y level + offset
        // otherwise, set the camera to their last grounded position + offset
        float targetY = playerY > _lastGroundedY + upYThreshold || playerY < _lastGroundedY
            ? playerY + yOffset
            : _lastGroundedY + yOffset;
        // add a constant lookahead to the camera, based on the player's facing direction
        float targetX = _playerTransform.position.x + lookaheadDistance * _playerController.Direction;

        return new Vector2(targetX, targetY);
    } 
    
    #endregion
}
