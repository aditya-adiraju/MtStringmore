using Managers;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace UI
{
    public class OptionsMenu : MonoBehaviour
    {
        [SerializeField] private AudioMixer audioMixer;
        [SerializeField] private Slider masterSlider;
        [SerializeField] private Slider bgmSlider;
        [SerializeField] private Slider sfxSlider;

        // Start is called before the first frame update
        private void Start()
        {
            var savedMasterVolume = PlayerPrefs.GetFloat("Master");
            var savedBgmVolume = PlayerPrefs.GetFloat("BGM");
            var savedSfxVolume = PlayerPrefs.GetFloat("SFX");
            masterSlider.value = savedMasterVolume;
            bgmSlider.value = savedBgmVolume;
            sfxSlider.value = savedSfxVolume;

            masterSlider.onValueChanged.AddListener(delegate
            {
                SoundManager.Instance.SetMasterVolume(masterSlider.value);
                audioMixer.SetFloat("Master", SoundManager.SliderToVolume(masterSlider.value));
            });
            bgmSlider.onValueChanged.AddListener(delegate { SoundManager.Instance.SetBgmVolume(bgmSlider.value); });
            sfxSlider.onValueChanged.AddListener(delegate { SoundManager.Instance.SetSfxVolume(sfxSlider.value); });
        }
    }
}
