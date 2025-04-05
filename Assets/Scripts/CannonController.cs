using System.Collections;
using UnityEngine;

/// <summary>
/// Controls the cannon that shoots the boulders
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class CannonController : MonoBehaviour
{
    [Tooltip("Boulder prefab")] public GameObject boulder;

    [SerializeField, Tooltip("Angle in degrees"), Range(-180, 180)]
    private float angle = 45f;

    [SerializeField] private float speed = 10f;
    [SerializeField] private float minTimeBetweenShots = 1f;
    [SerializeField] private float maxTimeBetweenShots = 3f;

    private AudioSource _audioSource;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
        StartCoroutine(RandomShotCoroutine());
    }

    private IEnumerator RandomShotCoroutine()
    {
        while (true)
        {
            float timeBetweenShots = Random.Range(minTimeBetweenShots, maxTimeBetweenShots);
            yield return new WaitForSeconds(timeBetweenShots);

            GameObject newBoulder = Instantiate(boulder, transform.position, Quaternion.identity);
            Rigidbody2D boulderRb = newBoulder.GetComponent<Rigidbody2D>();

            // create the angle that the boulder shoots out
            float angleInRadians = angle * Mathf.Deg2Rad;
            Vector2 velocity = new Vector2(Mathf.Cos(angleInRadians), Mathf.Sin(angleInRadians)) * speed;
            boulderRb.velocity = velocity;
            _audioSource.Play();
        }
    }
}
