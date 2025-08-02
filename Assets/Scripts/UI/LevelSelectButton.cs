using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// UI renderer for the level select button.
    /// </summary>
    [DisallowMultipleComponent, RequireComponent(typeof(Button), typeof(Image), typeof(AudioSource))]
    public class LevelSelectButton : MonoBehaviour
    {
        [SerializeField] private Sprite unlockedSprite;
        [SerializeField] private Sprite lockedSprite;

        /// <summary>
        /// Button's level number.
        /// </summary>
        public int LevelNumber { get; private set; }

        private TextMeshProUGUI _text;
        private Image _image;
        private AudioSource _audio;
        private Button _button;
        private LevelSelectMenu _levelSelectMenu;

        private void Awake()
        {
            _levelSelectMenu = GetComponentInParent<LevelSelectMenu>();
            _text = GetComponentInChildren<TextMeshProUGUI>();
            _image = GetComponent<Image>();
            _button = GetComponent<Button>();
            _audio = GetComponent<AudioSource>();
        }

        /// <summary>
        /// Initializes the renderer.
        /// </summary>
        /// <param name="levelNumber">Level number to display</param>
        /// <param name="unlocked">Whether the button is unlocked</param>
        public void Initialize(int levelNumber, bool unlocked)
        {
            _button.interactable = unlocked;
            _text.text = unlocked ? levelNumber.ToString() : string.Empty;
            _image.sprite = unlocked ? unlockedSprite : lockedSprite;
            LevelNumber = levelNumber;
        }

        /// <summary>
        /// Marks the button as selected.
        /// </summary>
        public void MarkSelected()
        {
            SetAlpha(0.75f);
        }

        /// <summary>
        /// Marks the button as unselected.
        /// </summary>
        public void MarkUnselected()
        {
            SetAlpha(1);
        }

        /// <summary>
        /// Called on button click.
        /// </summary>
        public void OnClick()
        {
            _audio.Play();
            _levelSelectMenu.OnLevelSelected(LevelNumber, this);
        }

        /// <summary>
        /// Sets the alpha of the image.
        /// </summary>
        /// <param name="alpha">New alpha</param>
        private void SetAlpha(float alpha)
        {
            Color color = _image.color;
            color.a = alpha;
            _image.color = color;
        }
    }
}
