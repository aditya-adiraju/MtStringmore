using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles showing and hiding tutorial.
/// Fades in when a checkpoint flag calls the ShowTutorial function,
/// Fades out 3 seconds after the user presses the key associated with the move.
/// </summary>
public class TutorialBox : MonoBehaviour
{
    private Animator _moveAnim;
    private Animator _keyAnim;
    private SpriteRenderer _moveSprite;
    private SpriteRenderer _keySprite;
    private Coroutine _fadeInCoroutine;
    private Coroutine _fadeOutCoroutine;

    [SerializeField] private GameObject moveDisplay;
    [SerializeField] private GameObject keyDisplay;
    [SerializeField, Min(0)] private float fadeDuration = 0.5f;

    private record TutorialMove(string playerAnimationName, float playerAnimationSpeed, string keyboardAnimationName, float keyboardAnimationSpeed);

    /// <summary>
    /// Dictionary of tutorial moves
    /// </summary>
    private readonly Dictionary<string, TutorialMove> _moves =
        new()
        {
            {"jump", new TutorialMove
                ("Tutorial_Player_Jump", 0.25f, "Tutorial_Key_Space", 0.25f)},
            {"dash", new TutorialMove
                ("Tutorial_Player_Dash", 0.25f, "Tutorial_Key_Space", 0.25f)},
            {"swing", new TutorialMove
                ("Tutorial_Player_Swing", 0.15f, "Tutorial_Key_Space_Hold", 0.15f)}
        };

    private void Start()
    {
        _moveAnim = moveDisplay.GetComponent<Animator>();
        _moveSprite = moveDisplay.GetComponent<SpriteRenderer>();
        _keyAnim = keyDisplay.GetComponent<Animator>();
        _keySprite = keyDisplay.GetComponent<SpriteRenderer>();
    }

    public void ShowTutorial(string move)
    {
        if (!_moves.TryGetValue(move, out TutorialMove tutorialMove))
        {
            Debug.LogError("Invalid tutorial move set " + move);
            return;
        }

        _moveAnim.enabled = true;
        _moveAnim.Play(tutorialMove.playerAnimationName);
        _moveAnim.speed = tutorialMove.playerAnimationSpeed;

        _keyAnim.enabled = true;
        _keyAnim.Play(tutorialMove.keyboardAnimationName);
        _keyAnim.speed = tutorialMove.keyboardAnimationSpeed;

        if (_fadeInCoroutine != null)
            StopCoroutine(_fadeInCoroutine);
        _fadeInCoroutine = StartCoroutine(FadeIn());
    }

    public void HideTutorial()
    {
        if (_fadeOutCoroutine != null)
            StopCoroutine(_fadeOutCoroutine);
        _fadeOutCoroutine = StartCoroutine(FadeOut());
    }

    private IEnumerator FadeIn()
    {
        float elapsedTime = 0f;
        Color moveColor = _moveSprite.color;
        Color keyColor = _keySprite.color;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            moveColor.a = Mathf.Clamp01(elapsedTime / fadeDuration);
            keyColor.a = Mathf.Clamp01(elapsedTime / fadeDuration);
            _moveSprite.color = moveColor;
            _keySprite.color = keyColor;
            yield return null;
        }

        moveColor.a = 1f;
        keyColor.a = 1f;
        _moveSprite.color = moveColor;
        _keySprite.color = keyColor;
    }

    private IEnumerator FadeOut()
    {
        while (!Mathf.Approximately(_moveSprite.color.a, 1f))
            yield return null;

        float elapsedTime = 0f;
        Color moveColor = _moveSprite.color;
        Color keyColor = _keySprite.color;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            moveColor.a = Mathf.Clamp01(1f - elapsedTime / fadeDuration);
            keyColor.a = Mathf.Clamp01(1f - elapsedTime / fadeDuration);
            _moveSprite.color = moveColor;
            _keySprite.color = keyColor;
            yield return null;
        }

        moveColor.a = 0f;
        keyColor.a = 0f;
        _moveSprite.color = moveColor;
        _keySprite.color = keyColor;

        _moveAnim.enabled = false;
        _keyAnim.enabled = false;
    }
}