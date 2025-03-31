using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Object that is 'destroyed' on dash. And by destroy, I mean disable the collider.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class DashDestructibleObject : MonoBehaviour
{
    [SerializeField] private bool destroyed;
    [SerializeField] private UnityEvent onDestroyed;
    [SerializeField, Tooltip("Tolerance to allow destruction after player dash (seconds)"), Min(0)]
    private float dashEndTolerance = 1f;

    private Collider2D _collider;

    private void Awake()
    {
        _collider = GetComponent<Collider2D>();
    }

    /// <summary>
    /// Determines whether the player should 'destroy' this object (is dashing or was recently dashing).
    /// </summary>
    /// <param name="player">Player in question</param>
    /// <returns>True if the player is/was recently dashing.</returns>
    /// <remarks>
    /// To avoid the player destroying this object while walking after having just dashed,
    /// we check for whether they're in the air as well.
    /// </remarks>
    private bool ShouldPlayerDestroy(PlayerController player)
    {
        return player.PlayerState switch
        {
            PlayerController.PlayerStateEnum.Dash => true,
            PlayerController.PlayerStateEnum.Air => Time.time - player.TimeDashEnded <= dashEndTolerance,
            _ => false
        };
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (destroyed) return;
        if (collision.collider.TryGetComponent(out PlayerController playerController) &&
            ShouldPlayerDestroy(playerController))
        {
            destroyed = true;
            _collider.enabled = false;
            onDestroyed?.Invoke();
        }
    }
}
