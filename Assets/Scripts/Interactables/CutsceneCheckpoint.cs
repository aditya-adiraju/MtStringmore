using Player;
using UnityEngine;
using Yarn.Unity;

namespace Interactables
{
    /// <summary>
    ///     Invisible checkpoint that triggers cutscene immediately after collision;
    ///     after cutscene completion, triggers normal checkpoint stuff
    /// </summary>
    public class CutsceneCheckpoint : Checkpoint
    {
        /// <inheritdoc cref="AbstractPlayerInteractable.OnPlayerEnter"/>
        public override void OnPlayerEnter(PlayerController player)
        {
            StartConversation();
        }
        
        public override void StartConversation()
        {
            if (conversationStartNode == "") return;
            if (IsCurrentConversation) return;
            Debug.Log("Started dialogue at checkpoint.");
            IsCurrentConversation = true;
            DialogRunner.StartDialogue(conversationStartNode);
        }

        [YarnCommand("trigger_checkpoint")]
        public void TriggerCheckpoint()
        {
            HitCheckpoint();
        }

        public override void StartInteract(PlayerController player)
        {
        }
        
    }
}
