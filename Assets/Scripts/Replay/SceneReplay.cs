using System;
using UnityEngine;

namespace Replay
{
    /// <summary>
    /// Scene replay data container.
    /// </summary>
    public class SceneReplay : ScriptableObject
    {
        /// <summary>
        /// Scene name.
        /// </summary>
        // ReSharper disable once NotAccessedField.Global
        public string sceneName;

        /// <summary>
        /// List of previous attempts.
        /// </summary>
        public Attempt[] attempts;

        /// <summary>
        /// Individual attempt data.
        /// </summary>
        [Serializable]
        public struct Attempt
        {
            public Vector3[] locations;
        }
    }
}
