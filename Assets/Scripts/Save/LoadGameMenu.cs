using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Util;

namespace Save
{
    /// <summary>
    /// Main 'Load Game' menu logic.
    /// </summary>
    public class LoadGameMenu : MonoBehaviour
    {
        [SerializeField, Tooltip("Location to place all the button prefabs")]
        private RectTransform locationOfChildren;

        [SerializeField] private LoadGameMenuButton loadGameMenuButtonPrefab;
        [SerializeField] private Button loadButton;
        [SerializeField] private Button deleteButton;

        private SaveDataManager _saveDataManager;
        private string _activeFile;

        private void Awake()
        {
            _saveDataManager = FindObjectOfType<SaveDataManager>();
        }

        private void OnEnable()
        {
            RefreshView();
        }

        /// <summary>
        /// Called by button: sets the currently selected or 'active' save.
        /// </summary>
        /// <param name="fileName">File name of save</param>
        public void SetActiveSave(string fileName)
        {
            _activeFile = fileName;
            deleteButton.interactable = _activeFile != null;
            loadButton.interactable = _activeFile != null;
        }

        /// <summary>
        /// Called by button: loads the save.
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public void LoadSelectedSave()
        {
            _saveDataManager.LoadExistingSave(_activeFile);
        }

        /// <summary>
        /// Called by button: deletes the save.
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public void DeleteSelectedSave()
        {
            SaveDataManager.DeleteSave(_activeFile);
            _activeFile = null;
            RefreshView();
        }

        /// <summary>
        /// Refreshes the view by getting all save files and initializing all the buttons.
        /// </summary>
        private void RefreshView()
        {
            (string, SaveData)[] data = SaveDataManager.GetSaveFiles().Where(pair => pair.Value.HasValue)
                .Select(pair => (pair.Key, pair.Value.Value))
                .ToArray();
            ObjectUtil.EnsureLength(locationOfChildren, data.Length, loadGameMenuButtonPrefab);

            for (int i = 0; i < data.Length; i++)
            {
                LoadGameMenuButton loadGameMenuButton =
                    locationOfChildren.GetChild(i).GetComponent<LoadGameMenuButton>();
                loadGameMenuButton.Initialize(data[i].Item2, data[i].Item1);
            }

            deleteButton.interactable = _activeFile != null;
            loadButton.interactable = _activeFile != null;
        }
    }
}
