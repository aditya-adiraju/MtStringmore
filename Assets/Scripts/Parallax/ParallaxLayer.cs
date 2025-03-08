using UnityEngine;

namespace Parallax
{
    /// <summary>
    /// Controls parallax background layer to move with respect to camera movement
    /// </summary>
    [DisallowMultipleComponent, RequireComponent(typeof(SpriteRenderer))]
    public class ParallaxLayer : MonoBehaviour
    {
        [SerializeField] private float xParallaxFactor;
        [SerializeField] private float yParallaxFactor = 0.99f;
        private const float SmoothTime = 0.01f;
        private SpriteRenderer _spriteRenderer;
        private Vector3 _velocity;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        /// <summary>
        /// Moves the parallax layer by a given delta after applying a parallax factor.
        /// </summary>
        /// <param name="delta">Change in camera position</param>
        public void Move(Vector2 delta)
        {
            Vector3 newPos = transform.position;
            newPos.x -= delta.x * xParallaxFactor;
            newPos.y -= delta.y * yParallaxFactor;

            transform.position = Vector3.SmoothDamp(transform.position, newPos, ref _velocity, SmoothTime);
        }

        /// <summary>
        /// Moves a background tiled to repeat 3x to the left or right as the player reaches the edge,
        /// to create the illusion of a seamlessly repeating background.
        /// Background MUST be tiled to repeat 3x.
        /// </summary>
        /// <param name="camPos">Camera position</param>
        /// <param name="screenWidth">Orthographic camera view width</param>
        public void Reposition(Vector2 camPos, float screenWidth)
        {
            float bgWidth = _spriteRenderer.bounds.size.x;

            Vector3 pos = transform.position;
            if (pos.x + bgWidth / 2f <= camPos.x + screenWidth / 2f)
            {
                pos.x += bgWidth / 3f;
            }
            else if (pos.x - bgWidth / 2f >= camPos.x - screenWidth / 2f)
            {
                pos.x -= bgWidth / 3f;
            }

            transform.position = pos;
        }
    }
}
