using System;
using TMPro.EditorUtilities;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    [SerializeField] private LayerMask collisionLayer;
    [SerializeField] private float collisionDistance;
    [SerializeField] private float jumpPower;
    [SerializeField] private float maxMoveSpeed;
    [SerializeField] private float moveAcceleration;
    [SerializeField] private float maxFallSpeed;
    [SerializeField] private float fallAcceleration;
    
    private Rigidbody2D _rb;
    private CapsuleCollider2D _col;
    
    private Vector2 _velocity;
    private bool _grounded;
    private bool _doButtonPressed;
    private bool _isButtonHeld;
    
    private Collider2D _swingArea;
    private bool _isSwinging;
    private float _swingRadius;
    
    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _col = GetComponent<CapsuleCollider2D>();
    }

    private void Update()
    {
        if (Input.GetButtonDown("Jump")) _doButtonPressed = true;
        _isButtonHeld = Input.GetButton("Jump");
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
        HandleJump();
        HandleDirection();
        HandleGravity();
        HandleSwing();
        ApplyMovement();
        
        _doButtonPressed = false;
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
        if (_grounded && _doButtonPressed)
        {
            _velocity.y = jumpPower;
            _doButtonPressed = false;
        }
    }

    private void HandleDirection()
    {
        if (!_isSwinging)
        {
            _velocity.x = Mathf.MoveTowards(_velocity.x, maxMoveSpeed, moveAcceleration * Time.fixedDeltaTime);
        }
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

    private void HandleSwing()
    {
        if (!_isSwinging && _doButtonPressed && _swingArea is not null)
        {
            _isSwinging = true;
            _doButtonPressed = false;
            _swingRadius = Vector3.Distance(_swingArea.transform.position, transform.position);
            Debug.Log("Swing started with radius " + _swingRadius);
        }

        if (_isSwinging && (_swingArea is null || !_isButtonHeld))
        {
            _isSwinging = false;
            Debug.Log("Swing stopped");
        }

        if (_isSwinging)
        {
            Debug.DrawLine(transform.position, _swingArea.transform.position);
            
            // TODO fix physics
            Vector2 relPos = transform.InverseTransformPoint(_swingArea.transform.position);
            Vector2 testPos = relPos + _velocity * Time.fixedDeltaTime;
            if (testPos.magnitude > _swingRadius)
            {
                testPos = testPos.normalized * _swingRadius;
                _velocity = relPos - testPos;
            }
        }
        
    }

    private void ApplyMovement()
    {
        _rb.velocity = _velocity;
    }
}
