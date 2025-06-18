using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections.Generic;
using Managers;
using Save;

public class LevelSelectMenu : MonoBehaviour
{
    [SerializeField] GameObject buttonPrefab;
    [SerializeField] Transform buttonContainer;
    [SerializeField] Button playButton;

    [SerializeField] Sprite unlockedSprite;
    [SerializeField] Sprite lockedSprite;

    [SerializeField] List<string> allLevelSceneNames;
    
    [SerializeField] private AudioSource canvasAudioSource;
    [SerializeField] private AudioClip buttonClickSound;
    [SerializeField] private string level1 = "IntroCutscene";
    
    private List<string> unlockedScenes;
    private string selectedScene;
    private List<Button> levelButtons = new List<Button>();

    private void Start()
    {
        LazyLoad();
        unlockedScenes = GameManager.Instance.LevelsAccessed;
        playButton.interactable = false;
        unlockedScenes.Add(level1);

        foreach (string sceneName in allLevelSceneNames)
        {
            GameObject btnObj = Instantiate(buttonPrefab, buttonContainer);
            Button button = btnObj.GetComponent<Button>();
            TMP_Text label = btnObj.GetComponentInChildren<TMP_Text>();
            Image background = btnObj.GetComponent<Image>();

            bool isUnlocked = unlockedScenes.Contains(sceneName);
            int levelNumber = allLevelSceneNames.IndexOf(sceneName) + 1;
            
            Debug.Log(sceneName + " is " + isUnlocked);

            if (isUnlocked)
            {
                label.text = levelNumber.ToString();
                background.sprite = unlockedSprite;
                button.interactable = true;

                button.onClick.AddListener(() =>
                {
                    PlayClickSound();
                    OnLevelSelected(sceneName, button);
                });            }
            else
            {
                label.text = "";
                background.sprite = lockedSprite;
                button.interactable = false;
            }

            levelButtons.Add(button);
        }

        playButton.onClick.AddListener(OnPlayClicked);
    }
    
    private void PlayClickSound()
    {
        if (canvasAudioSource != null && buttonClickSound != null)
        {
            canvasAudioSource.PlayOneShot(buttonClickSound);
        }
    }

    // <summary>
    // Ensures GameManager load save data before loading level select menu
    // </summary>
    private void LazyLoad()
    {
        FindObjectOfType<SaveDataManager>()?.PreloadSaveData();
    }

    private void OnLevelSelected(string sceneName, Button clickedButton)
    {
        selectedScene = sceneName;
        playButton.interactable = true;

        for (int i = 0; i < levelButtons.Count; i++)
        {
            Image bg = levelButtons[i].GetComponent<Image>();
            string thisScene = allLevelSceneNames[i];
            bool isUnlocked = unlockedScenes.Contains(thisScene);

            bg.sprite = isUnlocked ? unlockedSprite : lockedSprite;
            
            Color color = bg.color;
            color.a = 1f;
            bg.color = color;
        }
        Image selectedImage = clickedButton.GetComponent<Image>();
        Color selectedColor = selectedImage.color;
        selectedColor.a = 0.5f; 
        selectedImage.color = selectedColor;
    }

    private void OnPlayClicked()
    {
        if (!string.IsNullOrEmpty(selectedScene))
        {
            Debug.Log("Loading selected level: " + selectedScene);
            SceneManager.LoadScene(selectedScene);
        }
    }
}
