using System.Collections;
using UnityEngine;

/// <summary>
/// Causes the GameObject to fade out and deactivate over time when InvokeFadeOut() is called.
///
/// If <see cref="destroyOnFadeOut"/> is true, also destroys the object.
/// </summary>
[RequireComponent(typeof(Renderer))]
public class FadeOut : MonoBehaviour
{
    [SerializeField, Min(0), Tooltip("Duration of fade-out effect (seconds)")]
    private float fadeDuration = 0.3f;

    [SerializeField, Tooltip("Whether the fade-out is destroyed after animation")]
    private bool destroyOnFadeOut;

    private Renderer _renderer;

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
    }

    public void InvokeFadeOut()
    {
        StartCoroutine(FadeOutCoroutine());
    }

    private IEnumerator FadeOutCoroutine()
    {
        Material material = _renderer.material;
        Color color = material.color;
        float startAlpha = color.a;
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Lerp(startAlpha, 0, elapsedTime / fadeDuration);
            material.color = color;
            yield return null;
        }

        color.a = 0;
        material.color = color;
        gameObject.SetActive(false);
        if (destroyOnFadeOut) Destroy(gameObject);
    }
}
