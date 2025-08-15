using Interactables;
using Managers;
using UI;
using UnityEngine;
using UnityEngine.Events;
using Yarn.Unity;

namespace Level3
{
    /// <summary>
    /// Custom level 3 logic.
    /// </summary>
    public class Level3Logic : MonoBehaviour
    {
        [SerializeField] private UnityEvent onSecondHalfReached;
        [SerializeField] private GameObject[] gameObjects;
        [SerializeField] private GameObject[] cutsceneObjects;
        [SerializeField] private AudioClip secondHalfBGM;
        [SerializeField] private Checkpoint secondHalfCheckpoint;

        private Checkpoint[] _checkpoints;
        private AttachableMovingObject[] _zippers;
        private TimerManager _timerManager;

        private void Awake()
        {
            _checkpoints = FindObjectsByType<Checkpoint>(FindObjectsSortMode.None);
            _zippers = FindObjectsByType<AttachableMovingObject>(FindObjectsSortMode.None);
        }

        private void Start()
        {
            // there's no guarantee we grab the right instance in Awake so we use Start
            GameManager.Instance.AreInteractablesEnabled = false;
            _timerManager = FindAnyObjectByType<TimerManager>();
            
            foreach (AttachableMovingObject zipper in _zippers)
            {
                zipper.SetTabVisible(false);
            }
        }

        /// <summary>
        /// Special skip logic since the actual cutscene skip button is hella broken.
        /// </summary>
        public void SpecialSkipLogic()
        {
            SetCutsceneState(false);
            ReachSecondHalf();
            CutsceneFade.FadeIn();
            _timerManager.SetTimerState(true);
        }
        
        /// <summary>
        /// Sets whether we're in a cutscene or not, toggling objects appropriately.
        /// </summary>
        /// <param name="inCutscene">Whether we're in a cutscene</param>
        [YarnCommand("cutscene_state")]
        public void SetCutsceneState(bool inCutscene)
        {
            foreach (GameObject obj in gameObjects)
            {
                obj.SetActive(!inCutscene);
            }
            foreach (GameObject obj in cutsceneObjects)
            {
                obj.SetActive(inCutscene);
            }
        }

        /// <summary>
        /// Called to start second half - enables interactables, clears checkpoints and flips them.
        /// </summary>
        [YarnCommand("start_second_half")]
        public void ReachSecondHalf()
        {
            GameManager.Instance.AreInteractablesEnabled = true;
            // seems checkpoint calls this before adding itself
            // makes our life easier
            GameManager.Instance.ClearCheckpointData();
            GameManager.Instance.GetComponent<BGMManager>().PlayBGM(secondHalfBGM);
            
            foreach (Checkpoint checkpoint in _checkpoints)
            {
                if (checkpoint != secondHalfCheckpoint) checkpoint.FlipAndResetCheckpoint();
            }
            onSecondHalfReached.Invoke();
            
            foreach (AttachableMovingObject zipper in _zippers)
            {
                zipper.SetTabVisible(true);
            }
        }
    }
}
