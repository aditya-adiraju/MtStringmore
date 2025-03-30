using System;
using UnityEngine;

/// <summary>
/// Attached to a box collider region to stop showing a tutorial move.
/// </summary>
public class TutorialEnd : MonoBehaviour
{
    public event Action OnEnd;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        OnEnd?.Invoke();
        Destroy(gameObject);
    }
}