using System;
using Player;
using UnityEngine;

namespace Interactables
{
    public class TurnAroundClueHandler : MonoBehaviour
    {
        private bool _respawnFacingLeft;
        private GameObject _turnBackText;
        private bool _playerInTrigger;
        private PlayerController _player;
        private Collider2D _checkpointCollider;
        
        private void Awake()
        {
            Checkpoint checkpoint = GetComponentInParent<Checkpoint>();
            _respawnFacingLeft = checkpoint.respawnFacingLeft;
            _checkpointCollider = checkpoint.GetComponent<Collider2D>();
            _turnBackText = transform.GetChild(0).gameObject;
            _playerInTrigger = false;
            _player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        }

        private void Update()
        {
            if (!_playerInTrigger) return;
            
            if (_respawnFacingLeft)
            {
                _turnBackText.SetActive(_player.Direction > 0 && _player.transform.position.x < transform.position.x + _checkpointCollider.bounds.size.x / 2);
            }
            else
            {
                _turnBackText.SetActive(_player.Direction < 0 && _player.transform.position.x > transform.position.x - _checkpointCollider.bounds.size.x / 2);
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
