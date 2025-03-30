using UnityEngine;

/// <summary>
///     Handles Knitby's animation
/// </summary>
public class KnitbyAnimator : MonoBehaviour
{
    private static readonly int JumpKey = Animator.StringToHash("Jump");
    private static readonly int LandKey = Animator.StringToHash("Land");
    private static readonly int GroundedKey = Animator.StringToHash("Grounded");
    private static readonly int YVelocityKey = Animator.StringToHash("YVelocity");
    private static readonly int HitWallKey = Animator.StringToHash("HitWall");
    private static readonly int LeaveWallKey = Animator.StringToHash("LeaveWall");
    private static readonly int SwingKey = Animator.StringToHash("InSwing");
    private static readonly int IdleKey = Animator.StringToHash("Idle");
    [SerializeField] private Animator anim;
    [SerializeField] private GameObject deathSmoke;
    private KnitbyController _knitbyController;

    private SpriteRenderer _spriteRenderer;

    private void Awake()
    {
        _spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        _knitbyController = GetComponentInParent<KnitbyController>();
    }

    private void OnEnable()
    {
        _knitbyController.DirectionUpdated += OnMove;
        _knitbyController.GroundedChanged += OnGroundedChanged;
        _knitbyController.WallHitChanged += OnWallHitChanged;
        _knitbyController.Swing += OnSwing;
        _knitbyController.PlayerDeath += OnPlayerDeath;
        _knitbyController.SetIdle += OnIdle;
        GameManager.Instance.Reset += OnReset;
    }

    private void OnDisable()
    {
        _knitbyController.DirectionUpdated -= OnMove;
        _knitbyController.GroundedChanged -= OnGroundedChanged;
        _knitbyController.WallHitChanged -= OnWallHitChanged;
        _knitbyController.Swing -= OnSwing;
        _knitbyController.PlayerDeath -= OnPlayerDeath;
        _knitbyController.SetIdle -= OnIdle;

        GameManager.Instance.Reset -= OnReset;
    }

    private void OnIdle(bool value)
    {
        anim.SetBool(IdleKey, value);
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

    /// <summary>
    ///     On reset, re-enable animation
    /// </summary>
    public void OnReset()
    {
        anim.enabled = true;
    }
}
