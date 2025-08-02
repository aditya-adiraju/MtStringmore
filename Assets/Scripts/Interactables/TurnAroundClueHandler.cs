using Player;
using UnityEngine;

namespace Interactables
{
    /// <summary>
    /// Class to show an object to turn around when the player's going the wrong way.
    /// </summary>
    public class TurnAroundClueHandler : MonoBehaviour
    {
        private Checkpoint _checkpoint;
        private GameObject _turnBackText;
        private bool _playerInTrigger;
        private PlayerController _player;
        private Collider2D _checkpointCollider;
        
        private void Awake()
        {
            _checkpoint = GetComponentInParent<Checkpoint>();
            _checkpointCollider = _checkpoint.GetComponent<Collider2D>();
            _turnBackText = transform.GetChild(0).gameObject;
            _playerInTrigger = false;
            _player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        }

        private void FixedUpdate()
        {
            if (!_playerInTrigger) return;
            
            if (_checkpoint.respawnFacingLeft)
            {
                _turnBackText.SetActive(_player.Direction > 0 && _player.transform.position.x < _checkpointCollider.bounds.max.x);
            }
            else
            {
                _turnBackText.SetActive(_player.Direction < 0 && _player.transform.position.x > _checkpointCollider.bounds.min.x);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.gameObject.TryGetComponent(out PlayerController _)) return;
            _playerInTrigger = true;
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!other.gameObject.TryGetComponent(out PlayerController _)) return;
            _playerInTrigger = false;
        }
    }
}
