using System;
using System.IO;
using System.Threading;
using Managers;
using Player;
using StringmoreCamera;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Save
{
    /// <summary>
    /// Manager to save and load persistent save data.
    /// </summary>
    public class SaveDataManager : MonoBehaviour
    {
        private static readonly string SaveFileName = "data.save";

        private Thread _saveThread;
        private Vector2? _forcedNextFramePosition;
        private GameManager _gameManager;

        private void Awake()
        {
            _gameManager = GameManager.Instance;
            _gameManager.saveGame += SaveFile;
            SceneManager.sceneLoaded += SceneManagerOnSceneLoaded;
        }

        private void OnDestroy()
        {
            _gameManager.saveGame -= SaveFile;
            SceneManager.sceneLoaded -= SceneManagerOnSceneLoaded;
        }

        /// <summary>
        /// Called on scene load: sets the position of required objects.
        /// </summary>
        /// <param name="scene">New scene</param>
        /// <param name="mode">Scene load mode</param>
        /// <remarks>
        /// TODO this is hacky
        /// </remarks>
        private void SceneManagerOnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (SceneListManager.Instance.IsMainMenu(scene.name)) return;

            if (_forcedNextFramePosition != null)
            {
                // TODO sendMessage is hacky and WILL BREAK
                FindObjectOfType<PlayerController>()?.SendMessage("OnReset");
                FollowCamera followCamera = FindObjectOfType<FollowCamera>();
                if (followCamera)
                {
                    Vector3 position = followCamera.transform.position;
                    position.x = _forcedNextFramePosition.Value.x;
                    position.y = _forcedNextFramePosition.Value.y;
                    followCamera.transform.position = position;
                }

                _forcedNextFramePosition = null;
            }
        }

        /// <summary>
        /// Returns the current save state data.
        /// </summary>
        /// <returns>Current save state date</returns>
        private SaveFileData GetCurrentSaveData(string sceneOverride = null)
        {
            return new SaveFileData
            {
                saveData = new SaveData
                {
                    checkpointFacesLeft = _gameManager.RespawnFacingLeft,
                    dateTimeBinary = DateTime.Now.ToBinary(),
                    levelsAccessed = _gameManager.LevelsAccessed,

                    level1Data = _gameManager.allLevelData[0],
                    level2Data = _gameManager.allLevelData[1],
                    level3Data = _gameManager.allLevelData[2],
                    level4Data = _gameManager.allLevelData[3]
                }
            };
        }

        /// <summary>
        /// Determines if the save file is present.
        /// </summary>
        /// <returns>True if file is present</returns>
        public static bool HasExistingSave()
        {
            return File.Exists(Path.Combine(Application.persistentDataPath, "saves", SaveFileName));
        }

        /// <summary>
        /// Reads from the save file and returns the saved file data, if present.
        /// </summary>
        /// <returns>Save file data, or null if not present</returns>
        public SaveFileData? ReadExistingSave()
        {
            string folderLocation = Path.Combine(Application.persistentDataPath, "saves");
            if (!EnsureSaveFolderExists(folderLocation)) return null;
            string filePath = Path.Combine(folderLocation, SaveFileName);
            if (!File.Exists(filePath)) return null;
            try
            {
                SaveFileData saveFileData = JsonUtility.FromJson<SaveFileData>(File.ReadAllText(filePath));
                return saveFileData;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Couldn't read from save data: {filePath}: {e.Message}");
                return null;
            }
        }

        private SaveData? LoadAndApplySaveData()
        {
            SaveFileData? optionalData = ReadExistingSave();
            if (optionalData == null) return null;

            SaveData saveData = optionalData.Value.saveData;
            _gameManager.UpdateFromSaveData(saveData);
            return saveData;
        }

        public bool PreloadSaveData()
        {
            return LoadAndApplySaveData() != null;
        }


        /// <summary>
        /// Creates a new save file.
        /// </summary>
        public void CreateNewSave(string startingScene)
        {
            string folderLocation = Path.Combine(Application.persistentDataPath, "saves");
            if (!EnsureSaveFolderExists(folderLocation)) return;
            string outputFile = Path.Combine(folderLocation, SaveFileName);
            try
            {
                File.WriteAllText(outputFile, JsonUtility.ToJson(GetCurrentSaveData(startingScene)));
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Couldn't write to save data folder: {outputFile}: {e.Message}");
            }
        }

        /// <summary>
        /// Saves the file: creates the thread to save the file.
        /// </summary>
        /// <remarks>
        /// TODO: do we need to do this on a separate thread? would be easier but would cause freezing at checkpoint reach
        ///
        /// also TODO check this works with web since javascript is singlethreaded
        /// </remarks>
        public void SaveFile()
        {
            if (SceneListManager.Instance.InMainMenu)
            {
                Debug.LogWarning("Saving to main menu scene. Hopefully we're loading from it and not loading to it.");
                return;
            }

            _saveThread?.Join();
            string folderLocation = Path.Combine(Application.persistentDataPath, "saves");
            SaveFileWriter saveFileWriter = new(folderLocation, GetCurrentSaveData());
            _saveThread = new Thread(saveFileWriter.SaveFile);
            _saveThread.Start();
        }

        /// <summary>
        /// Somewhat more thread-safe save file writer.
        /// </summary>
        private class SaveFileWriter
        {
            private readonly string folderPath;
            private readonly SaveFileData saveFileData;

            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="folderPath">Folder path of saves directory</param>
            /// <param name="saveFileData">Data to save</param>
            /// <remarks>
            /// <see cref="Application.persistentDataPath"/> needs to be called in the main thread.
            /// </remarks>
            public SaveFileWriter(string folderPath, SaveFileData saveFileData)
            {
                this.folderPath = folderPath;
                this.saveFileData = saveFileData;
            }

            /// <summary>
            /// Saves the data to the save file.
            /// </summary>
            public void SaveFile()
            {
                if (!EnsureSaveFolderExists(folderPath)) return;
                string jsonData = JsonUtility.ToJson(saveFileData);
                string outputFilePath = Path.Combine(folderPath, SaveFileName);
                try
                {
                    File.WriteAllText(outputFilePath, jsonData);
                    Debug.Log($"Wrote to save file: {outputFilePath}");
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"Failed to write out save data at {outputFilePath}: {e.Message}");
                }
            }
        }

        /// <summary>
        /// Ensures the provided folder exists by creating it if not found.
        /// </summary>
        /// <param name="folderLocation">Folder path to ensure exists</param>
        /// <returns>True if successful and present, false otherwise</returns>
        private static bool EnsureSaveFolderExists(string folderLocation)
        {
            if (Directory.Exists(folderLocation)) return true;
            try
            {
                Directory.CreateDirectory(folderLocation);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Couldn't create save file directory {folderLocation}: {e.Message}");
                return false;
            }

            return true;
        }
    }
}
