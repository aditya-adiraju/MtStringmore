using System.Collections;
using Managers;
using UnityEngine;

namespace Interactables
{
    [RequireComponent(typeof(HingeJoint2D))]
    public class TrapdoorController : MonoBehaviour
    {
        #region Serialized Public Fields
        [Header("Platform Properties")] 
        [SerializeField] private float collapsePlatTimer;
        [SerializeField] private float removeCollisionTimer;
        [SerializeField] private float restorePlatTimer;
        [SerializeField] private float fixPlatTimer;
        #endregion

        #region Private Properties
        private HingeJoint2D _hinge;
        private Rigidbody2D _rb;
        private BoxCollider2D[] _colliders;
        private float _motorSpeed;
        private Vector3 _initPos;
        private Quaternion _initRot;
    
        private IEnumerator _activeRoutine;
        #endregion

        private void Awake()
        {
            _hinge = GetComponent<HingeJoint2D>();
            _rb = GetComponent<Rigidbody2D>();
            _colliders = GetComponents<BoxCollider2D>();
            _rb.constraints = RigidbodyConstraints2D.FreezeAll;
            _motorSpeed = _hinge.motor.motorSpeed;
            _initPos = transform.position;
            _initRot = transform.rotation;

            GameManager.Instance.Reset += OnReset;
        }
    
        private void OnDestroy() {
            GameManager.Instance.Reset -= OnReset;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.CompareTag("Player") && _activeRoutine == null)
            {
                StartCoroutine(_activeRoutine = FoldRoutine());    
            }
        }

        private IEnumerator FoldRoutine()
        {
            yield return new WaitForSeconds(collapsePlatTimer);
        
            // collapse
            _rb.constraints = RigidbodyConstraints2D.None;
            SetMotorSpeed(_motorSpeed);
            yield return new WaitForSeconds(removeCollisionTimer);
        
            // remove collision from platform
            _colliders[0].enabled = false;  // not sure which one of these is the collider, just do both
            _colliders[1].enabled = false;
            yield return new WaitForSeconds(restorePlatTimer);
        
            // restore platform
            _colliders[0].enabled = true;
            _colliders[1].enabled = true;
            SetMotorSpeed(-_motorSpeed);
            yield return new WaitForSeconds(fixPlatTimer);
        
            // fix platform rb back in place and end
            _rb.constraints = RigidbodyConstraints2D.FreezeAll;
            transform.position = _initPos;
            transform.rotation = _initRot;
            _activeRoutine = null;
        }

        private void SetMotorSpeed(float speed)
        {
            var motor = _hinge.motor;
            motor.motorSpeed = speed;
            _hinge.motor = motor;
        }

        private void OnReset()
        {
            if (_activeRoutine != null)
            {
                StopCoroutine(_activeRoutine);
                _activeRoutine = null;
            }
            _rb.constraints = RigidbodyConstraints2D.FreezeAll;
            transform.position = _initPos;
            transform.rotation = _initRot;
            _colliders[0].enabled = true;
            _colliders[1].enabled = true;
        }
    }
}
