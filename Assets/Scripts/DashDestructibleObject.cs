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

    private Collider2D _collider;

    private void Awake()
    {
        _collider = GetComponent<Collider2D>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (destroyed) return;
        if (collision.collider.TryGetComponent(out PlayerController playerController) &&
            playerController.PlayerState == PlayerController.PlayerStateEnum.Dash)
        {
            destroyed = true;
            _collider.enabled = false;
            onDestroyed?.Invoke();
        }
    }
}
