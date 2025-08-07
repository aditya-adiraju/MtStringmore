using UnityEngine;

namespace Knitby
{
    public class KnitbyCutsceneAnimator : MonoBehaviour
    {
        private static readonly int IdleKey = Animator.StringToHash("Idle");
        [SerializeField] private Animator anim;
        private Vector3 _lastPosition;

        // Update is called once per frame
        private void Update()
        {
            if (Time.deltaTime == 0) return;
            anim.SetBool(IdleKey, Mathf.Abs(Vector3.Distance(transform.position, _lastPosition)) < 0.05f);
            _lastPosition = transform.position;
        }
    }
}
