using System.Collections;
using UnityEngine;

namespace Interactables
{
    public class CollapsingPlatform : MonoBehaviour
    {
        #region Serialized Public Fields
        [Header("Platform Properties")] 
        [SerializeField] float collapsePlatTimer;
        [SerializeField] float restorePlatTimer;
        [SerializeField] Rigidbody2D rb;
        #endregion

        #region Private Properties
        private Vector2 _originalPosition;
        private IEnumerator _activeRoutine;
        #endregion


        private void OnCollisionEnter2D(Collision2D other)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                StartCoroutine(_activeRoutine = Falling());    
            }
        }

        IEnumerator Falling() 
        {
            //remember the platform's original position
            _originalPosition = transform.position;

            //wait for timer then drop platform by applying gravity
            yield return new WaitForSeconds(collapsePlatTimer);
            rb.bodyType = RigidbodyType2D.Dynamic;

            //put platfrom back into place and freeze it
            yield return new WaitForSeconds(restorePlatTimer);
            rb.bodyType = RigidbodyType2D.Static;
            transform.position = _originalPosition;
            _activeRoutine = null;
        }

    }
}
