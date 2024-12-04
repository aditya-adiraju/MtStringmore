using UnityEngine;

/// <summary>
/// Class represents a bouncy platform that is a 2D collider
/// </summary>
public class BouncyPlatform : MonoBehaviour
{
    #region Serialized Public Fields
    [Header("Bouncing")] 
    [SerializeField] public float yBounceForce;
    [SerializeField] public float xBounceForce;
    #endregion
    
}
