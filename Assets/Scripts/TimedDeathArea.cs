using System.Collections;
using UnityEngine;

/// <summary>
/// Area that increases the <see cref="PlayerAnimator.RoastState"/> of the player and kills them after a time delay.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class TimedDeathArea : MonoBehaviour
{
    [SerializeField, Min(0), Tooltip("Time until player death (seconds)")]
    private float timeToDeath;

    private PlayerController _playerController;
    private PlayerAnimator _playerAnimator;
    private Coroutine _coroutine;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.TryGetComponent(out _playerController)) return;
        _playerAnimator = _playerController.GetComponentInChildren<PlayerAnimator>();
        Debug.Assert(_playerAnimator);
        _coroutine = StartCoroutine(TimedDeathCoroutine());
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.GetComponent<PlayerController>()) return;
        _playerAnimator.RoastState = 0;
        if (_coroutine != null)
            StopCoroutine(_coroutine);
        _coroutine = null;
    }

    /// <summary>
    /// Coroutine that increases roast state and sends the death message.
    /// </summary>
    /// <returns>Coroutine</returns>
    private IEnumerator TimedDeathCoroutine()
    {
        for (int state = 1; state <= PlayerAnimator.NumRoastStates; state++)
        {
            _playerAnimator.RoastState = state;
            yield return new WaitForSeconds(timeToDeath / PlayerAnimator.NumRoastStates);
        }

        _playerController.ForceKill();
        _coroutine = null;
    }
}
