using UnityEngine;

namespace UI
{
    /// <summary>
    ///     Attached to a box collider region to start showing a tutorial move
    /// </summary>
    public class TutorialStart : MonoBehaviour
    {
        [SerializeField] private string tutorialMove;

        [Tooltip("Text to display for tutorial move")] [SerializeField] [TextArea(0, 3)]
        private string tutorialText;

        [Tooltip("Position to show the tutorial, relative to the center of the screen")] [SerializeField]
        private Vector2 tutorialPosition;

        [Tooltip("When the player reaches this game object's region, it'll hide the tutorial")] [SerializeField]
        private TutorialEnd tutorialEnd;

        private void Awake()
        {
            if (tutorialEnd)
                tutorialEnd.OnEnd += () => TutorialMenu.Instance.HideTutorial();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player") || tutorialMove == "") return;
            TutorialMenu.Instance.ShowTutorial(tutorialPosition, tutorialMove, tutorialText);
            tutorialMove = "";
        }
    }
}
