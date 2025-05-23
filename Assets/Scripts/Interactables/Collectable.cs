using Managers;
using UnityEngine;

namespace Interactables
{
    /// <summary>
    /// This object is collected upon collision with the Player, incrementing the total collectable count.
    /// </summary>
    [RequireComponent(typeof(CircleCollider2D))]
    public class Collectable : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                GameManager.Instance.NumCollected++;
                // TODO: play a visual/particle effect before destroying
                Destroy(gameObject);
            }
        }
    }
}
