using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Collisions")]
    [SerializeField] private LayerMask collisionLayer;
    [SerializeField] private float collisionDistance;
    [Header("Ground")]
    [SerializeField] private float maxMoveSpeed;
    [SerializeField] private float moveAcceleration;
    [SerializeField] private float startDirection;
    [Header("Air")]
    [SerializeField] private float jumpPower;
    [SerializeField] private float maxFallSpeed;
    [SerializeField] private float fallAcceleration;
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
    
    private Rigidbody2D _rb;
    private CapsuleCollider2D _col;
    
    private bool _doButtonPressed;
    private bool _isButtonHeld;
    
    private Vector2 _velocity;
    private float _direction;
    private bool _grounded;
    private bool _leftWallSliding;
    private bool _rightWallSliding;
    
    private Collider2D _swingArea;
    private bool _isSwinging;
    private float _swingRadius;
    
    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _col = GetComponent<CapsuleCollider2D>();

        _direction = startDirection;
    }

    private void Update()
    {
        if (Input.GetButtonDown("Jump")) _doButtonPressed = true;
        _isButtonHeld = Input.GetButton("Jump");

        RedrawRope();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("SwingArea"))
        {
            _swingArea = other;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("SwingArea"))
        {
            _swingArea = null;
        }
    }

    private void FixedUpdate()
    {
        CheckCollisions();
        HandleWallJump();
        HandleJump();
        HandleWalk();
        HandleGravity();
        HandleSwing();
        ApplyMovement();
        
        _doButtonPressed = false;
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

        if (!_grounded && groundHit) _grounded = true;
        else if (_grounded && !groundHit) _grounded = false;
    }

    private void HandleWallJump()
    {
        if ((_rightWallSliding || _leftWallSliding) && _doButtonPressed && !_grounded)
        {
            _velocity = new Vector2(Mathf.Cos(wallJumpAngle), Mathf.Sin(wallJumpAngle)) * wallJumpPower;
            if (_rightWallSliding) _velocity.x = -_velocity.x;
            
            _direction = _leftWallSliding ? 1 : -1;
            _doButtonPressed = false;
            _leftWallSliding = false;
            _rightWallSliding = false;
        }
    }

    private void HandleJump()
    {
        if (_grounded && _doButtonPressed)
        {
            _velocity.y = jumpPower;
            _doButtonPressed = false;
            _grounded = false;
        }
    }

    private void HandleWalk()
    {
        if (_grounded && !_leftWallSliding && !_rightWallSliding)
        {
            _velocity.x = Mathf.MoveTowards(_velocity.x, maxMoveSpeed * _direction, moveAcceleration * Time.fixedDeltaTime);
        }
    }

    private void HandleGravity()
    {
        if (_grounded && _velocity.y <= 0f)
        {
            _velocity.y = 0f;
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
        if (!_isSwinging && _doButtonPressed && _swingArea is not null)
        {
            _isSwinging = true;
            _doButtonPressed = false;
            _swingRadius = Vector2.Distance(_swingArea.transform.position, transform.position);
            // Debug.Log("Swing started with radius " + _swingRadius);
            // Debug.Log($"swing pos: {_swingArea.transform.position} my pos: {transform.position}");
            ropeRenderer.enabled = true;
        }

        if (_isSwinging && (_swingArea is null || !_isButtonHeld))
        {
            _isSwinging = false;
            // Debug.Log("Swing stopped");
            // boost velocity after letting go
            _velocity *= swingBoostMultiplier;
            ropeRenderer.enabled = false;
        }

        if (_isSwinging && _swingArea is not null)
        {
            Vector2 relPos = transform.position - _swingArea.transform.position;  // pos relative to centre of swing area
            Vector2 boostedVelocity = _velocity.normalized * Mathf.MoveTowards(_velocity.magnitude, maxSwingSpeed,
                swingAcceleration * Time.fixedDeltaTime);
            Vector2 testPos = relPos + boostedVelocity * Time.fixedDeltaTime;
            Vector2 newPos = testPos.normalized * _swingRadius;
            _velocity = (newPos - relPos) / Time.fixedDeltaTime;
            // Debug.Log($"Swing speed: {boostedVelocity.magnitude}");
            // Debug.Log("rel pos: " + relPos + " test pos: " + testPos + " new pos: " + newPos);
        }
    }

    private void RedrawRope()
    {
        if (_isSwinging && _swingArea)
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
}
