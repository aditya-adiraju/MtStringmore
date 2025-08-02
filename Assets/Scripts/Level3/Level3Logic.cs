using Interactables;
using Managers;
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
        [SerializeField] private Checkpoint secondHalfCheckpoint;

        private Checkpoint[] _checkpoints;
        private AttachableMovingObject[] _zippers;

        private void Awake()
        {
            _checkpoints = FindObjectsByType<Checkpoint>(FindObjectsSortMode.None);
            _zippers = FindObjectsByType<AttachableMovingObject>(FindObjectsSortMode.None);
        }

        private void Start()
        {
            // there's no guarantee we grab the right instance in Awake so we use Start
            GameManager.Instance.AreInteractablesEnabled = false;
            
            foreach (AttachableMovingObject zipper in _zippers)
            {
                zipper.SetTabVisible(false);
            }
        }
        
        [YarnCommand("cutscene_state")]
        public void SetCutsceneState(bool value)
        {
            foreach (GameObject obj in gameObjects)
            {
                obj.SetActive(!value);
            }
            foreach (GameObject obj in cutsceneObjects)
            {
                obj.SetActive(value);
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
