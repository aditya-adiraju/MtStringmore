using System.Collections;
using UnityEngine;

namespace StringmoreCamera
{
    /// <summary>
    /// Adds a rumbling shake to the MainCamera
    /// </summary>
    public class ShakeCamera : MonoBehaviour
    {
        private FollowCamera _followCamera;
        private Coroutine _activeDashShake;
        private Coroutine _activeBreakingShake;
        
        private void Awake()
        {
            _followCamera = FindObjectOfType<FollowCamera>();
            _activeDashShake = null;
            _activeBreakingShake = null;
        }

        /// <summary>
        /// Public method that any class can shake the camera.
        /// </summary>
        /// <param name = "shakeDuration">the length of time to shake the camera for</param>
        /// <param name = "shakeIntensity">the level of intensity to shake the camera </param>
        /// <param name = "xShake">biases the x direction in which the camera should shake</param>
        /// <param name = "yShake">biases the y direction in which the camera should shake</param>
        /// <param name = "breakable">set to true if object is breakable</param>
        public void Shake(float shakeDuration, float shakeIntensity, bool xShake, bool yShake, bool breakable)
        {
            // Prioritize destructible objects in the event that two shakes happen simultaneously 
            if(_activeBreakingShake != null)
                StopCoroutine(_activeBreakingShake);
            if (_activeDashShake != null)
                StopCoroutine(_activeDashShake);
            
            // Start new shake
            Coroutine coroutine = StartCoroutine(ShakeRoutine(shakeDuration, shakeIntensity, xShake, yShake));
            if (breakable)
            {
                _activeBreakingShake = coroutine;
            }
            else
            {
                _activeDashShake = coroutine;
            }
        }
        
        // Coroutine to shake the camera
        private IEnumerator ShakeRoutine(float shakeDuration, float shakeIntensity, bool xShake, bool yShake)
        {
            for (float elapsed = 0; elapsed < shakeDuration; elapsed += Time.deltaTime)
            {
                // Decrease the shake over time
                float currentIntensity = shakeIntensity * (1f - (elapsed / shakeDuration)); 

                float noiseTime = Time.time * 10f;
                Vector2 shakeOffset = new Vector2(
                    xShake ? (Mathf.PerlinNoise(noiseTime, 0f) - 0.5f) * 2f * currentIntensity : 0f,
                    yShake ? (Mathf.PerlinNoise(0f, noiseTime) - 0.5f) * 2f * currentIntensity : 0f
                );

                _followCamera.ShakeOffset = new Vector3(shakeOffset.x,shakeOffset.y,0);
                yield return null;
            }

            _followCamera.ShakeOffset = Vector3.zero; // Reset after shake
        }
    }
}
