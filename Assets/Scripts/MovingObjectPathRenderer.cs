using UnityEngine;

/// <summary>
/// Path renderer for the Moving Object.
/// </summary>
/// <remarks>
/// This path renderer kinda sucks, but I wanted a lot of cats.
/// </remarks>
[DisallowMultipleComponent, RequireComponent(typeof(SpriteRenderer))]
public class MovingObjectPathRenderer : MonoBehaviour
{
    /// <summary>
    /// Called by the editor to update the location to set the correct location.
    /// </summary>
    /// <param name="attachableMovingObject">AttachableMovingObject to copy the path of.</param>
    /// <remarks>
    /// Can't use OnValidate unless you want the sprite renderer spamming your console.
    /// </remarks>
    public void UpdateLocation(AttachableMovingObject attachableMovingObject)
    {
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (!attachableMovingObject) return;
        Vector2 distance = attachableMovingObject.secondPosition - attachableMovingObject.firstPosition;
        float spriteWidth = spriteRenderer.sprite.bounds.size.x;
        Vector2 size = spriteRenderer.size;
        size.x = Mathf.Ceil(distance.magnitude / spriteWidth) * spriteWidth;
        spriteRenderer.size = size;
        transform.position = attachableMovingObject.firstPosition + distance / 2;
        transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(distance.y, distance.x) * Mathf.Rad2Deg);
    }
}
