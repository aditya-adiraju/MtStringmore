using UnityEngine;

public class KnitbyCutsceneAnimator : MonoBehaviour
{
    private static readonly int IdleKey = Animator.StringToHash("Idle");
    [SerializeField] private Animator anim;
    private Vector3 _lastPosition;

    // Update is called once per frame
    private void Update()
    {
        if (Time.deltaTime == 0) return;
        anim.SetBool(IdleKey, Mathf.Approximately(Vector3.Distance(transform.position, _lastPosition), 0));
        _lastPosition = transform.position;
    }
}
