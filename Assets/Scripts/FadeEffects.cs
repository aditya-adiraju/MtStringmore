using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

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
    
    private Material _material;

    private void Awake()
    {
        if (TryGetComponent(out Image image))
        {
            // please don't overwrite literally *every UI object's* material
            // I've had to restart Unity so many times because of this
            image.material = new Material(image.material);
            _material = image.material;
        }
        else if (TryGetComponent(out Renderer componentRenderer))
        {
            _material = componentRenderer.material;
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
        Color color = _material.color;
        float startAlpha = color.a;
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Lerp(startAlpha, 0, elapsedTime / fadeDuration);
            _material.color = color;
            yield return null;
        }

        color.a = 0;
        _material.color = color;
        if (deactivateOnFade)
            gameObject.SetActive(false);
        if (destroyOnFade)
            Destroy(gameObject);
    }

    private IEnumerator FadeInCoroutine()
    {
        Color color = _material.color;
        float startAlpha = color.a;
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Lerp(startAlpha, 1, elapsedTime / fadeDuration);
            _material.color = color;
            yield return null;
        }

        color.a = 1;
        _material.color = color;
    }

    private IEnumerator FadeInAndOutCoroutine()
    {
        yield return FadeInCoroutine();
        FadeIn?.Invoke();
        yield return FadeOutCoroutine();
    }
}
