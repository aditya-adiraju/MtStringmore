using System;
using System.IO;
using System.Threading;
using Managers;
using UnityEngine;

namespace Save
{
    /// <summary>
    /// Manager to save and load persistent save data.
    /// </summary>
    public static class SaveDataManager
    {
        private static readonly string SaveFileName = "data.save";
        private static readonly ReaderWriterLock FileWriteLock = new();

        private static Thread _saveThread;

        /// <summary>
        /// Returns the current save state data.
        /// </summary>
        /// <returns>Current save state date</returns>
        private static SaveFileData GetCurrentSaveData()
        {
            GameManager gameManager = GameManager.Instance;
            return new SaveFileData
            {
                saveData = new SaveData
                {
                    dateTimeBinary = DateTime.Now.ToBinary(),
                    levelsAccessed = gameManager.LevelsAccessed,
                    level1Data = gameManager.AllLevelData[0],
                    level2Data = gameManager.AllLevelData[1],
                    level3Data = gameManager.AllLevelData[2],
                    level4Data = gameManager.AllLevelData[3]
                }
            };
        }

        /// <summary>
        /// Reads from the save file and returns the saved file data, if present.
        /// </summary>
        /// <returns>Save file data, or null if not present</returns>
        public static SaveFileData? ReadExistingSave()
        {
            string folderLocation = Path.Combine(Application.persistentDataPath, "saves");
            if (!EnsureSaveFolderExists(folderLocation)) return null;
            string filePath = Path.Combine(folderLocation, SaveFileName);
            if (!File.Exists(filePath)) return null;
            try
            {
                FileWriteLock.AcquireReaderLock(1000);
                SaveFileData saveFileData = JsonUtility.FromJson<SaveFileData>(File.ReadAllText(filePath));
                return saveFileData;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Couldn't read from save data: {filePath}: {e.Message}");
                return null;
            }
            finally
            {
                try
                {
                    FileWriteLock.ReleaseReaderLock();
                }
                catch (ApplicationException)
                {
                }
            }
        }

        /// <summary>
        /// Deletes the save file.
        /// </summary>
        public static void DeleteSaveData()
        {
            string folderLocation = Path.Combine(Application.persistentDataPath, "saves");
            if (!EnsureSaveFolderExists(folderLocation)) return;
            string filePath = Path.Combine(folderLocation, SaveFileName);
            if (!File.Exists(filePath)) return;
            try
            {
                FileWriteLock.AcquireWriterLock(1000);
                File.Delete(filePath);
            }
            finally
            {
                try
                {
                    FileWriteLock.ReleaseWriterLock();
                }
                catch (ApplicationException)
                {
                }
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
        public static void SaveFile()
        {
            if (SceneListManager.Instance.InMainMenu)
            {
                Debug.LogWarning("Saving to main menu scene. Hopefully we're loading from it and not loading to it.");
                return;
            }

            _saveThread?.Join();
            string folderLocation = Path.Combine(Application.persistentDataPath, "saves");
            SaveFileWriter saveFileWriter = new(folderLocation, GetCurrentSaveData());
            _saveThread = new Thread(saveFileWriter.WriteFile);
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
            public void WriteFile()
            {
                if (!EnsureSaveFolderExists(folderPath)) return;
                string jsonData = JsonUtility.ToJson(saveFileData);
                string outputFilePath = Path.Combine(folderPath, SaveFileName);
                try
                {
                    FileWriteLock.AcquireWriterLock(1000);
                    File.WriteAllText(outputFilePath, jsonData);
                    Debug.Log($"Wrote to save file: {outputFilePath}");
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"Failed to write out save data at {outputFilePath}: {e.Message}");
                }
                finally
                {
                    try
                    {
                        FileWriteLock.ReleaseWriterLock();
                    }
                    catch (ApplicationException)
                    {
                    }
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
