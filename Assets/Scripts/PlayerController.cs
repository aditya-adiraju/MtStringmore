using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Input")] 
    [SerializeField] private float buttonBufferTime;
    [Header("Collisions")]
    [SerializeField] private LayerMask collisionLayer;
    [SerializeField] private float collisionDistance;
    [Header("Ground")]
    [SerializeField] private float maxGroundSpeed;
    [SerializeField] private float groundAcceleration;
    [SerializeField] private float startDirection;
    [SerializeField] private float groundingForce;
    [Header("Jump")]
    [SerializeField] private float jumpPower;
    [SerializeField] private float doubleJumpPower;
    [Header("Air")]
    [SerializeField] private float maxFallSpeed;
    [SerializeField] private float fallAcceleration;
    // [SerializeField] private float earlyReleaseFallAcceleration;
    [Header("Wall")] 
    [SerializeField] private float wallJumpAngle;
    [SerializeField] private float wallJumpPower;
    [SerializeField] private float wallSlideSpeed;
    [SerializeField] private float wallSlideAcceleration;
    [Header("Swinging")] 
    [SerializeField] private LineRenderer ropeRenderer;
    [SerializeField] private float swingBoostMultiplier;
    [SerializeField] private float maxSwingSpeed;
    [SerializeField] private float swingAcceleration;
    [Header("Visual")] 
    [SerializeField] private Color doubleJumpUsedColor;
    [SerializeField] private Color doubleJumpUnusedColor;
    
    private Rigidbody2D _rb;
    private CapsuleCollider2D _col;
    private SpriteRenderer _sprite;

    private float _time;
    private float _timeButtonPressed;
    // private float _timeJumped;
    private bool _buttonUsed;
    private bool _isButtonHeld;
    
    private Vector2 _velocity;
    private bool _grounded;
    private bool _leftWallSliding;
    private bool _rightWallSliding;
    private bool _canDoubleJump;
    
    private Collider2D _swingArea;
    private bool _inSwingArea;
    private bool _isSwinging;
    private float _swingRadius;
    
    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _col = GetComponent<CapsuleCollider2D>();
        _sprite = GetComponent<SpriteRenderer>();
        
        _sprite.color = doubleJumpUnusedColor;
        _buttonUsed = true;
    }

    private void Update()
    {
        _time += Time.deltaTime;

        if (Input.GetButtonDown("Jump"))
        {
            _timeButtonPressed = _time;
            _buttonUsed = false;
        }
        _isButtonHeld = Input.GetButton("Jump");

        RedrawRope();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("SwingArea"))
        {
            _swingArea = other;
            _inSwingArea = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("SwingArea"))
        {
            _inSwingArea = false;
        }
    }

    private void FixedUpdate()
    {
        CheckCollisions();
        
        HandleWallJump();
        HandleSwing();
        HandleJump();
        HandleDoubleJump();
        HandleWalk();
        HandleGravity();
        
        ApplyMovement();
    }

    private void CheckCollisions()
    {
        bool groundHit = Physics2D.CapsuleCast(_col.bounds.center, _col.size, _col.direction, 0, Vector2.down, collisionDistance, collisionLayer);
        bool ceilingHit = Physics2D.CapsuleCast(_col.bounds.center, _col.size, _col.direction, 0, Vector2.up, collisionDistance, collisionLayer);
        bool leftWallHit = Physics2D.CapsuleCast(_col.bounds.center, _col.size, _col.direction, 0, Vector2.left, collisionDistance, collisionLayer);
        bool rightWallHit = Physics2D.CapsuleCast(_col.bounds.center, _col.size, _col.direction, 0, Vector2.right, collisionDistance, collisionLayer);
        
        if (ceilingHit) _velocity.y = Mathf.Min(0, _velocity.y);

        _leftWallSliding = leftWallHit;
        _rightWallSliding = rightWallHit;

        if (!_grounded && groundHit)
        {
            _grounded = true;
            _canDoubleJump = true;
            _sprite.color = doubleJumpUnusedColor;
        }
        else if (_grounded && !groundHit) _grounded = false;
    }

    private void HandleWallJump()
    {
        if ((_rightWallSliding || _leftWallSliding) && !_grounded && CanUseButton())
        {
            _velocity = new Vector2(Mathf.Cos(wallJumpAngle), Mathf.Sin(wallJumpAngle)) * wallJumpPower;
            if (_rightWallSliding) _velocity.x = -_velocity.x;
            
            _buttonUsed = true;
            _leftWallSliding = false;
            _rightWallSliding = false;
        }
    }

    private void HandleJump()
    {
        if (_grounded && CanUseButton())
        {
            _velocity.y = jumpPower;
            _buttonUsed = true;
            _grounded = false;
        }
    }

    private void HandleDoubleJump()
    {
        if (CanUseButton() && _canDoubleJump)
        {
            _buttonUsed = true;
            _canDoubleJump = false;
            
            _velocity.y = doubleJumpPower;
            _sprite.color = doubleJumpUsedColor;
        }
    }

    private void HandleWalk()
    {
        if (_grounded && !_leftWallSliding && !_rightWallSliding)
        {
            float direction = Mathf.Sign(_velocity.x);
            if (direction == 0f) direction = startDirection;
            _velocity.x = Mathf.MoveTowards(_velocity.x, maxGroundSpeed * direction, groundAcceleration * Time.fixedDeltaTime);
        }
    }

    private void HandleGravity()
    {
        if (_grounded && _velocity.y <= 0f)
        {
            _velocity.y = -groundingForce;
        }
        else if ((_leftWallSliding || _rightWallSliding) && _velocity.y <= 0f)
        {
            _velocity.y = Mathf.MoveTowards(_velocity.y, -wallSlideSpeed, wallSlideAcceleration * Time.fixedDeltaTime);
        }
        else
        {
            _velocity.y = Mathf.MoveTowards(_velocity.y, -maxFallSpeed, fallAcceleration * Time.fixedDeltaTime);
        }
    }

    private void HandleSwing()
    {
        if (!_isSwinging && CanUseButton() && _inSwingArea)
        {
            // start swing
            _isSwinging = true;
            _buttonUsed = true;
            _swingRadius = Vector2.Distance(_swingArea.transform.position, transform.position);
            // Debug.Log("Swing started with radius " + _swingRadius);
            // Debug.Log($"swing pos: {_swingArea.transform.position} my pos: {transform.position}");
            ropeRenderer.enabled = true;
        }
        
        if (_isSwinging && !_isButtonHeld)
        {
            // stop swing
            _isSwinging = false;
            ropeRenderer.enabled = false;
            // boost velocity if going up
            if (_velocity.y >= 0f) _velocity *= swingBoostMultiplier;
        }

        if (_isSwinging)
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
            // Debug.Log($"Swing speed: {boostedVelocity.magnitude}");
            // Debug.Log("rel pos: " + relPos + " test pos: " + testPos + " new pos: " + newPos);
        }
    }

    private void RedrawRope()
    {
        if (_isSwinging)
        {
            ropeRenderer.SetPosition(0, transform.position);
            ropeRenderer.SetPosition(1, _swingArea.transform.position);
        }
    }

    private void ApplyMovement()
    {
        _rb.velocity = _velocity;
        Debug.DrawRay(transform.position, _velocity, Color.magenta);
    }
    
    private bool CanUseButton()
    {
        return !_buttonUsed && _time <= _timeButtonPressed + buttonBufferTime;
    }
}
