using Player;
using UnityEngine;

namespace Interactables.LetterBlock
{
    /// <summary>
    /// Simple listener to attach to the letter block object.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class CollisionListener : MonoBehaviour
    {
        private DashDestructibleObject _parent;
        private void Awake()
        {
            _parent = GetComponentInParent<DashDestructibleObject>();
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            PlayerController player = other.gameObject.GetComponent<PlayerController>();
            if (player) _parent.OnPlayerCollide(player);
        }
    }
}
