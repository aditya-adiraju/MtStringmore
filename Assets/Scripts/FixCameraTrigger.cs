using UnityEngine;

/// <summary>
/// Fixes the camera at a set position when the player enters the bounding box.
/// Camera position is the centre of the box collider, plus an offset.
/// Uses "MainCamera" and "Player" tags.
/// </summary>
public class FixCameraTrigger : MonoBehaviour
{
    [SerializeField] private Vector2 targetOffset;
    [SerializeField] private bool fixX = true;
    [SerializeField] private bool fixY = true;

    private FollowCamera _cam;
    private Vector2 _target;
    private static int _triggerCount;

    private void Awake()
    {
        _cam = GameObject.FindWithTag("MainCamera").GetComponent<FollowCamera>();
        Vector2 boxOffset = GetComponent<BoxCollider2D>().offset;
        _target = (Vector2) transform.position + boxOffset + targetOffset;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            _cam.FixTarget(_target, fixX, fixY);
            _triggerCount++;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            _triggerCount--;
            if (_triggerCount == 0)
                _cam.FollowPlayer();
        }
    }
}
