using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Managers
{
    /// <summary>
    /// Background music manager to play scene-specific background music.
    /// </summary>
    /// <remarks>
    /// There's probably a better way (e.g. each scene defines a self-destructing object that tells the BGM what to play)
    /// but this is nice to me.
    /// </remarks>
    [DisallowMultipleComponent, RequireComponent(typeof(AudioSource))]
    public class BGMManager : MonoBehaviour
    {
        [SerializeField] private SceneMusicPairing[] sceneMusicPairings;

        private Dictionary<string, AudioClip> sceneToClip;
        private AudioSource _audioSource;
        private string _currScene;

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            SceneManager.sceneLoaded += OnSceneLoaded;
            InitializeDictionary();
            _currScene = SceneManager.GetActiveScene().name;
            AudioClip nextClip = GetNextAudioClip(_currScene, _audioSource.clip);
            if (_audioSource.clip != GetNextAudioClip(_currScene, _audioSource.clip))
            {
                _audioSource.clip = nextClip;
                _audioSource.Play();
            }
        }

        private void OnValidate()
        {
            InitializeDictionary();
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        /// <summary>
        /// Gets the next background music clip to play.
        /// </summary>
        /// <param name="sceneName">Next scene name</param>
        /// <param name="prevClip">Previous playing clip</param>
        /// <returns>Next clip if present and not null, else the previous</returns>
        private AudioClip GetNextAudioClip(string sceneName, AudioClip prevClip)
        {
            if (sceneToClip.TryGetValue(sceneName, out AudioClip clip))
            {
                return clip ?? prevClip;
            }

            return prevClip;
        }

        /// <summary>
        /// Initializes the scene to audio clip lookup.
        /// </summary>
        private void InitializeDictionary()
        {
            sceneToClip = sceneMusicPairings.ToDictionary(pair => pair.sceneName, pair => pair.audioClip);
        }

        /// <summary>
        /// Runs on scene load: changes the music if needed
        /// </summary>
        /// <param name="nextScene">Next scene</param>
        /// <param name="mode">LoadScene mode</param>
        private void OnSceneLoaded(Scene nextScene, LoadSceneMode mode)
        {
            AudioClip clip = GetNextAudioClip(nextScene.name, _audioSource.clip);
            if (_audioSource.clip != clip)
            {
                _audioSource.clip = clip;
                _audioSource.Play();
            }
        }

        /// <summary>
        /// Pairing of scene name to audio clip.
        /// </summary>
        [Serializable]
        private struct SceneMusicPairing
        {
            public string sceneName;
            public AudioClip audioClip;
        }
    }
}
