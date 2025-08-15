using System.Collections;
using Level3;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using Util;
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
                        level3Logic.SpecialSkipLogic();
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

        /// <summary>
        /// Fades out the currently playing sound.
        /// </summary>
        /// <param name="fadeDuration">Fade duration, seconds</param>
        /// <returns>Coroutine</returns>
        [YarnCommand("stop_sound")]
        public static IEnumerator StopSound(float fadeDuration = 0) => _source.FadeOut(fadeDuration);
    }
}
