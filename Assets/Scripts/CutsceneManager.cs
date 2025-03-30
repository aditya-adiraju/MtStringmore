
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Yarn.Unity;

[RequireComponent(typeof(AudioSource)), DisallowMultipleComponent]
public class CutsceneManager : MonoBehaviour
{
    private static AudioSource _source;
    [SerializeField] private string nextScene;

    private void Awake()
    {
        _source = GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K)) NextScene();
    }

    [YarnCommand("next_scene")]
    public void NextScene()
    {
        if (nextScene != "") SceneManager.LoadScene(nextScene);
    }

    [YarnCommand("play_sound")]
    public static IEnumerator PlaySound(string soundName, bool blockUntilDone = true)
    {
        _source.PlayOneShot(Resources.Load<AudioClip>(soundName));
        if (blockUntilDone)
            yield return new WaitUntil(() => !_source.isPlaying);
    }
}
