using JetBrains.Annotations;
using UnityEngine;

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
    }
}
