using System;
using TMPro.EditorUtilities;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    [SerializeField] private LayerMask collisionLayer;
    [SerializeField] private float collisionDistance;
    [SerializeField] private float jumpPower;
    [SerializeField] private float moveSpeed;
    [SerializeField] private float maxFallSpeed;
    [SerializeField] private float fallAcceleration;
    
    private Rigidbody2D _rb;
    private CapsuleCollider2D _col;
    private PlayerInput _playerInput;
    private Vector2 _velocity;
    private bool _grounded;
    private bool _doJump;
    
    public struct PlayerInput
    {
        public bool JumpDown;
        public bool JumpHeld;
    }
    
    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _col = GetComponent<CapsuleCollider2D>();
    }

    private void Update()
    {
        _playerInput = new PlayerInput
        {
            JumpDown = Input.GetButtonDown("Jump"),
            JumpHeld = Input.GetButton("Jump")
        };

        if (_playerInput.JumpDown) _doJump = true;
    }

    private void FixedUpdate()
    {
        CheckCollisions();
        HandleJump();
        HandleDirection();
        HandleGravity();
        ApplyMovement();
    }

    private void CheckCollisions()
    {
        bool groundHit = Physics2D.CapsuleCast(_col.bounds.center, _col.size, _col.direction, 0, Vector2.down, collisionDistance, collisionLayer);
        bool ceilingHit = Physics2D.CapsuleCast(_col.bounds.center, _col.size, _col.direction, 0, Vector2.up, collisionDistance, collisionLayer);
        
        if (ceilingHit) _velocity.y = Mathf.Min(0, _velocity.y);

        if (!_grounded && groundHit) _grounded = true;
        else if (_grounded && !groundHit) _grounded = false;
    }

    private void HandleJump()
    {
        if (_grounded && _doJump) _velocity.y = jumpPower;
        _doJump = false;
    }

    private void HandleDirection()
    {
        _velocity.x = moveSpeed;
    }

    private void HandleGravity()
    {
        if (_grounded && _velocity.y <= 0f)
        {
            _velocity.y = 0f;
        }
        else
        {
            _velocity.y = Mathf.MoveTowards(_velocity.y, -maxFallSpeed, fallAcceleration * Time.fixedDeltaTime);
        }
    }

    private void ApplyMovement()
    {
        _rb.velocity = _velocity;
    }
}
