using UnityEngine;

/// <summary>
/// Class represents a bouncy platform that is a 2D collider
/// </summary>
[DisallowMultipleComponent, RequireComponent(typeof(Collider2D))]
public class BouncyPlatform : MonoBehaviour, IPlayerVelocityEffector
{
    #region Serialized Private Fields

    [Header("Bouncing")]
    [SerializeField] private float yBounceForce;
    [SerializeField] private float xBounceForce;

    #endregion

    private PlayerController _player;
    private Collision2D _bounceArea;

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
    }
}
