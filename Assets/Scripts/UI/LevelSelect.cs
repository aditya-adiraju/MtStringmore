using System.Collections.Generic;
using Managers;
using Save;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UI
{
    public class LevelSelectMenu : MonoBehaviour
    {
        [SerializeField] GameObject buttonPrefab;
        [SerializeField] Transform buttonContainer;
        [SerializeField] Button playButton;

        [SerializeField] Sprite unlockedSprite;
        [SerializeField] Sprite lockedSprite;

        [SerializeField] private List<Sprite> levelCandyImages;

        //<summary>
        //The names of the scenes that need to be loaded (i.e. introcutscene, secondcutscene, etc.)
        //</summary>
        [SerializeField] List<string> allLevelLoadingSceneNames;
        
        //<summary>
        //The names of the level scenes (i.e. Level1Trial, Level2, etc.)
        //</summary>
        [SerializeField] private List<string> allLevelSceneNames;
    
        [SerializeField] private AudioSource canvasAudioSource;
        [SerializeField] private AudioClip buttonClickSound;
        [SerializeField] private string level1 = "IntroCutscene";

        [SerializeField] private TextMeshProUGUI levelText, timeText, deathText, candyText;
        [SerializeField] private Image candyImage;
    
        private List<string> unlockedScenes;
        private string selectedScene;
        private readonly List<Button> levelButtons = new List<Button>();
        private GameManager _gameManager;

        private void Awake()
        {
            _gameManager = GameManager.Instance;
        }

        private void Start()
        {
            LazyLoad();
            unlockedScenes = _gameManager.LevelsAccessed;
            playButton.interactable = false;
            unlockedScenes.Add(level1);

            foreach (string sceneName in allLevelLoadingSceneNames)
            {
                GameObject btnObj = Instantiate(buttonPrefab, buttonContainer);
                Button button = btnObj.GetComponent<Button>();
                TMP_Text label = btnObj.GetComponentInChildren<TMP_Text>();
                Image background = btnObj.GetComponent<Image>();

                bool isUnlocked = unlockedScenes.Contains(sceneName);
                int levelNumber = allLevelLoadingSceneNames.IndexOf(sceneName) + 1;
                
                if (isUnlocked)
                {
                    label.text = levelNumber.ToString();
                    background.sprite = unlockedSprite;
                    button.interactable = true;

                    button.onClick.AddListener(() =>
                    {
                        PlayClickSound();
                        OnLevelSelected(sceneName, button, levelNumber);
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

        private void OnLevelSelected(string sceneName, Button clickedButton, int levelNumber)
        {
            selectedScene = sceneName;
            playButton.interactable = true;
            
            //change the text of the level stats
            levelText.text = "Level " + levelNumber;

            LevelData selectedLevel = _gameManager.allLevelData[levelNumber - 1];
            
            // Candy
            if (selectedLevel.totalCandiesInLevel < 0)
                candyText.text = "N/A";
            else
                candyText.text = selectedLevel.mostCandiesCollected + "/" + selectedLevel.totalCandiesInLevel;

            // Deaths
            Debug.Log(selectedLevel.leastDeaths);
            deathText.text = selectedLevel.leastDeaths == -1 ? "N/A" : selectedLevel.leastDeaths.ToString();

            // Time
            timeText.text = string.IsNullOrEmpty(selectedLevel.bestTime) ? 
                GameManager.EmptySaveTime : selectedLevel.bestTime;
            
            //CandyImage
            candyImage.sprite = levelCandyImages[levelNumber - 1];
            candyImage.enabled = true;
            
            for (int i = 0; i < levelButtons.Count; i++)
            {
                Image bg = levelButtons[i].GetComponent<Image>();
                string thisScene = allLevelLoadingSceneNames[i];
                bool isUnlocked = unlockedScenes.Contains(thisScene);

                bg.sprite = isUnlocked ? unlockedSprite : lockedSprite;
            
                Color color = bg.color;
                color.a = 1f;
                bg.color = color;
            }
            Image selectedImage = clickedButton.GetComponent<Image>();
            Color selectedColor = selectedImage.color;
            selectedColor.a = 0.75f; 
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
}
