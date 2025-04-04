using System.Collections.Generic;
using System.Linq;
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
    private List<FixCameraTrigger> _fixCameraTriggers;
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
        _cameraZ = transform.position.z;
        _fixCameraTriggers = new List<FixCameraTrigger>();
    }

    private void Start()
    {
        GameManager.Instance.Reset += OnReset;
    }

    private void OnDisable()
    {
        GameManager.Instance.Reset -= OnReset;
    }

    private void LateUpdate()
    {
        Vector2 target = GetPlayerTarget();
        _target = _fixCameraTriggers.LastOrDefault()?.AffectTarget(target) ?? target;

        // apply smoothing to the camera
        Vector3 camPosition = transform.position;
        float smoothedX = Mathf.SmoothDamp(camPosition.x, _target.x, ref _xVelocity, xSmoothTime);
        float smoothedY = Mathf.SmoothDamp(camPosition.y, _target.y, ref _yVelocity, ySmoothTime);
        transform.position = new Vector3(smoothedX, smoothedY, _cameraZ);
    }

    #endregion

    #region Public Methods

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

    /// <summary>
    /// Called when entering a new FixCameraTrigger.
    /// Camera will fix to the new area.
    /// </summary>
    public void EnterFixCameraTrigger(FixCameraTrigger trigger)
    {
        _fixCameraTriggers.Add(trigger);
    }

    /// <summary>
    /// Called when exiting a FixCameraTrigger.
    /// Camera will follow the previous area, or start following the player.
    /// </summary>
    public void ExitFixCameraTrigger(FixCameraTrigger trigger)
    {
        _fixCameraTriggers.Remove(trigger);
    }

    #endregion

    /// <summary>
    /// On level reset, place camera at player's position
    /// </summary>
    private void OnReset()
    {
        var checkpointPos = GameManager.Instance.CheckPointPos;
        transform.position = new Vector3(checkpointPos.x, checkpointPos.y, transform.position.z);
    }
}
