using System;
using UnityEngine;

/// <summary>
/// Controls player movement and invokes events for different player states
/// </summary>
public class PlayerController : MonoBehaviour
{
    #region Serialized Private Fields
    
    // @formatter:off
    [Header("Input")]
    [SerializeField] private bool doubleJumpEnabled;
    [SerializeField] private bool dashEnabled;
    [SerializeField] private float buttonBufferTime;
    [Header("Collisions")] 
    [SerializeField] private LayerMask collisionLayer;
    [SerializeField] private float collisionDistance;
    [SerializeField] private float wallCloseDistance;
    [Header("Ground")]
    [SerializeField] private float maxGroundSpeed;
    [SerializeField] private float groundAcceleration;
    [SerializeField] private float startDirection;
    [SerializeField] private float groundingForce;
    [Header("Jump")]
    [SerializeField] private float jumpPower;
    [SerializeField] private float doubleJumpPower;
    [SerializeField] private float coyoteTime;
    [Header("Air")]
    [SerializeField] private float maxFallSpeed;
    [SerializeField] private float fallAccelerationUp;
    [SerializeField] private float fallAccelerationDown;
    [SerializeField] private float earlyReleaseFallAcceleration;
    [SerializeField] private float maxAirSpeed;
    [SerializeField] private float airAcceleration;
    [Header("Dash")] 
    [SerializeField] private float dashTime;
    [SerializeField] private float dashSpeed;
    [SerializeField] private float endDashSpeed;
    [Header("Wall")] 
    [SerializeField] private float wallJumpAngle;
    [SerializeField] private float wallJumpPower;
    [SerializeField] private float wallSlideSpeed;
    [SerializeField] private float wallSlideAcceleration;
    [Header("Swinging")] 
    [SerializeField] private float swingBoostMultiplier;
    [SerializeField] private float maxSwingSpeed;
    [SerializeField] private float swingAcceleration;
    [SerializeField] private float minSwingReleaseX;
    [Header("Visual")]
    [SerializeField] private LineRenderer ropeRenderer;
    [SerializeField] private float deathTime;
    // this is just here for battle of the concepts
    [Header("Temporary")]
    [SerializeField] private GameObject poofSmoke;
    // @formatter:on

    #endregion

    #region Public Properties and Actions

    public enum PlayerStateEnum
    {
        Run,
        Air,
        LeftWallSlide,
        RightWallSlide,
        Dead,
        Swing,
        Dash,
        OnObject
    }

    // TODO: PlayerState set should be a function that fires an action
    public PlayerStateEnum PlayerState { get; private set; }

    /// <summary>
    /// Current velocity of the player.
    /// </summary>
    public Vector2 Velocity => _velocity;

    /// <summary>
    /// Facing direction of the player. -1.0 for left, 1.0 for right.
    /// </summary>
    public float Direction { get; private set; }

    /// <summary>
    /// Time when the most recent dash would have ended (may be a time in the future).
    /// </summary>
    public float TimeDashEnded { get; private set; }

    /// <summary>
    /// Active velocity effector.
    /// </summary>
    public IPlayerVelocityEffector ActiveVelocityEffector
    {
        get => _activeEffector;
        set
        {
            if (_activeEffector != null && value != null && _activeEffector != value
                && _activeEffector.IgnoreOtherEffectors)
            {
                Debug.LogWarning("Ignoring other effector as current active effector ignores other effectors.");
            }
            else
            {
                _activeEffector = value;
            }
        }
    }

    /// <summary>
    /// Fires when the player becomes grounded or leaves the ground.
    /// Parameters:
    ///     bool: false if leaving the ground, true if becoming grounded
    ///     float: player's Y velocity
    /// </summary>
    public event Action<bool, float> GroundedChanged;

    /// <summary>
    /// Fires when the player hits the wall or leaves the wall.
    /// Parameters:
    ///     bool: True if hitting the wall, false if leaving the wall
    /// </summary>
    public event Action<bool> WallChanged;

    public event Action Jumped;
    public event Action Dashed;
    /// <summary>
    /// Fires when the player hangs onto an interactable object.
    /// Parameters:
    ///     bool (hanging): True when player attaches, false when player lets go
    ///     bool (facingLeft): True when facing left, false otherwise
    /// </summary>
    public event Action<bool, bool> HangChanged;
    public event Action DoubleJumped;
    public event Action Death;
    public event Action<bool> SwingDifferentDirection;

    /// <summary>
    /// If true, skips death logic.
    /// </summary>
    public bool DebugIgnoreDeath { get; set; }

    /// <summary>
    /// Current interactable in range.
    /// </summary>
    public AbstractPlayerInteractable CurrentInteractableArea { get; set; }
    
    /// <summary>
    /// Whether the player can use their dash in the air.
    /// </summary>
    public bool CanDash
    {
        get => _canDash;
        set => _canDash = value;
    }

    #endregion

    #region Private Properties

    private Rigidbody2D _rb;
    private CapsuleCollider2D _col;
    private IPlayerVelocityEffector _activeEffector;
    private ParticleSystem _runningDust;
    private ParticleSystem _landingDust;
    private ParticleSystem _leftWallSlideDust;
    private ParticleSystem _rightWallSlideDust;

    private float _time;
    private float _timeButtonPressed;
    private float _timeLeftGround;
    private float _timeDashed;

    private bool _buttonUsed;
    private bool _isButtonHeld;
    private bool _canReleaseEarly;
    private bool _releasedEarly;
    private bool _canDoubleJump;
    private bool _canDash;

    private Vector2 _velocity;
    private bool _closeToWall;
    private Vector2 _groundNormal;

    private Collider2D _swingArea;
    private float _swingRadius;
    private bool _canSwing;
    private bool _swingStarted;
    private bool _wasSwingClockwise;

    #endregion

    #region Unity Event Handlers

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _col = GetComponent<CapsuleCollider2D>();
        ParticleSystem[] particleSystems = GetComponentsInChildren<ParticleSystem>();
        
        foreach (ParticleSystem ps in particleSystems)
        {
            switch (ps.gameObject.name)
            {
                case "RunningDust":
                    _runningDust = ps;
                    break;
                case "LandingDust":
                    _landingDust = ps;
                    break;
                case "LeftWallSlidingDust":
                    _leftWallSlideDust = ps;
                    break;
                case "RightWallSlidingDust":
                    _rightWallSlideDust = ps;
                    break;
            }
        }
        _buttonUsed = true;
        Direction = startDirection;
    }

    private void Start()
    {
        GameManager.Instance.Reset += OnReset;
    }

    private void Update()
    {
        _time += Time.deltaTime;

        GetInput();
        RedrawRope(); // TODO this should be moved outside player controller when knitby is real
    }

    private void OnDisable()
    {
        GameManager.Instance.Reset -= OnReset;
    }

    private void GetInput()
    {
        if (Input.GetButtonDown("Jump"))
        {
            _timeButtonPressed = _time;
            _buttonUsed = false;
        }

        _isButtonHeld = Input.GetButton("Jump");

        if (Input.GetButtonDown("Debug Reset"))
        {
            GameManager.Instance.Respawn();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("SwingArea"))
        {
            _swingArea = other;
            _swingRadius = _swingArea.GetComponent<SwingArea>().swingRadius;
            _swingRadius *= _swingArea.transform.lossyScale.x; // assume global scale is same for every dimension
            _canSwing = true;
        }
        else if (other.gameObject.CompareTag("Death"))
        {
            if (PlayerState != PlayerStateEnum.Dead)
            {
                HandleDeath();
            }
        }
    }
    
    private void OnParticleCollision(GameObject other)
    {
        if (other.CompareTag("Death"))
        {
            if (PlayerState != PlayerStateEnum.Dead)
            {
                HandleDeath();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("SwingArea"))
        {
            // can't swing if outside swing area
            // assumes swing areas are not overlapping
            _canSwing = false;
        }
    }

    private void FixedUpdate()
    {
        if (PlayerState == PlayerStateEnum.Dead)
            return;

        CheckCollisions();
        HandleWallJump();
        HandleSwing();
        HandleInteractables();
        HandleJump();
        if (doubleJumpEnabled) HandleDoubleJump();
        HandleEarlyRelease();
        HandleWalk();
        HandleGravity();
        if (ActiveVelocityEffector != null)
            _velocity = ActiveVelocityEffector.ApplyVelocity(_velocity);
        else if (dashEnabled) HandleDash();
        ApplyMovement();
    }

    #endregion

    /// <summary>
    /// Stops interacting with the interactable.
    /// </summary>
    /// <param name="interactable">Requesting interactable, only used to check activity.</param>
    /// <remarks>
    /// We could possibly remove the parameter since it's only used for a precondition check,
    /// but I've left it here in case I screwed up somewhere.
    /// </remarks>
    public void StopInteraction(AbstractPlayerInteractable interactable)
    {
        Debug.Assert(CurrentInteractableArea == interactable, $"Requested to stop interaction on inactive interactable: {interactable} vs Current {CurrentInteractableArea}");
        _buttonUsed = true;
        CurrentInteractableArea.EndInteract(this);
        PlayerState = PlayerStateEnum.Air;
        HangChanged?.Invoke(false, _velocity.x < 0);
    }

    /// <summary>
    /// Stops dashing if the player is currently dashing.
    /// </summary>
    public void ForceCancelDash()
    {
        if (PlayerState != PlayerStateEnum.Dash)
        {
            Debug.LogWarning("ForceCancelDash called but PlayerState is not Dash");
            return;
        }

        PlayerState = PlayerStateEnum.Air;
        TimeDashEnded = Time.time;
    }

    /// <summary>
    /// Force kills the player.
    /// </summary>
    public void ForceKill()
    {
        HandleDeath();
    }

    #region Private Methods

    private RaycastHit2D CapsuleCastCollision(Vector2 direction, float distance)
    {
        return Physics2D.CapsuleCast(_col.bounds.center, _col.size, _col.direction, 0,
            direction, distance, collisionLayer);
    }

    private void CheckCollisions()
    {
        RaycastHit2D groundCast = CapsuleCastCollision(Vector2.down, collisionDistance);
        bool groundHit = groundCast;
        bool ceilingHit = CapsuleCastCollision(Vector2.up, collisionDistance);
        bool leftWallHit = CapsuleCastCollision(Vector2.left, collisionDistance);
        bool rightWallHit = CapsuleCastCollision(Vector2.right, collisionDistance);
        _closeToWall = CapsuleCastCollision(_velocity, wallCloseDistance);

        if (groundCast) _groundNormal = groundCast.normal;

        if (ceilingHit) _velocity.y = Mathf.Min(0, _velocity.y);

        // TODO is this being fired constantly while on a wall?
        if (leftWallHit || rightWallHit)
            WallChanged?.Invoke(true);
        else
            WallChanged?.Invoke(false);

        if (PlayerState == PlayerStateEnum.Run && !groundHit)
        {
            PlayerState = PlayerStateEnum.Air;
            _timeLeftGround = _time;
            GroundedChanged?.Invoke(false, 0);
        }

        if (PlayerState == PlayerStateEnum.Air)
        {
            if (leftWallHit) PlayerState = PlayerStateEnum.LeftWallSlide;
            if (rightWallHit) PlayerState = PlayerStateEnum.RightWallSlide;
        }

        if (PlayerState != PlayerStateEnum.Run && groundHit)
        {
            PlayerState = PlayerStateEnum.Run;
            _canDoubleJump = true;
            GroundedChanged?.Invoke(true, Mathf.Abs(_velocity.y));
            _landingDust.Play();
        }

        if (PlayerState == PlayerStateEnum.RightWallSlide && !rightWallHit ||
            PlayerState == PlayerStateEnum.LeftWallSlide && !leftWallHit)
            PlayerState = PlayerStateEnum.Air;
        
        UpdateParticleSystemState(_runningDust, PlayerStateEnum.Run);
        UpdateParticleSystemState(_leftWallSlideDust, PlayerStateEnum.LeftWallSlide);
        UpdateParticleSystemState(_rightWallSlideDust, PlayerStateEnum.RightWallSlide);
    }

    private void UpdateParticleSystemState(ParticleSystem system, PlayerStateEnum targetState) {
      if (PlayerState == targetState) {
        if (!system.isPlaying) system.Play();
      } else if (system.isPlaying) {
        system.Stop();
      }
    }

    private void HandleDeath()
    {
        if (DebugIgnoreDeath) return;
        _velocity = Vector2.zero;
        _rb.velocity = _velocity;
        PlayerState = PlayerStateEnum.Dead;
        Death?.Invoke();
        Invoke(nameof(Respawn), deathTime);
    }

    private void Respawn()
    {
        GameManager.Instance.Respawn();
    }

    private void HandleWallJump()
    {
        if (CanUseButton() && PlayerState is PlayerStateEnum.LeftWallSlide or PlayerStateEnum.RightWallSlide)
        {
            _velocity = new Vector2(Mathf.Cos(wallJumpAngle), Mathf.Sin(wallJumpAngle)) * wallJumpPower;
            if (PlayerState == PlayerStateEnum.RightWallSlide) _velocity.x = -_velocity.x;

            _buttonUsed = true;
            PlayerState = PlayerStateEnum.Air;
            _canReleaseEarly = false;
            _releasedEarly = false;
            _canDoubleJump = true;
            Jumped?.Invoke();
            GroundedChanged?.Invoke(false, Mathf.Abs(_velocity.y)); // TODO need a new action for wj
        }
    }

    private void HandleJump()
    {
        bool canUseCoyote = PlayerState == PlayerStateEnum.Air && _time - _timeLeftGround <= coyoteTime;
        if ((PlayerState == PlayerStateEnum.Run || canUseCoyote) && CanUseButton())
        {
            _velocity.y = jumpPower;
            _buttonUsed = true;
            PlayerState = PlayerStateEnum.Air;
            _canReleaseEarly = true;
            Jumped?.Invoke();
            GroundedChanged?.Invoke(false, Mathf.Abs(_velocity.y));
        }
    }

    private void HandleDoubleJump()
    {
        if (CanUseButton() && _canDoubleJump && !_closeToWall)
        {
            _velocity.y = doubleJumpPower;
            _buttonUsed = true;
            _canDoubleJump = false;
            _canReleaseEarly = true;
            DoubleJumped?.Invoke();
        }
    }

    private void HandleDash()
    {
        if (PlayerState is PlayerStateEnum.Air && CanUseButton() && _canDash && !_closeToWall)
        {
            // start a dash
            _canDash = false;
            _buttonUsed = true;
            PlayerState = PlayerStateEnum.Dash;
            Dashed?.Invoke();
            _timeDashed = _time;
            TimeDashEnded = Time.time + dashTime;
            // for battle of the concepts: add temp dash anim
            Instantiate(poofSmoke, transform.position, new Quaternion());
        }
        else if (PlayerState is PlayerStateEnum.Dash)
        {
            // move player forward at dash speed
            _velocity.y = 0;
            _velocity.x = dashSpeed * Direction;
            // check if dash is over
            if (_time - _timeDashed >= dashTime)
            {
                PlayerState = PlayerStateEnum.Air;
                _velocity.x = endDashSpeed * Direction;
                TimeDashEnded = Time.time;
            }
        }
        else if (PlayerState is PlayerStateEnum.Run or PlayerStateEnum.Swing)
        {
            // can dash after landing on the ground or swinging
            _canDash = true;
        }
    }

    private void HandleEarlyRelease()
    {
        if (!_canReleaseEarly) return;

        if (!_isButtonHeld) _releasedEarly = true;
        if (_velocity.y < 0f)
        {
            _canReleaseEarly = false;
            _releasedEarly = false;
        }
    }

    private void HandleWalk()
    {
        switch (PlayerState)
        {
            case PlayerStateEnum.Run:
                // rotate ground normal vector 90 degrees towards facing direction
                Vector2 walkTarget = new Vector2(_groundNormal.y * Direction, _groundNormal.x * -Direction) *
                                     maxGroundSpeed;
                float newX = Mathf.MoveTowards(_velocity.x, walkTarget.x, groundAcceleration * Time.fixedDeltaTime);
                float newY = walkTarget.y;
                _velocity = new Vector2(newX, newY);
                break;
            case PlayerStateEnum.Air:
                if (Mathf.Abs(_velocity.x) < maxAirSpeed)
                    _velocity.x = Mathf.MoveTowards(_velocity.x, maxAirSpeed * Direction,
                        airAcceleration * Time.fixedDeltaTime);
                break;
        }
    }

    private void HandleGravity()
    {
        if (ActiveVelocityEffector?.IgnoreGravity ?? false) return;
        switch (PlayerState)
        {
            case PlayerStateEnum.Run:
                if (_velocity.y <= 0f)
                    _velocity.y = -groundingForce;
                break;
            case PlayerStateEnum.LeftWallSlide:
            case PlayerStateEnum.RightWallSlide:
                _velocity.x = 0f;
                if (_velocity.y <= 0f)
                    _velocity.y = Mathf.MoveTowards(_velocity.y, -wallSlideSpeed,
                        wallSlideAcceleration * Time.fixedDeltaTime);
                else goto case PlayerStateEnum.Air;
                break;
            case PlayerStateEnum.Air:
            case PlayerStateEnum.Swing:
                float accel;
                if (_releasedEarly) accel = earlyReleaseFallAcceleration;
                else if (_velocity.y >= 0f) accel = fallAccelerationUp;
                else accel = fallAccelerationDown;

                _velocity.y = Mathf.MoveTowards(_velocity.y, -maxFallSpeed, accel * Time.fixedDeltaTime);
                break;
        }
    }

    /// <summary>
    /// Handle logic of interactable objects.
    /// </summary>
    private void HandleInteractables()
    {
        if (!CurrentInteractableArea) return;
        bool previouslyGrounded = PlayerState == PlayerStateEnum.Run;
        if (CanUseButton())
        {
            // CanUseButton can be true multiple frames so don't re-call StartInteract jic
            if (PlayerState == PlayerStateEnum.OnObject) return;
            PlayerState = PlayerStateEnum.OnObject;
            CurrentInteractableArea.StartInteract(this);
            HangChanged?.Invoke(true, _velocity.x < 0);
            if (previouslyGrounded)
            {
                _timeLeftGround = _time;
                GroundedChanged?.Invoke(false, 0);
            }
        }

        // fix for occasional race condition where state changes after moving sometimes
        // (such as due to weird quirks in how unity persists collision data for 1 additional tick)
        // thus setting the state to Run/Air without re-setting the player object state
        // I suspect the swing may have this problem but who puts the swing in contact with the ground?
        // should we just have the grounded check first check if you're on an object?
        if (ReferenceEquals(ActiveVelocityEffector, CurrentInteractableArea) && PlayerState != PlayerStateEnum.OnObject)
        {
            Debug.LogWarning($"ActiveVelocityEffector == CurrentInteractableArea while State != PlayerStateEnum.OnObject, actual state: {PlayerState}");
            PlayerState = PlayerStateEnum.OnObject;
            
            // not sure if it's guaranteed for the player to not be on the ground when on the object
            // (e.g. if we build objects that push the player along the ground or something like that)
            // but this is required to stop the footsteps from playing.
            if (previouslyGrounded)
            {
                _timeLeftGround = _time;
                GroundedChanged?.Invoke(false, 0);
            }
        }
        
        if (!CanUseButton() && PlayerState == PlayerStateEnum.OnObject && !_isButtonHeld)
        {
            StopInteraction(CurrentInteractableArea);
        }
    }

    private void HandleSwing()
    {
        if (_canSwing && CanUseButton())
        {
            // in swing area, button pressed
            PlayerState = PlayerStateEnum.Swing;
            ropeRenderer.enabled = true;
            HangChanged?.Invoke(true, _velocity.x < 0);
        }
        else if (PlayerState is PlayerStateEnum.Swing && _isButtonHeld)
        {
            // in swing and holding down button
            // if not at max radius yet, fall normally
            if (!_swingStarted && Vector2.Distance(_swingArea.transform.position, transform.position) >= _swingRadius)
            {
                // reached max radius, start swing
                _swingStarted = true;
                Vector2 relPos = transform.position - _swingArea.transform.position;
                _wasSwingClockwise = Vector3.Cross(relPos, _velocity).z > 0f;
            }

            if (_swingStarted)
            {
                // swinging at max radius
                Vector2 relPos = transform.position - _swingArea.transform.position;
                // if going down, accelerate to target swing speed
                if (_velocity.y <= 0f && _velocity.magnitude <= maxSwingSpeed)
                {
                    _velocity = _velocity.normalized * Mathf.MoveTowards(_velocity.magnitude, maxSwingSpeed,
                        swingAcceleration * Time.fixedDeltaTime);
                }

                Vector2 testPos = relPos + _velocity * Time.fixedDeltaTime;
                Vector2 newPos = testPos.normalized * _swingRadius;
                _velocity = Vector2.ClampMagnitude((newPos - relPos) / Time.fixedDeltaTime, maxSwingSpeed);
                if (_wasSwingClockwise ^ Vector3.Cross(relPos, _velocity).z > 0f)
                {
                    SwingDifferentDirection?.Invoke(_wasSwingClockwise);
                    _wasSwingClockwise = !_wasSwingClockwise;
                }
            }
        }
        else if (PlayerState is PlayerStateEnum.Swing && !_isButtonHeld)
        {
            // swinging but button is released
            // stop swinging, disallow swing until reentering area
            PlayerState = PlayerStateEnum.Air;
            ropeRenderer.enabled = false;
            _canSwing = false;
            HangChanged?.Invoke(false, _velocity.x < 0);

            if (_swingStarted)
            {
                // give x velocity boost on release
                float boostDirection = transform.position.x >= _swingArea.transform.position.x ? 1f : -1f;
                if (Mathf.Abs(_velocity.x) <= minSwingReleaseX)
                    _velocity.x = minSwingReleaseX * boostDirection;
                _buttonUsed = true;
                _swingStarted = false;
            }
        }
    }

    private void RedrawRope()
    {
        if (PlayerState == PlayerStateEnum.Swing)
        {
            ropeRenderer.positionCount = 2;
            ropeRenderer.SetPosition(0, transform.position);
            ropeRenderer.SetPosition(1, _swingArea.transform.position);
        }
    }

    private void ApplyMovement()
    {
        _rb.velocity = _velocity;
        if (_velocity.x != 0f) Direction = Mathf.Sign(_velocity.x);
        Debug.DrawRay(transform.position, _velocity, Color.magenta);
    }

    private bool CanUseButton()
    {
        return !_buttonUsed && _time <= _timeButtonPressed + buttonBufferTime;
    }

    #endregion

    /// <summary>
    /// On reset, respawn at checkpoint with the direction we faced at that checkpoint
    /// </summary>
    private void OnReset()
    {
        var checkpointPos = GameManager.Instance.CheckPointPos;
        var spawnPos = new Vector3(checkpointPos.x, checkpointPos.y, transform.position.z);
        transform.position = spawnPos;
        Direction = GameManager.Instance.RespawnFacingLeft ? -1.0f : 1.0f;
        PlayerState = PlayerStateEnum.Run;
    }
}
