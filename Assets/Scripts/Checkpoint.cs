using UnityEngine;
using Yarn.Unity;

/// <summary>
///     Checkpoint flag that sets checkpoint position when player collides with it
/// </summary>
public class Checkpoint : MonoBehaviour
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

    public void Start()
    {
        _dialogueRunner = FindObjectOfType<DialogueRunner>();
        if (_dialogueRunner) _dialogueRunner.onDialogueComplete.AddListener(EndConversation);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player") || anim.GetBool(HoistKey)) return;
        anim.SetBool(HoistKey, true);
        GameManager.Instance.CheckPointPos = transform.position;
        GameManager.Instance.RespawnFacingLeft = respawnFacingLeft;
        StartConversation();
    }

    private void StartConversation()
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
}
