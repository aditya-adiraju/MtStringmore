using Player;
using UnityEngine;

namespace Interactables.Balloon
{
    /// <summary>
    /// Trigger to tell the balloon the player is closeby.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class BalloonTrigger : MonoBehaviour
    {
        [SerializeField] private Balloon balloon;

        private void OnTriggerEnter2D(Collider2D collision)
        {
            PlayerController player = collision.GetComponent<PlayerController>();
            if (player) balloon.OnPlayerEnterInflationZone();
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            PlayerController player = collision.GetComponent<PlayerController>();
            if (player) balloon.OnPlayerExitInflationZone();
        }
    }
}
