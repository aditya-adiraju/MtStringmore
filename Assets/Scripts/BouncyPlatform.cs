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
        if (_bounceArea.contactCount == 0) return velocity;
        Vector2 directionVector = Vector2.Reflect(velocity, _bounceArea.GetContact(0).normal);
        // apply at most once
        if (ReferenceEquals(_player.ActiveVelocityEffector, this))
            _player.ActiveVelocityEffector = null;
        return new Vector2(xBounceForce * Mathf.Sign(directionVector.x), yBounceForce);
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (!other.gameObject.TryGetComponent(out _player)) return;
        _bounceArea = other;
        _player.ActiveVelocityEffector = this;
        _player.CanDash = true;
    }

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the player touched the object
        if (other.CompareTag("Player"))
            // Trigger the animation
            _animator.SetTrigger(BounceHash);
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        // Check if the player touched the object
        if (other.CompareTag("Player"))
            // Trigger the animation
            _animator.ResetTrigger(BounceHash);
    }
}
