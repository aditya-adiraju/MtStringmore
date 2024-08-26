using System;
using UnityEngine;

/// <summary>
/// Fixes the camera at a set position when the player enters the bounding box.
/// Uses "MainCamera" and "Player" tags.
/// </summary>
public class FixCamera : MonoBehaviour
{
    // TODO make this point visible/movable in editor
    [SerializeField] private Vector2 targetPoint;

    private FollowCamera _cam;

    private void Awake()
    {
        _cam = GameObject.FindWithTag("MainCamera").GetComponent<FollowCamera>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            _cam.FixTarget(targetPoint);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            _cam.FollowPlayer();
        }
    }
}
