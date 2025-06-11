using System;
using Managers;
using Player;
using UnityEngine;
using Yarn.Unity;

namespace Interactables
{
    /// <summary>
    ///     Checkpoint flag that sets checkpoint position when player collides with it
    /// </summary>
    public class Checkpoint : MonoBehaviour, IPlayerVelocityEffector
    {
        private static readonly int HoistKey = Animator.StringToHash("Hoisted");

        [Header("References")] [SerializeField]
        private Animator anim;

        [SerializeField] private SpriteRenderer sprite;

        [Tooltip("Node that starts from this checkpoint. Set to \"\" to not trigger dialog from checkpoint.")]
        [SerializeField]
        private string conversationStartNode;

        [Tooltip("If checked, the player faces left when they respawn on this checkpoint")]
        [SerializeField] private bool respawnFacingLeft;

        // internal properties not exposed to editor
        private DialogueRunner _dialogueRunner;
        private bool _isCurrentConversation;
        
        public bool hasBeenHit;
        public event Action OnCheckpointHit;
        
        public void Start()
        {
            hasBeenHit = false;
            _dialogueRunner = FindObjectOfType<DialogueRunner>();
            if (_dialogueRunner) _dialogueRunner.onDialogueComplete.AddListener(EndConversation);
        }

        private void HitCheckpoint()
        {
            if (hasBeenHit) return;
            hasBeenHit = true;
            OnCheckpointHit?.Invoke();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;
            PlayerController player = other.GetComponent<PlayerController>();
            if (player)
                player.AddPlayerVelocityEffector(this, true);
            if (anim.GetBool(HoistKey)) return;
            HitCheckpoint();
            anim.SetBool(HoistKey, true);
            GameManager.Instance.UpdateCheckpointData(transform.position, respawnFacingLeft);
        }

        public void StartConversation()
        {
            if (conversationStartNode == "") return;
            Debug.Log("Started dialogue at checkpoint.");
            _isCurrentConversation = true;
            _dialogueRunner.StartDialogue(conversationStartNode);
            Time.timeScale = 0;
        }

        private void EndConversation()
        {
            if (!_isCurrentConversation) return;
            _isCurrentConversation = false;
            Debug.Log("Ended dialogue at checkpoint.");
            Time.timeScale = 1;
        }

        /// <summary>
        /// Flips the player if they're going the wrong direction.
        /// </summary>
        /// <param name="velocity">Player's velocity</param>
        /// <returns>New velocity</returns>
        public Vector2 ApplyVelocity(Vector2 velocity)
        {
            float signX = respawnFacingLeft ? -1 : 1;
            return Mathf.Sign(velocity.x * signX) < 0 ? new Vector2(signX, velocity.y) : velocity;
        }
    }
}
