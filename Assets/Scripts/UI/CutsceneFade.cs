using UnityEngine;
using UnityEngine.UI;
using Yarn.Unity;

namespace UI
{
    [RequireComponent(typeof(Image))]
    public class CutsceneFade : MonoBehaviour
    {
        private static Image _image;
        private static float _opacity;
        private static float _fadeSpeed;

        private void Awake()
        {
            _image = GetComponent<Image>();
        }

        private void OnEnable()
        {
            // reset opacity to 0 at start of new scene
            _opacity = 0;
        }

        private void LateUpdate()
        {
            // apply fade
            float newAlpha = Mathf.MoveTowards(_image.color.a, _opacity, Time.unscaledDeltaTime * _fadeSpeed);
            _image.color = new Color(_image.color.r, _image.color.g, _image.color.b, newAlpha);
        }

        [YarnCommand("fade_out")]
        public static void FadeOut(float fadeSpeed = 5)
        {
            _opacity = 1;
            _fadeSpeed = fadeSpeed;
        }

        [YarnCommand("fade_in")]
        public static void FadeIn(float fadeSpeed = 5)
        {
            _opacity = 0;
            _fadeSpeed = fadeSpeed;
        }
    }
}
