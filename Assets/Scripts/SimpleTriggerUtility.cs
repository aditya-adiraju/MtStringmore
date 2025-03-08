using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Simple utility class to invoke UnityEvents on triggers.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class SimpleTriggerUtility : MonoBehaviour
{
    [Tooltip("Layer mask to check collisions with")]
    public LayerMask layerMask;

    public UnityEvent onTriggerEnter;
    public UnityEvent onTriggerStay;
    public UnityEvent onTriggerExit;

    /// <summary>
    /// Checks if the other collider matches the layer mask.
    /// </summary>
    /// <param name="other">Other collider</param>
    /// <returns>True if the other object is in the layer mask</returns>
    private bool CheckLayer(Collider2D other)
    {
        return ((layerMask >> other.gameObject.layer) & 1) != 0;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (CheckLayer(other)) onTriggerEnter?.Invoke();
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (CheckLayer(other)) onTriggerStay?.Invoke();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (CheckLayer(other)) onTriggerExit?.Invoke();
    }
}
