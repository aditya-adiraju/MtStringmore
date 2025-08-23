using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Interactables.Balloon
{
    /// <summary>
    /// Balloon flying visual that collides with walls.
    /// </summary>
    [DisallowMultipleComponent, RequireComponent(typeof(Rigidbody2D))]
    public class BalloonFunnyVisual : MonoBehaviour
    {
        [SerializeField, Min(0)] private float verticalAcceleration = 120;
        [SerializeField, Min(0)] private float maxAngularVelocity = 350;
        [SerializeField, Min(0)] private float timeToMaxAngularVelocity = 0.4f;
        [SerializeField, Min(0)] private float timeToDeath = 1;
        private Balloon _parent;
        private Rigidbody2D _rigidbody;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
            _parent = GetComponentInParent<Balloon>();
        }

        private void OnEnable()
        {
            _rigidbody.position = _parent.transform.position;
            _rigidbody.rotation = 0;
            transform.position = _parent.transform.position;
            StartCoroutine(MovementCoroutine());
        }

        /// <summary>
        /// Routine that applies constant acceleration and angular velocity.
        /// </summary>
        /// <returns>Motion coroutine</returns>
        private IEnumerator MovementCoroutine()
        {
            float target = (Random.Range(0, 2) * 2 - 1) * maxAngularVelocity;
            Vector2 accel = new(0, verticalAcceleration);
            for (float time = 0; time < timeToDeath; time += Time.fixedDeltaTime)
            {
                _rigidbody.AddRelativeForce(accel);
                _rigidbody.angularVelocity = Mathf.Lerp(0, target, time / timeToMaxAngularVelocity);
                yield return new WaitForFixedUpdate();
            }
            gameObject.SetActive(false);
            _parent.OnEndingVisualFinish();
        }
    }
}
