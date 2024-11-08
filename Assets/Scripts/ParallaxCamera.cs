using System;
using UnityEngine;

/// <summary>
/// Creates tiling background and moves them according to their parallax factors as the camera moves
/// </summary>
public class ParallaxCamera : MonoBehaviour
{
    #region Properties
    
    [SerializeField] private float choke;
    [SerializeField] private float scrollSpeed;
    [SerializeField] private int repeats = 3;
    private Camera _mainCamera;
    private Vector2 _screenBounds;
    private float _prevX;
    private float _prevY;
    private GameObject[] _layers;
    
    /// <summary>
    /// Fired when camera moves.
    /// Parameter is the amount the camera moved on the x-axis since the last frame.
    /// </summary>
    public event Action<float, float> Moved;
    
    #endregion
    
    #region Unity Event Handlers

    private void Start()
    {
        _mainCamera = gameObject.GetComponent<Camera>();
        var screenTopRight = _mainCamera.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0));
        var screenCenter = _mainCamera.ScreenToWorldPoint(new Vector3((float) Screen.width / 2, (float) Screen.height / 2, 0));
        _screenBounds = new Vector2(Mathf.Abs(screenTopRight.x - screenCenter.x), Mathf.Abs(screenTopRight.y - screenCenter.y));
        
        GameObject background = GameObject.FindGameObjectWithTag("ParallaxBackground");
        _layers = new GameObject[background.transform.childCount];
        int i = 0;
        foreach (Transform child in background.transform)
        {
            _layers[i] = child.gameObject;
            i++;
        }
    }

    private void Update()
    {
        var velocity = Vector3.zero;
        var desiredPosition = transform.position + new Vector3(scrollSpeed, 0, 0);
        var smoothPosition = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, 0.3f);
        transform.position = smoothPosition;
    }

    private void LateUpdate()
    {
        if (!Mathf.Approximately(_prevX, transform.position.x) | !Mathf.Approximately(_prevY, transform.position.y))
        {
            Moved?.Invoke(_prevX - transform.position.x, _prevY - transform.position.y);
            _prevX = transform.position.x;
            _prevY = transform.position.y;
        }
        
        foreach (var layer in _layers)
        {
            // RepositionLayer(layer);
        }
    }
    
    #endregion
    
    #region Helper Functions

    private void RepositionLayer(GameObject obj)
    {
        var bgWidth = obj.GetComponent<SpriteRenderer>().bounds.size.x;
        var bgHeight = obj.GetComponent<SpriteRenderer>().bounds.size.y;
        var distanceFromCam = obj.transform.position.x - transform.position.x;
        
        if (distanceFromCam >= _screenBounds.x)
        {
            obj.transform.position = new Vector3(obj.transform.position.x - bgWidth / 3, obj.transform.position.y, obj.transform.position.z);
        }
        else if (distanceFromCam <= -_screenBounds.x)
        {
            obj.transform.position = new Vector3(obj.transform.position.x + bgWidth / 3, obj.transform.position.y, obj.transform.position.z);
        }
        
        if (distanceFromCam >= _screenBounds.y)
        {
            obj.transform.position = new Vector3(obj.transform.position.x, obj.transform.position.y- bgHeight / 3, obj.transform.position.z);
        }
        else if (distanceFromCam <= -_screenBounds.y)
        {
            obj.transform.position = new Vector3(obj.transform.position.x, obj.transform.position.y- bgHeight / 3, obj.transform.position.z);
        }
    }
    
    #endregion
}