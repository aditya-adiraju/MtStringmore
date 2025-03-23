using UnityEngine;

/// <summary>
/// Handles the death of player, when in contact with particles 
/// </summary>
public class ExplosionParticleController : MonoBehaviour
{
    [Range(0, 2)]
    [SerializeField] private float emissionRadius = 1.0f;
    [SerializeField] private ParticleSystem explosionParticles;

    private void Start()
    {
        if (explosionParticles == null)
        {
            explosionParticles = GetComponent<ParticleSystem>();
        }

        if (explosionParticles != null)
        {
            var shapeModule = explosionParticles.shape;
            shapeModule.radius = emissionRadius;

            var collisionModule = explosionParticles.collision;
            collisionModule.enabled = true;
            collisionModule.collidesWith = LayerMask.GetMask("Player", "Terrain");
        }
    }
}