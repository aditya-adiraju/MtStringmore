using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Yarn.Unity;

public class CutsceneManager : MonoBehaviour
{
    [SerializeField] private string nextScene;
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K)) NextScene();
    }

    [YarnCommand("next_scene")]
    public void NextScene()
    {
        if (nextScene != "") SceneManager.LoadScene(nextScene);
    }
}
