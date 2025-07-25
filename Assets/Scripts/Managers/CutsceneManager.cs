using System.Collections;
using System.Threading.Tasks;
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

        private void Awake()
        {
            _source = GetComponent<AudioSource>();
            _source.Play();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.K)) NextScene();
        }

        [YarnCommand("next_scene")]
        public void NextScene()
        {
            if (nextScene != "")
            {
                SceneManager.LoadScene(nextScene);
            }
            else
            {
                DialogueRunner dialogueRunner = FindObjectOfType<DialogueRunner>();
                dialogueRunner.Stop();
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
