using UnityEngine;
using System.Collections;

public class FadeOut : MonoBehaviour
{
    public float fadeDuration = 0.3f; // Duration of the fade-out effect

    public void InvokeFadeOut()
    {
        StartCoroutine(FadeOutCoroutine());
    }

    private IEnumerator FadeOutCoroutine()
    {
        var material = GetComponent<Renderer>().material;
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
    }
}
