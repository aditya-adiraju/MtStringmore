using System.Collections;
using UnityEngine;

/// <summary>
/// Choose letter block color, handle shake and break animation
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class LetterBlock : MonoBehaviour
{
    [SerializeField] private GameObject letter;
    [SerializeField] private GameObject particles;
    [SerializeField] [Min(0)] private float blockBreakDelay;
    [SerializeField] [Range(0f, 0.1f)] private float delayBetweenShakes = 0f;
    [SerializeField] [Range(0f, 2f)] private float distance = 0.1f;

    private SpriteRenderer _renderer;

    private void Awake()
    {
        _renderer = GetComponent<SpriteRenderer>();
    }

    public void Crack()
    {
        particles.SetActive(true);
        StartCoroutine(Shake());
        StartCoroutine(Break());
    }
    
    private IEnumerator Shake()
    {
        Vector3 startPos = transform.position;
        
        for (float timer = 0; timer < blockBreakDelay; timer += Time.deltaTime)
        {
            transform.position = startPos + (Random.insideUnitSphere * distance);

            if (delayBetweenShakes > 0f)
            {
                yield return new WaitForSeconds(delayBetweenShakes);
            }
            else
            {
                yield return null;
            }
        }

        transform.position = startPos;
    }

    private IEnumerator Break()
    {
        // Hide block and letter after half the block break time
        yield return new WaitForSeconds(blockBreakDelay / 2);
        _renderer.enabled = false;
        letter.SetActive(false);
        // Give time for particles to spawn, then destroy object and children
        Destroy(gameObject, blockBreakDelay / 2);
    }
}
