using System;
using System.Collections.Generic;
using Managers;
using Player;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Replay
{
    /// <summary>
    /// Class that constantly monitors player location and writes it to a file.
    ///
    /// If we detect we're not in the editor, it destroys itself to save on performance.
    /// </summary>
    [DisallowMultipleComponent]
    public class PlayerReplayMonitor : MonoBehaviour
    {
        /// <summary>
        /// Colors to render the different attempts with.
        /// </summary>
        private static readonly Color[] PreviewColorCycle =
            { Color.yellow, Color.red, Color.green, Color.blue, Color.magenta, Color.cyan };

        [SerializeField] private SceneReplay sceneReplayPreview;

        [SerializeField, Tooltip("Warning: likely performance intensive!")]
        private bool showPreview = true;

        private PlayerController _player;
        private string _activeSceneName;
        private readonly List<SceneReplay.Attempt> _prevAttempts = new();
        private readonly List<Vector3> _currAttempt = new();

        private void Awake()
        {
            if (!Application.isEditor)
            {
                Debug.LogWarning("PlayerReplayMonitor is only supported on Editor.");
                Destroy(this);
            }

            _player = FindObjectOfType<PlayerController>();

            SceneManager.sceneLoaded += OnSceneLoaded;
            _activeSceneName = SceneManager.GetActiveScene().name;
            _prevAttempts.Clear();
            _currAttempt.Clear();
            GameManager.Instance.Reset += OnReset;
        }

        private void FixedUpdate()
        {
            if (_player) _currAttempt.Add(_player.transform.position);
        }

        private void OnDestroy()
        {
            AddCurrentAttempt();
            WriteOutSceneReplay();
        }

        private void OnDrawGizmos()
        {
            if (!showPreview) return;
            if (sceneReplayPreview)
            {
                ShowAttemptsPreview(sceneReplayPreview.attempts);
            }
            else
            {
                ShowAttemptsPreview(_prevAttempts);
                // note: VERY performance intensive
                ShowAttemptPreview(_currAttempt.ToArray(), _prevAttempts.Count);
            }
        }

        /// <summary>
        /// Draw gizmos preview of a single attempt with an index into the preview color cycle.
        /// </summary>
        /// <param name="attempt">Attempt to draw</param>
        /// <param name="index">Index into color cycle</param>
        private static void ShowAttemptPreview(ReadOnlySpan<Vector3> attempt, int index)
        {
            if (attempt.Length < 2) return;
            Gizmos.color = PreviewColorCycle[index % PreviewColorCycle.Length];
            Gizmos.DrawLineStrip(attempt, false);
        }

        /// <summary>
        /// Draw gizmos preview of a list of attempts.
        /// </summary>
        /// <param name="attempts">List or array of attempts</param>
        private static void ShowAttemptsPreview(IList<SceneReplay.Attempt> attempts)
        {
            for (int i = 0; i < attempts.Count; i++)
            {
                ShowAttemptPreview(attempts[i].locations, i);
            }
        }

        /// <summary>
        /// Called on reset: saves the attempt.
        /// </summary>
        private void OnReset()
        {
            AddCurrentAttempt();
        }

        /// <summary>
        /// Called on scene load: writes out the previous scene data to a file (if there were attempts/data).
        /// </summary>
        /// <param name="scene">New scene</param>
        /// <param name="loadSceneMode">Scene load mode</param>
        private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
        {
            AddCurrentAttempt();
            WriteOutSceneReplay();
            _activeSceneName = scene.name;
            _currAttempt.Clear();
            _prevAttempts.Clear();
            _player = FindObjectOfType<PlayerController>();
        }

        /// <summary>
        /// Adds the current attempt to the list of previous attempts.
        /// </summary>
        private void AddCurrentAttempt()
        {
            if (_currAttempt.Count > 0)
                _prevAttempts.Add(new SceneReplay.Attempt { locations = _currAttempt.ToArray() });
            _currAttempt.Clear();
        }

        /// <summary>
        /// Writes out the scene replay.
        /// </summary>
        /// <remarks>
        /// If we're not in the editor, it doesn't do anything.
        /// </remarks>
        private void WriteOutSceneReplay()
        {
            if (_currAttempt.Count == 0 && _prevAttempts.Count == 0) return;
            SceneReplay sceneReplay = ScriptableObject.CreateInstance<SceneReplay>();
            sceneReplay.sceneName = _activeSceneName;
            sceneReplay.attempts = _prevAttempts.ToArray();
            string filePath =
                $"Assets/Editor/Replays/Replay-{_activeSceneName}-{DateTime.Now.ToString("s").Replace(':', '-')}.asset";
#if UNITY_EDITOR
            if (!AssetDatabase.IsValidFolder("Assets/Editor/Replays"))
                AssetDatabase.CreateFolder("Assets/Editor", "Replays");
            AssetDatabase.CreateAsset(sceneReplay, filePath);
            AssetDatabase.SaveAssets();
            Debug.Log($"Saved replay to {filePath}");
#endif
        }
    }
}
