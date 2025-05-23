using UnityEngine;

namespace Interactables
{
    /// <summary>
    /// Handles the physics and features behind the boulders 
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(AudioSource))]
    public class BoulderController : MonoBehaviour
    {
        [SerializeField, Tooltip("ParticleSystem prefab to instantiate")]
        private ParticleSystem liquidMolecule;

        [SerializeField, Tooltip("Whether the projectile explodes")]
        private bool toExplode;

        [Range(0, 5)] [SerializeField] private float minGravityScale = 3f;
        [Range(0, 5)] [SerializeField] private float maxGravityScale = 5f;

        private Rigidbody2D _rb;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            float randomGravityScale = Random.Range(minGravityScale, maxGravityScale);
            _rb.gravityScale = randomGravityScale;
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.layer == LayerMask.NameToLayer("Terrain"))
            {
                if (toExplode)
                {
                    TurnIntoParticles();
                }

                Destroy(gameObject);
            }
        }

        private void TurnIntoParticles()
        {
            if (liquidMolecule != null)
            {
                var yOffset = 0.5f;

                // Set the particle system position to just above the boulder, so the animation looks better
                var particlePosition = transform.position + new Vector3(0, yOffset, 0);

                // physically move the particles to the newly created position
                var particles = Instantiate(liquidMolecule, particlePosition, Quaternion.identity);

                particles.Play();
                Destroy(particles.gameObject, particles.main.duration + particles.main.startLifetime.constantMax);
            }
        }
    }
}
