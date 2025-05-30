using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// Applies fade effects to a game object
    /// </summary>
    public class FadeEffects : MonoBehaviour
    {
        [SerializeField, Min(0)] private float fadeDuration = 0.3f;

        [Tooltip("Delay between fading in and fading out, if applicable")] [SerializeField]
        private bool deactivateOnFade;

        [SerializeField] private bool destroyOnFade;
        public Action FadeIn;

        private IFadeEffectHandler _fadeEffectHandler;

        private void Awake()
        {
            _fadeEffectHandler = GetFadeEffectHandler(transform);
            if (_fadeEffectHandler == null)
            {
                Debug.LogWarning("Fade effect handler not found");
            }
        }

        public void InvokeFadeOut()
        {
            StartCoroutine(FadeOutCoroutine());
        }

        public void InvokeFadeIn()
        {
            StartCoroutine(FadeInCoroutine());
        }

        public void InvokeFadeInAndOut()
        {
            StartCoroutine(FadeInAndOutCoroutine());
        }

        private IEnumerator FadeOutCoroutine()
        {
            for (float elapsedTime = 0; elapsedTime < fadeDuration; elapsedTime += Time.deltaTime)
            {
                _fadeEffectHandler?.SetAlpha(1.0f - elapsedTime / fadeDuration);
                yield return null;
            }

            _fadeEffectHandler?.SetAlpha(0);
            if (deactivateOnFade)
                gameObject.SetActive(false);
            if (destroyOnFade)
                Destroy(gameObject);
        }

        private IEnumerator FadeInCoroutine()
        {
            for (float elapsedTime = 0; elapsedTime < fadeDuration; elapsedTime += Time.deltaTime)
            {
                _fadeEffectHandler?.SetAlpha(elapsedTime / fadeDuration);
                yield return null;
            }

            _fadeEffectHandler?.SetAlpha(1);
        }

        private IEnumerator FadeInAndOutCoroutine()
        {
            yield return FadeInCoroutine();
            FadeIn?.Invoke();
            yield return FadeOutCoroutine();
        }

        /// <summary>
        /// Get the fade effect handler of an object.
        /// </summary>
        /// <param name="go">GameObject to get fade effect of</param>
        /// <param name="startAlpha">Starting alpha, -1 if unspecified</param>
        /// <returns>Fade effect handler</returns>
        public static IFadeEffectHandler GetFadeEffectHandler(Transform go, float startAlpha = -1)
        {
            if (go.TryGetComponent(out Image image))
            {
                return new ImageFadeHandler(image, startAlpha);
            }

            if (go.TryGetComponent(out SpriteRenderer spriteRenderer))
            {
                return new SpriteFadeHandler(spriteRenderer, startAlpha);
            }

            if (go.TryGetComponent(out Renderer componentRenderer))
            {
                return new FallbackEffectHandler(componentRenderer, startAlpha);
            }

            return null;
        }

        /// <summary>
        /// Handles changing the alpha of sprite renderers.
        /// </summary>
        private class SpriteFadeHandler : IFadeEffectHandler
        {
            private readonly SpriteRenderer _spriteRenderer;
            private readonly float _startAlpha;

            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="spriteRenderer">Sprite renderer to change alpha of</param>
            /// <param name="startAlpha">Starting alpha: -1 if unspecified</param>
            public SpriteFadeHandler(SpriteRenderer spriteRenderer, float startAlpha = -1)
            {
                _spriteRenderer = spriteRenderer;
                _startAlpha = startAlpha < 0 ? _spriteRenderer.color.a : startAlpha;
            }

            public void SetAlpha(float alpha)
            {
                _spriteRenderer.color = IFadeEffectHandler.CreateColor(_spriteRenderer.color, _startAlpha * alpha);
            }
        }

        /// <summary>
        /// Handler to fade out a UI image.
        /// </summary>
        private class ImageFadeHandler : IFadeEffectHandler
        {
            private readonly Image _image;
            private readonly float _startAlpha;

            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="image">Image to set the alpha of</param>
            /// <param name="startAlpha">Starting alpha: -1 if unspecified</param>
            public ImageFadeHandler(Image image, float startAlpha = -1)
            {
                _image = image;
                _startAlpha = startAlpha < 0 ? _image.color.a : startAlpha;
            }

            /// <inheritdoc />
            public void SetAlpha(float alpha)
            {
                _image.color = IFadeEffectHandler.CreateColor(_image.color, alpha * _startAlpha);
            }
        }

        /// <summary>
        /// Fallback handler that creates a new material and modifies the alpha.
        /// <p/>
        /// Ideally shouldn't be used as the creation of materials prevents draw call batching.
        /// </summary>
        private class FallbackEffectHandler : IFadeEffectHandler
        {
            private readonly Material _material;
            private readonly float _startAlpha;

            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="renderer">Renderer to get material of</param>
            /// <param name="startAlpha">Starting alpha: -1 if unspecified</param>
            public FallbackEffectHandler(Renderer renderer, float startAlpha = -1)
            {
                _material = renderer.material;
                _startAlpha = startAlpha < 0 ? _material.color.a : startAlpha;
            }

            /// <inheritdoc />
            public void SetAlpha(float alpha)
            {
                _material.color = IFadeEffectHandler.CreateColor(_material.color, alpha * _startAlpha);
            }
        }

        /// <summary>
        /// Interface to handle fading objects.
        /// </summary>
        public interface IFadeEffectHandler
        {
            /// <summary>
            /// Creates a color with a specified alpha.
            /// </summary>
            /// <param name="c">Color</param>
            /// <param name="alpha">Specific alpha</param>
            /// <returns>New color with specific alpha</returns>
            protected static Color CreateColor(Color c, float alpha)
            {
                return new Color(c.r, c.g, c.b, alpha);
            }

            /// <summary>
            /// Sets the alpha of the object.
            /// </summary>
            /// <param name="alpha">Object's alpha</param>
            void SetAlpha(float alpha);
        }
    }
}
