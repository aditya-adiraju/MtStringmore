using Managers;
using UnityEngine;
using Util;

namespace Interactables
{
    /// <summary>
    /// This object is collected upon collision with the Player, incrementing the total collectable count.
    /// </summary>
    [RequireComponent(typeof(Collider2D), typeof(SpriteRenderer))]
    public class Collectable : MonoBehaviour
    {
        [SerializeField] private Sprite[] possibleSprites;
        
        private SpriteRenderer _spriteRenderer;
        private Collider2D _collider;
        
        private void Awake()
        {
            _collider = GetComponent<Collider2D>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void OnValidate()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                GameManager.Instance.CollectCollectable(this);
                // TODO: play a visual/particle effect before destroying
                SoundManager.Instance.PlayCollectableComboSound();
                _spriteRenderer.enabled = false;
                _collider.enabled = false;
            }
        }

        /// <summary>
        /// Chooses a random sprite from the possible sprites of this collectable.
        /// Should only be run in editor.
        /// </summary>
        public void RandomizeSprite()
        {
            _spriteRenderer.sprite = RandomUtil.SelectRandom(possibleSprites);
        }

        /// <summary>
        /// Rotates this collectable's transform to a random angle.
        /// Should only be run in editor.
        /// </summary>
        public void RandomizeRotation()
        {
            transform.eulerAngles = new Vector3(0, 0, Random.Range(0, 360));
        }
    }
}
