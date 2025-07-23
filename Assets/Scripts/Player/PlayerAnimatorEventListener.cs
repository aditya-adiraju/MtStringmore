using JetBrains.Annotations;
using UnityEngine;
using Util;

namespace Player
{
    /// <summary>
    /// Player sprite animator event listener.
    ///
    /// Listens to events (e.g. walk PlaySound events).
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class PlayerAnimatorEventListener : MonoBehaviour
    {
        private AudioSource _audioSource;
        /// <summary>
        /// Audio clips that can be played by an animator event.
        /// </summary>
        [SerializeField] private AudioClip[] audioClips;

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
        }

        /// <summary>
        /// Plays a specific audio clip.
        /// </summary>
        /// <param name="clip">Audio clip</param>
        /// <remarks>
        /// Called implicitly by an animator event.
        /// </remarks>
        [UsedImplicitly]
        public void PlaySound(AudioClip clip)
        {
            _audioSource.clip = clip;
            _audioSource.Play();
        }

        /// <summary>
        /// Plays a random audio clip from a list configured in <see cref="audioClips"/>.
        /// </summary>
        /// <remarks>
        /// Called implicitly by an animator event.
        /// </remarks>
        [UsedImplicitly]
        public void PlayRandomSound()
        {
            if (audioClips.Length == 0)
                Debug.LogError("No audio clips to select from.");
            _audioSource.clip = RandomUtil.SelectRandom(audioClips);
            _audioSource.Play();
        }
    }
}
