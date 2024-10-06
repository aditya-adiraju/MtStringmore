using System;
using UnityEngine;

/// <summary>
/// Controls player movement and invokes events for different player states
/// </summary>
public class PlayerController : MonoBehaviour
{
    #region Serialized Private Fields

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
    [SerializeField] private float fallAcceleration;
    [SerializeField] private float earlyReleaseFallAcceleration;
    [SerializeField] private float maxAirSpeed;
    [SerializeField] private float airAcceleration;
    [Header("Dash")] 
    [SerializeField] private float dashTime;
    [SerializeField] private float dashSpeed;
    [Header("Wall")] 
    [SerializeField] private float wallJumpAngle;
    [SerializeField] private float wallJumpPower;
    [SerializeField] private float wallSlideSpeed;
    [SerializeField] private float wallSlideAcceleration;
    [Header("Swinging")] 
    [SerializeField] private float swingBoostMultiplier;
    [SerializeField] private float maxSwingSpeed;
    [SerializeField] private float swingAcceleration;
    [Header("Visual")]
    [SerializeField] private LineRenderer ropeRenderer;
    [SerializeField] private int deathTime;
    
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
        Dash
    }
    
    public PlayerStateEnum PlayerState { get; private set; }
    
    /// <summary>
    /// Current velocity of the player.
    /// </summary>
    public Vector2 Velocity => _velocity;
    
    /// <summary>
    /// Facing direction of the player. -1.0 for left, 1.0 for right.
    /// </summary>
    public float Direction => _lastDirection;
    
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
    public event Action DoubleJumped; 
    public event Action Death;
    
    #endregion
    
    #region Private Properties
    
    private Rigidbody2D _rb;
    private CapsuleCollider2D _col;

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
    private int _dashDirection;
    private int _directionPressed;
    
    private Vector2 _velocity;
    private float _lastDirection;
    private bool _closeToWall;
    private Vector2 _groundNormal;
    
    private Collider2D _swingArea;
    private bool _enteredSwingArea;
    private float _swingRadius;
    
    #endregion

    #region Unity Event Handlers
    
    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _col = GetComponent<CapsuleCollider2D>();
        _buttonUsed = true;
        _lastDirection = startDirection;
    }

    private void Update()
    {
        _time += Time.deltaTime;

        GetInput();
        RedrawRope();  // TODO this should be moved outside player controller when knitby is real
    }

    private void GetInput()
    {
        bool leftDown = Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A);
        bool leftHeld = Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A);
        bool rightDown = Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D);
        bool rightHeld = Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D);
        
        if (leftDown || rightDown)
        {
            _timeButtonPressed = _time;
            _buttonUsed = false;
            if (leftDown) _directionPressed = -1;
            if (rightDown) _directionPressed = 1;
        }
        _isButtonHeld = leftHeld || rightHeld;

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
            _enteredSwingArea = true;
        }
        else if (other.gameObject.CompareTag("Death"))
        {
            if (PlayerState != PlayerStateEnum.Dead)
            {
                HandleDeath();
            }
        }
    }

    private void FixedUpdate()
    {
        if (PlayerState == PlayerStateEnum.Dead)
            return;
        
        CheckCollisions();
        
        HandleWallJump();
        HandleSwing();
        HandleJump();
        HandleDoubleJump();
        HandleEarlyRelease();
        HandleWalk();
        HandleGravity();
        if (dashEnabled) HandleDash();
        ApplyMovement();
    }
    
    #endregion

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
        }
        if (PlayerState == PlayerStateEnum.RightWallSlide && !rightWallHit || 
            PlayerState == PlayerStateEnum.LeftWallSlide && !leftWallHit) 
            PlayerState = PlayerStateEnum.Air;
    }

    private void HandleDeath()
    {
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
        if (CanUseButton() && PlayerState is PlayerStateEnum.LeftWallSlide && _directionPressed == 1 ||
            CanUseButton() && PlayerState is PlayerStateEnum.RightWallSlide && _directionPressed == -1)
        {
            _velocity = new Vector2(Mathf.Cos(wallJumpAngle), Mathf.Sin(wallJumpAngle)) * wallJumpPower;
            if (PlayerState == PlayerStateEnum.RightWallSlide) _velocity.x = -_velocity.x;
            
            _buttonUsed = true;
            PlayerState = PlayerStateEnum.Air;
            _canReleaseEarly = true;
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
            if ((int)Mathf.Sign(_velocity.x) != _directionPressed)
            {
                _velocity.x = -_velocity.x;
            }
            
            _buttonUsed = true;
            PlayerState = PlayerStateEnum.Air;
            _canReleaseEarly = true;
            Jumped?.Invoke();
            GroundedChanged?.Invoke(false, Mathf.Abs(_velocity.y));
        }
    }

    private void HandleDoubleJump()
    {
        // TODO add left/right behavior
        if (doubleJumpEnabled && CanUseButton() && _canDoubleJump && !_closeToWall)
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
        if (PlayerState is PlayerStateEnum.Air && CanUseButton() && _canDash)
        {
            // start a dash
            _canDash = false;
            _buttonUsed = true;
            PlayerState = PlayerStateEnum.Dash;
            _timeDashed = _time;
            _dashDirection = _directionPressed;
        } 
        else if (PlayerState is PlayerStateEnum.Dash)
        {
            // move player forward at dash speed
            _velocity.y = 0;
            _velocity.x = dashSpeed * _dashDirection;
            // check if dash is over
            if (_time - _timeDashed >= dashTime)
            {
                PlayerState = PlayerStateEnum.Air;
            }
        } 
        else if (PlayerState is PlayerStateEnum.Run or PlayerStateEnum.LeftWallSlide or PlayerStateEnum.RightWallSlide)
        {
            // can dash after landing on the ground or wall sliding
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
                Vector2 walkTarget = new Vector2(_groundNormal.y * _lastDirection, _groundNormal.x * -_lastDirection) * maxGroundSpeed;
                float newX = Mathf.MoveTowards(_velocity.x, walkTarget.x, groundAcceleration * Time.fixedDeltaTime);
                float newY = walkTarget.y;
                _velocity = new Vector2(newX, newY);
                break;
            case PlayerStateEnum.Air:
                if (Mathf.Abs(_velocity.x) < maxAirSpeed)
                    _velocity.x = Mathf.MoveTowards(_velocity.x, maxAirSpeed * _lastDirection, airAcceleration * Time.fixedDeltaTime);
                break;
        }
    }

    private void HandleGravity()
    {
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
                    _velocity.y = Mathf.MoveTowards(_velocity.y, -wallSlideSpeed, wallSlideAcceleration * Time.fixedDeltaTime);
                else goto default;
                break;
            default:
                float accel = _releasedEarly ? earlyReleaseFallAcceleration : fallAcceleration;
                _velocity.y = Mathf.MoveTowards(_velocity.y, -maxFallSpeed, accel * Time.fixedDeltaTime);
                break;
        }
    }

    private void HandleSwing()
    {
        if (_enteredSwingArea)
        {
            // start swing automatically when entering
            PlayerState = PlayerStateEnum.Swing;
            _swingRadius = Vector2.Distance(_swingArea.transform.position, transform.position);
            ropeRenderer.enabled = true;
        } else if (PlayerState is PlayerStateEnum.Swing && CanUseButton())
        {
            // press button to release
            // TODO: only release with correct direction
            PlayerState = PlayerStateEnum.Air;
            ropeRenderer.enabled = false;
            // boost velocity if going up
            // TODO: set a min x and y velocity instead?
            if (_velocity.y >= 0f) _velocity *= swingBoostMultiplier;
            _buttonUsed = true;
        } else if (PlayerState is PlayerStateEnum.Swing)
        {
            Vector2 relPos = transform.position - _swingArea.transform.position;
            // if going down, accelerate to target swing speed
            if (_velocity.y <= 0f && _velocity.magnitude <= maxSwingSpeed)
            {
                _velocity = _velocity.normalized * Mathf.MoveTowards(_velocity.magnitude, maxSwingSpeed, 
                    swingAcceleration * Time.fixedDeltaTime);
            }
            Vector2 testPos = relPos + _velocity * Time.fixedDeltaTime;
            Vector2 newPos = testPos.normalized * _swingRadius;
            _velocity = (newPos - relPos) / Time.fixedDeltaTime;
        }
        _enteredSwingArea = false;
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
        if (_velocity.x != 0f) _lastDirection = Mathf.Sign(_velocity.x);
        Debug.DrawRay(transform.position, _velocity, Color.magenta);
    }
    
    private bool CanUseButton()
    {
        return !_buttonUsed && _time <= _timeButtonPressed + buttonBufferTime;
    }
    
    #endregion
}
