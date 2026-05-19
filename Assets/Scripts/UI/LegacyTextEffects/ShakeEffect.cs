using System.Collections;
using UnityEngine;

namespace LegacyTextEffects
{
    public class ShakeEffect : TextEffectBase
    {
        [SerializeField] private float defaultIntensity = 5.0f;
        [SerializeField] private float defaultDuration = 0.2f;

        private Vector2 _originalPosition;
        private Coroutine _shakeCoroutine;

        protected override void OnEnable()
        {
            base.OnEnable();
            _originalPosition = RectTransform.anchoredPosition;
        }

        public void PlayShake(float intensity = -1, float duration = -1)
        {
            if (intensity < 0) intensity = defaultIntensity;
            if (duration < 0) duration = defaultDuration;

            if (_shakeCoroutine != null) StopCoroutine(_shakeCoroutine);
            _shakeCoroutine = StartCoroutine(DoShake(intensity, duration));
        }

        private IEnumerator DoShake(float intensity, float duration)
        {
            float elapsed = 0;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float currentIntensity = intensity * (1 - (elapsed / duration));
                RectTransform.anchoredPosition = _originalPosition + Random.insideUnitCircle * currentIntensity;
                yield return null;
            }
            RectTransform.anchoredPosition = _originalPosition;
            _shakeCoroutine = null;
        }
    }
}
