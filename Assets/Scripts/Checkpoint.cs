
using UnityEngine;

/// <summary>
/// Checkpoint flag that sets checkpoint position when player collides with it
/// </summary>
public class Checkpoint : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager.Instance.CheckPointPos = transform.position;
        }
    }
}
