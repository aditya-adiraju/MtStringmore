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
        _screenBounds = _mainCamera.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, _mainCamera.transform.position.z));
        _screenBounds = new Vector2(Mathf.Abs(_screenBounds.x), Mathf.Abs(_screenBounds.y));
        
        GameObject background = GameObject.FindGameObjectWithTag("ParallaxBackground");
        _layers = new GameObject[background.transform.childCount];
        int i = 0;
        foreach(Transform child in background.transform)
        {
            _layers[i] = child.gameObject;
            i++;
        }
        
        foreach (GameObject obj in _layers)
        {
            LoadLayers(obj);
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
            RepositionLayer(layer);
        }
    }
    
    #endregion
    
    #region Helper Functions

    private void LoadLayers(GameObject obj)
    {
        float objectWidth = obj.GetComponent<SpriteRenderer>().bounds.size.x - choke;
        // int repeats = (int)Mathf.Ceil(_screenBounds.x * 2 / objectWidth);
        GameObject clone = Instantiate(obj);
        for (int i = 0; i <= repeats; i++)
        {
            GameObject c = Instantiate(clone, obj.transform, true);
            c.transform.position = new Vector3(obj.transform.position.x + objectWidth * i, obj.transform.position.y, obj.transform.position.z);
            c.name = obj.name + i;
        }

        Destroy(clone);
        Destroy(obj.GetComponent<SpriteRenderer>());
    }

    private void RepositionLayer(GameObject obj)
    {
        Transform[] children = obj.GetComponentsInChildren<Transform>();
        if (children.Length > 1)
        {
            GameObject firstChild = children[1].gameObject;
            GameObject lastChild = children[children.Length - 1].gameObject;
            float halfObjectWidth = lastChild.GetComponent<SpriteRenderer>().bounds.extents.x - choke;
            
            if (transform.position.x + _screenBounds.x > lastChild.transform.position.x + halfObjectWidth)
            {
                firstChild.transform.SetAsLastSibling();
                firstChild.transform.position = new Vector3(lastChild.transform.position.x + halfObjectWidth * 2,
                    lastChild.transform.position.y, lastChild.transform.position.z);
            }
            else if (transform.position.x - _screenBounds.x < firstChild.transform.position.x - halfObjectWidth)
            {
                lastChild.transform.SetAsFirstSibling();
                lastChild.transform.position = new Vector3(firstChild.transform.position.x - halfObjectWidth * 2,
                    firstChild.transform.position.y, firstChild.transform.position.z);
            }
        }
    }
    
    #endregion
}