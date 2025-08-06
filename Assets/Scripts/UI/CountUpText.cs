using System.Collections;
using Managers;
using TMPro;
using UnityEngine;

namespace UI
{
    /// <summary>
    /// Text to count up to a number in the final results menu page.
    /// </summary>
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class CountUpText : MonoBehaviour
    {
        [SerializeField] private float deathCountRate = 4f;
        [SerializeField] private float collectibleMaxDuration = 1.2f;
        [SerializeField]
        private float timeBetweenCollectibleAudio = 0.15f;
        private TextMeshProUGUI _text;

        private void Awake()
        {
            _text = GetComponent<TextMeshProUGUI>();
        }

        /// <summary>
        /// Starts counting up to the number from 0 at <see cref="deathCountRate"/> units/sec.
        /// </summary>
        /// <param name="number">Target final number</param>
        public void RunCountUpWithRate(int number)
        {
            StartCoroutine(CountUpRateCoroutine(number));
        }

        /// <summary>
        /// Starts counting up to (number / max) from 0 with fixed duration <see cref="collectibleMaxDuration"/>.
        /// </summary>
        /// <param name="number"></param>
        /// <param name="max"></param>
        public void RunCountUpForCollectible(int number, int max)
        {
            StartCoroutine(CountUpForCollectible(number, max));
        }

        /// <summary>
        /// Coroutine to count up to a target number from 0 at <see cref="deathCountRate"/> units/sec.
        /// </summary>
        /// <param name="toMax">Target number</param>
        /// <returns>Coroutine</returns>
        private IEnumerator CountUpRateCoroutine(int toMax)
        {
            float time = 0;
            for (int curr = 0; curr <= toMax; curr = Mathf.Min(toMax, Mathf.RoundToInt(deathCountRate * time)),
                 time += Time.unscaledDeltaTime)
            {
                _text.text = curr.ToString();
                yield return null;
            }
            _text.text = toMax.ToString();
        }

        /// <summary>
        /// Coroutine to count up to (number / max) from 0 with fixed duration <see cref="collectibleMaxDuration"/>.
        /// </summary>
        /// <param name="number">Number to count to</param>
        /// <param name="max">Maximum</param>
        /// <returns>Coroutine</returns>
        private IEnumerator CountUpForCollectible(int number, int max)
        {
            SoundManager.Instance.ResetTimeSinceLastCollect();
            for (float time = 0, timeToNextPlay = 0; time < collectibleMaxDuration; time += Time.unscaledDeltaTime)
            {
                int curr = Mathf.Min(number, Mathf.RoundToInt(max * time / collectibleMaxDuration));
                if (curr == number) break;
                _text.text = $"{curr} / {max}";
                timeToNextPlay -= Time.unscaledDeltaTime;
                if (timeToNextPlay <= 0)
                {
                    SoundManager.Instance.PlayCollectableComboSound();
                    timeToNextPlay = timeBetweenCollectibleAudio;
                }
                yield return null;
            }
            _text.text = $"{number} / {max}";
        }
    }
}
