using UnityEngine;

/// <summary>
/// Fixes the camera at a set position when the player enters the bounding box.
/// Camera position is the centre of the box collider, plus an offset.
/// Uses "MainCamera" and "Player" tags.
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
public class FixCameraTrigger : MonoBehaviour
{
    [SerializeField] private Vector2 targetOffset;
    public bool fixX = true;
    public bool fixY = true;

    public Vector2 Target { get; private set; }

    private FollowCamera _cam;

    private void Awake()
    {
        _cam = GameObject.FindWithTag("MainCamera").GetComponent<FollowCamera>();
        Vector2 boxOffset = GetComponent<BoxCollider2D>().offset;
        Target = (Vector2)transform.position + boxOffset + targetOffset;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            _cam.EnterFixCameraTrigger(this);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            _cam.ExitFixCameraTrigger(this);
        }
    }
}
