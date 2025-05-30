using UnityEngine;

namespace Parallax
{
    /// <summary>
    /// Controls parallax background layer to move with respect to camera movement
    /// </summary>
    [DisallowMultipleComponent]
    public class ParallaxLayer : MonoBehaviour
    {
        [SerializeField] private float xParallaxFactor;
        [SerializeField] private float yParallaxFactor = 0.99f;
        [SerializeField, Min(0)] private float fallbackSpriteBounds = 10;
        private const float SmoothTime = 0.01f;
        private float _bgWidth;
        private Vector3 _velocity;

        private void Awake()
        {
            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
            _bgWidth = spriteRenderer ? spriteRenderer.bounds.size.x : fallbackSpriteBounds;
        }

        private void OnDrawGizmosSelected()
        {
            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
            _bgWidth = spriteRenderer ? spriteRenderer.bounds.size.x : fallbackSpriteBounds;
            Vector3 size = new(_bgWidth, spriteRenderer ? spriteRenderer.bounds.size.y : 10, 1);
            Gizmos.DrawWireCube(transform.position, transform.TransformVector(size));
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
            Vector3 pos = transform.position;
            if (pos.x + _bgWidth / 2f <= camPos.x + screenWidth / 2f)
            {
                pos.x += _bgWidth / 3f;
            }
            else if (pos.x - _bgWidth / 2f >= camPos.x - screenWidth / 2f)
            {
                pos.x -= _bgWidth / 3f;
            }

            transform.position = pos;
        }
    }
}
