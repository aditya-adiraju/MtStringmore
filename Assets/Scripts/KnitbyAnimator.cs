using UnityEngine;

/// <summary>
/// Handles Knitby's animation
/// </summary>
public class KnitbyAnimator : MonoBehaviour
{
    [SerializeField] private Animator anim;
    [SerializeField] private GameObject deathSmoke;

    private SpriteRenderer _spriteRenderer;
    private KnitbyController _knitbyController;
    private Vector3 _lastPosition;

    private static readonly int JumpKey = Animator.StringToHash("Jump");
    private static readonly int LandKey = Animator.StringToHash("Land");
    private static readonly int GroundedKey = Animator.StringToHash("Grounded");
    private static readonly int YVelocityKey = Animator.StringToHash("YVelocity");
    private static readonly int HitWallKey = Animator.StringToHash("HitWall");
    private static readonly int LeaveWallKey = Animator.StringToHash("LeaveWall");
    private static readonly int SwingKey = Animator.StringToHash("InSwing");
    private static readonly int IdleKey = Animator.StringToHash("Idle");

    private void Awake()
    {
        _spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        _knitbyController = GetComponentInParent<KnitbyController>();
    }

    private void Start()
    {
        GameManager.Instance.Reset += OnReset;
    }

    private void FixedUpdate()
    {
        HandleIdle();
    }

    private void OnEnable()
    {
        _knitbyController.DirectionUpdated += OnMove;
        _knitbyController.GroundedChanged += OnGroundedChanged;
        _knitbyController.WallHitChanged += OnWallHitChanged;
        _knitbyController.Swing += OnSwing;
        _knitbyController.PlayerDeath += OnPlayerDeath;
    }

    private void OnDisable()
    {
        _knitbyController.DirectionUpdated -= OnMove;
        _knitbyController.GroundedChanged -= OnGroundedChanged;
        _knitbyController.WallHitChanged -= OnWallHitChanged;
        _knitbyController.Swing -= OnSwing;
        _knitbyController.PlayerDeath -= OnPlayerDeath;
        
        GameManager.Instance.Reset -= OnReset;
    }

    private void OnMove(float x, float y)
    {
        anim.SetFloat(YVelocityKey, y);
        _spriteRenderer.flipX = x < 0;
    }

    private void OnGroundedChanged(bool grounded)
    {
        anim.SetTrigger(grounded ? LandKey : JumpKey);
        anim.SetBool(GroundedKey, grounded);
    }

    private void OnWallHitChanged(bool wallHit)
    {
        anim.SetTrigger(wallHit ? HitWallKey : LeaveWallKey);
    }

    private void OnSwing(bool inSwing)
    {
        anim.SetBool(SwingKey, inSwing);
    }

    private void OnPlayerDeath()
    {
        Instantiate(deathSmoke, transform);
        anim.enabled = false;
    }
    
    private void HandleIdle()
    {
        // if paused, don't change the idle state
        if (Time.deltaTime == 0) return;
        // update idle state to whether position changed 
        float movement = Vector3.Distance(_lastPosition, transform.position);
        anim.SetBool(IdleKey, Mathf.Abs(movement) < 0.01);
        _lastPosition = transform.position;
    }

    /// <summary>
    /// On reset, re-enable animation
    /// </summary>
    public void OnReset()
    {
        anim.enabled = true;
    }
}
