using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

namespace Save
{
    /// <summary>
    /// Load game button logic.
    /// </summary>
    public class LoadGameButton : MonoBehaviour
    {
        private SaveDataManager _saveDataManager;
        
        [SerializeField] private TextMeshProUGUI levelNumber;

        private void Awake()
        {
            _saveDataManager = FindObjectOfType<SaveDataManager>();
            if (!SaveDataManager.HasExistingSave())
            {
                gameObject.SetActive(false);
            }
            else
            {
                SaveFileData? saveFileData = _saveDataManager.ReadExistingSave();
                if (saveFileData != null)
                {
                    string sceneName = saveFileData.Value.saveData.sceneName;
                    // stole the following from stackoverflow and modified it
                    levelNumber.text = string.Join(' ', Regex.Split(sceneName, @"(?<!^)(?=[A-Z0-9])"));
                }
                else
                {
                    gameObject.SetActive(false);
                }
            }
        }

        /// <summary>
        /// Called by the button: loads the existing save.
        /// </summary>
        public void LoadFromSave()
        {
            _saveDataManager.LoadExistingSave();
        }
    }
}
