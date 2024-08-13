
using System;
using UnityEngine;

/// <summary>
/// Checkpoint flag that sets checkpoint position when player collides with it
/// </summary>
public class Checkpoint : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator anim;
    [SerializeField] private SpriteRenderer sprite;
    
    private static readonly int HoistKey = Animator.StringToHash("Hoisted");
 
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && anim.GetBool(HoistKey) == false)
        {
            anim.SetBool(HoistKey, true);
            GameManager.Instance.CheckPointPos = transform.position;
        }
    }
    
}
