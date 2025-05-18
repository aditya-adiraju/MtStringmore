using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Object that is 'destroyed' on dash. And by destroy, I mean disable the collider.
///
/// If there's a trigger, it also detects if the player enters the trigger.
/// </summary>
[RequireComponent(typeof(Collider2D), typeof(AudioSource))]
public class DashDestructibleObject : MonoBehaviour
{
    [SerializeField] private bool destroyed;
    [SerializeField] private UnityEvent onDestroyed;
    [SerializeField, Tooltip("Tolerance to allow destruction after player dash (seconds)"), Min(0)]
    private float dashEndTolerance = 1f;

    private Collider2D _collider;
    private AudioSource _audioSource;

    private void Awake()
    {
        _collider = GetComponent<Collider2D>();
        _audioSource = GetComponent<AudioSource>();
        GameManager.Instance.Reset += OnReset;
    }

    /// <summary>
    /// Called on reset: re-enables the collider.
    /// </summary>
    private void OnReset()
    {
        destroyed = false;
        _collider.enabled = true;
    }

    private void OnDisable()
    {
        GameManager.Instance.Reset -= OnReset;
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

    /// <summary>
    /// "Destroys" this object.
    /// </summary>
    private void DestroyObject()
    {
        destroyed = true;
        _collider.enabled = false;
        _audioSource.Play();
        onDestroyed?.Invoke();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (destroyed) return;
        if (collision.collider.TryGetComponent(out PlayerController playerController) &&
            ShouldPlayerDestroy(playerController))
        {
            DestroyObject();
        }
    }

    // hack to allow tolerance without collisions
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (destroyed) return;
        if (other.TryGetComponent(out PlayerController playerController) &&
            // because in some wacky cases you can enter trigger while moving in the opposite direction
            Vector2.Dot(transform.position - playerController.transform.position, playerController.Velocity) > 0 &&
            playerController.PlayerState == PlayerController.PlayerStateEnum.Dash)
        {
            DestroyObject();
        }
    }
}
