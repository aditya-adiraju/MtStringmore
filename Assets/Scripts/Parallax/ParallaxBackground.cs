using System.Collections.Generic;
using UnityEngine;

namespace Parallax
{
    /// <summary>
    /// Loads background layers from child objects, and moves them with respect to camera movement
    /// </summary>
    public class ParallaxBackground : MonoBehaviour
    {
        private ParallaxCamera _parallaxCamera;
        private readonly List<ParallaxLayer> _parallaxLayers = new();

        private void Awake()
        {
            _parallaxCamera = Camera.main?.GetComponent<ParallaxCamera>();

            if (_parallaxCamera)
                _parallaxCamera.Moved += Move;

            _parallaxLayers.AddRange(GetComponentsInChildren<ParallaxLayer>());
            // Z0rb14n: do we *really* need this??
            //          actually wait, if we have each layer listening to the event, do we even need THIS FILE???
            // for (int i = 0; i < _parallaxLayers.Count; i++) _parallaxLayers[i].gameObject.name = $"Layer-{i}";
        }

        /// <summary>
        /// Moves all child layers with the given camera data.
        /// </summary>
        /// <param name="motionData">Camera motion data</param>
        private void Move(ParallaxCamera.CameraMotionData motionData)
        {
            foreach (ParallaxLayer layer in _parallaxLayers)
            {
                layer.Move(motionData.delta);
                layer.Reposition(motionData.position, motionData.screenWidth);
            }
        }
    }
}
