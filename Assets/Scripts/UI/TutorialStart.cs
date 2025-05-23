using UnityEngine;

namespace UI
{
    /// <summary>
    /// Attached to a box collider region to start showing a tutorial move
    /// </summary>
    public class TutorialStart : MonoBehaviour
    {
        [SerializeField] private string tutorialMove;

        [Tooltip("When the player reaches this game object's region, it'll hide the tutorial")]
        [SerializeField] private TutorialEnd tutorialEnd;

        private TutorialBox _tutorialBox;

        private void Start()
        {
            _tutorialBox = FindObjectOfType<TutorialBox>();
            if (tutorialEnd)
                tutorialEnd.OnEnd += () => _tutorialBox.HideTutorial();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player") || tutorialMove == "") return;
            _tutorialBox.ShowTutorial(tutorialMove);
            tutorialMove = "";
        }
    }
}