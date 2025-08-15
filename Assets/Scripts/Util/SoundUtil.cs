using System.Collections;
using UnityEngine;

namespace Util
{
    /// <summary>
    /// Utility class to manage Sound-related methods.
    /// </summary>
    public static class SoundUtil
    {
        /// <summary>
        /// Converts a slider volume (0.001 - 1) to an AudioMixer volume (-80 dB to +20dB)
        /// </summary>
        /// <remarks>
        /// Why don't we just set sliders to -80 to 20?
        /// </remarks>
        /// <param name="sliderValue">Slider value, 0.001-1</param>
        /// <returns>Converted volume</returns>
        public static float SliderToVolume(float sliderValue)
        {
            return Mathf.Log10(sliderValue) * 20;
        }
        /// <summary>
        /// Fades out an audio source via a Coroutine.
        /// </summary>
        /// <param name="audioSource">AudioSource to fade out</param>
        /// <param name="fadeDuration">Fade duration, seconds</param>
        /// <returns>Coroutine to fade out over time</returns>
        public static IEnumerator FadeOut(this AudioSource audioSource, float fadeDuration = 0)
        {
            if (fadeDuration == 0)
            {
                audioSource.Stop();
                yield break;
            }
        
            float startVolume = audioSource.volume;
            for (float elapsedTime = 0; elapsedTime < fadeDuration; elapsedTime += Time.unscaledDeltaTime)
            {
                audioSource.volume = Mathf.Lerp(startVolume, 0, elapsedTime / fadeDuration);
                yield return null;
            }

            audioSource.Stop();
            audioSource.volume = startVolume;
        }
    }
}
