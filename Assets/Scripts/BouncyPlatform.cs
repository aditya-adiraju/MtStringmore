using UnityEngine;

/// <summary>
/// Class represents a bouncy platform that is a 2D collider
/// </summary>
[DisallowMultipleComponent, RequireComponent(typeof(Collider2D), typeof(Animator))]
public class BouncyPlatform : MonoBehaviour, IPlayerVelocityEffector
{
    private static readonly int BounceHash = Animator.StringToHash("Bounce");

    #region Serialized Private Fields

    [Header("Bouncing")]
    [SerializeField] private float yBounceForce;
    [SerializeField] private float xBounceForce;

    #endregion

    private PlayerController _player;
    private Collision2D _bounceArea;
    private Animator _animator;

    /// <inheritdoc />
    public Vector2 ApplyVelocity(Vector2 velocity)
    {
        // apply at most once
        if (ReferenceEquals(_player.ActiveVelocityEffector, this))
            _player.ActiveVelocityEffector = null;
        return new Vector2(xBounceForce, yBounceForce);
    }

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.gameObject.TryGetComponent(out _player)) return;
        _player.ActiveVelocityEffector = this;
        _player.CanDash = true;
        if (_player.PlayerState == PlayerController.PlayerStateEnum.Dash)
        {
            _player.ForceCancelDash();
        }
        _animator.SetTrigger(BounceHash);
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.TryGetComponent(out PlayerController _)) return;
        _animator.ResetTrigger(BounceHash);
        if (ReferenceEquals(_player.ActiveVelocityEffector, this))
            _player.ActiveVelocityEffector = null;
    }
}
