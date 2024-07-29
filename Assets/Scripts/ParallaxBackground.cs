using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Loads background layers from child objects, and moves them with respect to camera movement
/// </summary>
public class ParallaxBackground : MonoBehaviour
{
    private ParallaxCamera _parallaxCamera;
    private readonly List<ParallaxLayer> _parallaxLayers = new();

    private void Start()
    {
        _parallaxCamera = Camera.main?.GetComponent<ParallaxCamera>();

        if (_parallaxCamera is not null)
            _parallaxCamera.Moved += Move;

        SetLayers();
    }

    private void SetLayers()
    {
        _parallaxLayers.Clear();

        for (int i = 0; i < transform.childCount; i++)
        {
            ParallaxLayer layer = transform.GetChild(i).GetComponent<ParallaxLayer>();

            if (layer is not null)
            {
                layer.name = "Layer-" + i;
                _parallaxLayers.Add(layer);
            }
        }
    }

    private void Move(float delta)
    {
        foreach (ParallaxLayer layer in _parallaxLayers)
        {
            layer.Move(delta);
        }
    }
}