using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using Util;

namespace Managers
{
    public class SoundManager : MonoBehaviour
    {
        private static SoundManager _instance;
        public static SoundManager Instance
        {
            get
            {
                if (!_instance)
                    _instance = FindObjectOfType<SoundManager>();

                return _instance;
            }
        }

        [Header("Audio Control")]
        [SerializeField] private AudioMixer audioMixer;
        [SerializeField] private Slider masterSlider;
        [SerializeField] private Slider bgmSlider;
        [SerializeField] private Slider sfxSlider;
        [SerializeField, Range(float.Epsilon, 1)] private float startBgmVolume = 0.5f;
        [SerializeField, Range(float.Epsilon, 1)] private float startSfxVolume = 0.5f;
        [SerializeField, Range(float.Epsilon, 1)] private float startMasterVolume = 0.5f;

        [Header("Collectable SFX")]
        [SerializeField] private AudioClip[] collectableClips;
        [SerializeField, Tooltip("Max time between collections to continue combo (in seconds)")]
        private float collectableComboWindow = 0.6f;
        
        [SerializeField, Tooltip("Max time between max collections to continue combo (in seconds)")]
        private float lastColletableComboWindow = 0.4f;

        private float _timeSinceLastCollect = Mathf.Infinity;
        private int _audioIndex;
        private AudioSource _collectableAudioSource;

        private void Awake()
        {
            if (Instance != this) Destroy(gameObject);

            _collectableAudioSource = gameObject.AddComponent<AudioSource>();
            _collectableAudioSource.outputAudioMixerGroup = audioMixer.FindMatchingGroups("SFX")[0];
        }

        private void Start()
        {
            float savedMasterVolume = PlayerPrefs.GetFloat("Master", startMasterVolume);
            float savedBgmVolume = PlayerPrefs.GetFloat("BGM", startBgmVolume);
            float savedSfxVolume = PlayerPrefs.GetFloat("SFX", startSfxVolume);
            SetMasterVolume(savedMasterVolume);
            SetBgmVolume(savedBgmVolume);
            SetSfxVolume(savedSfxVolume);
        }

        private void Update()
        {
            _timeSinceLastCollect += Time.deltaTime;
        }

        /// <summary> Sets Master volume (0.0001 to 1). </summary>
        public void SetMasterVolume(float volume)
        {
            masterSlider.value = volume;
            PlayerPrefs.SetFloat("Master", volume);
            PlayerPrefs.Save();
        }

        public void SetBgmVolume(float volume)
        {
            audioMixer.SetFloat("BGM", SoundUtil.SliderToVolume(volume));
            bgmSlider.value = volume;
            PlayerPrefs.SetFloat("BGM", volume);
            PlayerPrefs.Save();
        }

        public void SetSfxVolume(float volume)
        {
            audioMixer.SetFloat("SFX", SoundUtil.SliderToVolume(volume));
            sfxSlider.value = volume;
            PlayerPrefs.SetFloat("SFX", volume);
            PlayerPrefs.Save();
        }

        public void SetMute(bool isMuted)
        {
            audioMixer.SetFloat("Master",
                isMuted ? -80f : SoundUtil.SliderToVolume(PlayerPrefs.GetFloat("Master", startMasterVolume)));
        }

        /// <summary>
        /// Resets the time since last collect.
        /// </summary>
        public void ResetTimeSinceLastCollect()
        {
            _timeSinceLastCollect = Mathf.Infinity;
        }

        /// <summary>
        /// Plays a collectable sound in sequence with combo logic using PlayOneShot.
        /// </summary>
        public void PlayCollectableComboSound()
        {
            if (collectableClips == null || collectableClips.Length == 0) return;
            
            if (_timeSinceLastCollect > collectableComboWindow || 
                (_audioIndex == collectableClips.Length - 1 &&
                 _timeSinceLastCollect > lastColletableComboWindow))
            {
                _audioIndex = 0;
            }
            else
            {
                if (_audioIndex < collectableClips.Length - 1)
                    _audioIndex++;
            }
            
            

            _collectableAudioSource.PlayOneShot(collectableClips[_audioIndex]);
            _timeSinceLastCollect = 0f;
        }
    }
}
