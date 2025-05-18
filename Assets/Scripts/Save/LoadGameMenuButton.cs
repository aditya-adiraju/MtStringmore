using System;
using System.IO;
using TMPro;
using UnityEngine;

namespace Save
{
    /// <summary>
    /// Load game menu button logic.
    /// </summary>
    [DisallowMultipleComponent]
    public class LoadGameMenuButton : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI sceneNameText;
        [SerializeField] private TextMeshProUGUI fileNameText;
        [SerializeField] private TextMeshProUGUI checkpointNumText;
        [SerializeField] private TextMeshProUGUI dateTimeText;

        private LoadGameMenu _loadGameMenu;
        private string _fileName;

        private void Awake()
        {
            _loadGameMenu = GetComponentInParent<LoadGameMenu>();
        }

        /// <summary>
        /// Initializes all the text fields to the correct values.
        /// </summary>
        /// <param name="saveData">Save data</param>
        /// <param name="fileName">File name of save</param>
        public void Initialize(SaveData saveData, string fileName)
        {
            sceneNameText.text = saveData.sceneName;
            fileNameText.text = Path.GetFileNameWithoutExtension(fileName);
            checkpointNumText.text = $"Checkpoint {saveData.checkpointsReached.Length}";
            dateTimeText.text = GetDateTimeString(DateTime.FromBinary(saveData.dateTimeBinary));
            _fileName = fileName;
        }

        /// <summary>
        /// Called by the button: tells the load game menu that this is now active
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public void OnButtonClick()
        {
            _loadGameMenu.SetActiveSave(_fileName);
        }

        /// <summary>
        /// Converts a DateTime into a nicer looking "x minute(s) ago" string.
        ///
        /// Yes, I could have a check for ==1 and do "1 minute ago" but I'm lazy.
        /// </summary>
        /// <param name="dateTime">DateTime in the past</param>
        /// <returns>Nicer looking date time string</returns>
        /// <remarks>
        /// Yes, I tested all cases.
        /// </remarks>
        private static string GetDateTimeString(DateTime dateTime)
        {
            DateTime now = DateTime.Now;
            TimeSpan offset = now - dateTime;
            Debug.Assert(offset.TotalSeconds >= 0);
            if (offset.TotalSeconds < 60)
            {
                // Localization? what's that?
                return $"{Mathf.RoundToInt((float)offset.TotalSeconds)} second(s) ago";
            }

            if (offset.TotalMinutes < 60)
            {
                return $"{Mathf.RoundToInt((float)offset.TotalMinutes)} minute(s) ago";
            }

            if (offset.TotalHours < 24)
            {
                return $"{Mathf.RoundToInt((float)offset.TotalHours)} hour(s) ago";
            }

            if (offset.TotalDays < 31)
            {
                return $"{Mathf.RoundToInt((float)offset.TotalDays)} days(s) ago";
            }

            return dateTime.ToString(now.Year - dateTime.Year == 0 ? "M" : "dd MMMM yyyy");
        }
    }
}
