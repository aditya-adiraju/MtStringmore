using Managers;
using TMPro;
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
        [SerializeField] private Toggle timerToggle;
        [SerializeField] private TextMeshProUGUI versionNumber;

        // Start is called before the first frame update
        private void Start()
        {
            var savedMasterVolume = PlayerPrefs.GetFloat("Master");
            var savedBgmVolume = PlayerPrefs.GetFloat("BGM");
            var savedSfxVolume = PlayerPrefs.GetFloat("SFX");
            int savedSpeedToggle = PlayerPrefs.GetInt("SpeedTime");

            masterSlider.value = savedMasterVolume;
            bgmSlider.value = savedBgmVolume;
            sfxSlider.value = savedSfxVolume;
            timerToggle.isOn = savedSpeedToggle == 1;

            masterSlider.onValueChanged.AddListener(delegate
            {
                SoundManager.Instance.SetMasterVolume(masterSlider.value);
                audioMixer.SetFloat("Master", SoundManager.SliderToVolume(masterSlider.value));
            });
            bgmSlider.onValueChanged.AddListener(delegate { SoundManager.Instance.SetBgmVolume(bgmSlider.value); });
            sfxSlider.onValueChanged.AddListener(delegate { SoundManager.Instance.SetSfxVolume(sfxSlider.value); });
            timerToggle.onValueChanged.AddListener(delegate { TimerManager.Instance.ToggleTimer(timerToggle.isOn); });
   
            versionNumber.text = Application.version;
        }
       
    }
}
