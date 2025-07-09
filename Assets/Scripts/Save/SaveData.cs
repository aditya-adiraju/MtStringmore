using System;
using System.Collections.Generic;
using Managers;

namespace Save
{
    /// <summary>
    /// Save data.
    /// </summary>
    [Serializable]
    public struct SaveData
    {
        public long dateTimeBinary;
        public bool checkpointFacesLeft;
        public List<string> levelsAccessed;

        public LevelData level1Data;
        public LevelData level2Data;
        public LevelData level3Data;
        public LevelData level4Data;
    }

    [Serializable]
    public class LevelData
    {
        public int mostCandiesCollected = -1;
        public int totalCandiesInLevel = -1;
        public int leastDeaths = -1;
        public string bestTime = GameManager.EmptySaveTime;
    }

    /// <summary>
    /// Data saved in the save file.
    /// </summary>
    [Serializable]
    public struct SaveFileData
    {
        public SaveData saveData;
    }
}
