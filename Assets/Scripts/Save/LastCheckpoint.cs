using Interactables;
using Managers;
using UnityEngine;

namespace Save
{
    /// <summary>
    /// Additional logic attached to the last checkpoint.
    /// </summary>
    [RequireComponent(typeof(Checkpoint))]
    public class LastCheckpoint : MonoBehaviour
    {
        [SerializeField] private string nextLevel;

        /// <summary>
        /// Returns the attached checkpoint to this object.
        /// </summary>
        public Checkpoint AttachedCheckpoint
        {
            get
            {
                if (_checkpoint) return _checkpoint;
                return _checkpoint = GetComponent<Checkpoint>();
            }
        }

        private Checkpoint _checkpoint;

        /// <summary>
        /// Adds the next level to the list of levels accessed.
        /// </summary>
        public void UpdateLevelAccess()
        {
            if (GameManager.Instance.AddLevelAccessed(nextLevel)) return;
            SaveDataManager.SaveFile();
            Debug.Log("Unlocked: " + nextLevel);
        }
    }
}
