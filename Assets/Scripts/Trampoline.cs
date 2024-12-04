using UnityEngine;

/// <summary>
/// Class represents a trampoline that is a trigger
/// </summary>
public class Trampoline : MonoBehaviour
{
    #region Serialized Public Fields
    [Header("Bouncing")] 
    [SerializeField] public float yBounceForce;
    [SerializeField] public float xBounceForce;
    #endregion
}
