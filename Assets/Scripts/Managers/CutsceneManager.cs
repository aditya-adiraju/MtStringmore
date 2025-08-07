using System.Collections;
using System.Threading.Tasks;
using Level3;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using Yarn.Unity;

namespace Managers
{
    [RequireComponent(typeof(AudioSource)), DisallowMultipleComponent]
    public class CutsceneManager : MonoBehaviour
    {
        private static AudioSource _source;
        [SerializeField] private string nextScene;
        [SerializeField] private UnityEvent onSceneInterrupt;
        private TimerManager _timerManager;

        private void Awake()
        {
            _source = GetComponent<AudioSource>();
            _source.Play();
            
            _timerManager = FindObjectOfType<TimerManager>();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.K)) NextScene();
        }

        [YarnCommand("next_scene")]
        public void NextScene()
        {
            if (!string.IsNullOrWhiteSpace(nextScene))
            {
                SceneManager.LoadScene(nextScene);
            }
            else
            {
                DialogueRunner dialogueRunner = FindAnyObjectByType<DialogueRunner>();
                dialogueRunner.Stop();
                // not sure there's a way to trigger events in yarn if we're killing it sooo guess we manually trigger
                // all of them
                // this is a very good system
                if (SceneListManager.Instance.LevelNumber == 3)
                {
                    Level3Logic level3Logic = FindAnyObjectByType<Level3Logic>();
                    if (level3Logic)
                    {
                        level3Logic.SetCutsceneState(false);
                        level3Logic.ReachSecondHalf();
                        
                        _timerManager.SetTimerState(true);
                    }
                }
            }
            onSceneInterrupt.Invoke();
        }

        [YarnCommand("play_sound")]
        public static IEnumerator PlaySound(string soundName, bool blockUntilDone = true)
        {
            _source.PlayOneShot(Resources.Load<AudioClip>(soundName));
            if (blockUntilDone)
                yield return new WaitUntil(() => !_source.isPlaying);
        }

        [YarnCommand("stop_sound")]
        public static async void StopSound(float fadeDuration = 0)
        {
            if (fadeDuration == 0)
            {
                _source.Stop();
                return;
            }
        
            float startVolume = _source.volume;
            float elapsedTime = 0f;

            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                _source.volume = Mathf.Lerp(startVolume, 0, elapsedTime / fadeDuration);
                await Task.Yield(); // Non-blocking
            }

            _source.Stop();
            _source.volume = startVolume;
        }
    }
}
