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
    private GameObject[] _layers;
    
    /// <summary>
    /// Fired when camera moves.
    /// Parameter is the amount the camera moved on the x-axis since the last frame.
    /// </summary>
    public event Action<float> Moved;
    
    #endregion
    
    #region Unity Event Handlers

    private void Start()
    {
        _mainCamera = gameObject.GetComponent<Camera>();
        Vector3 screenTopRight = _mainCamera.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0));
        Vector3 screenCenter = _mainCamera.ScreenToWorldPoint(new Vector3((float) Screen.width / 2, (float) Screen.height / 2, 0));
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
        Vector3 velocity = Vector3.zero;
        Vector3 desiredPosition = transform.position + new Vector3(scrollSpeed, 0, 0);
        Vector3 smoothPosition = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, 0.3f);
        transform.position = smoothPosition;
    }

    private void LateUpdate()
    {
        if (!Mathf.Approximately(_prevX, transform.position.x))
        {
            Moved?.Invoke(_prevX - transform.position.x);
            _prevX = transform.position.x;
        }
        
        foreach (GameObject layer in _layers)
        {
            // RepositionLayer(layer);
        }
    }
    
    #endregion
    
    #region Helper Functions

    private void RepositionLayer(GameObject obj)
    {
        float bgWidth = obj.GetComponent<SpriteRenderer>().bounds.size.x;
        float distanceFromCam = obj.transform.position.x - transform.position.x;
        
        if (distanceFromCam >= _screenBounds.x)
        {
            obj.transform.position = new Vector3(obj.transform.position.x - bgWidth / 3, obj.transform.position.y, obj.transform.position.z);
        }
        else if (distanceFromCam <= -_screenBounds.x)
        {
            obj.transform.position = new Vector3(obj.transform.position.x + bgWidth / 3, obj.transform.position.y, obj.transform.position.z);
        }
    }
    
    #endregion
}