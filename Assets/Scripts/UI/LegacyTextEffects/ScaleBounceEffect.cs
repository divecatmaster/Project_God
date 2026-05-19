using System.Collections;
using UnityEngine;

namespace LegacyTextEffects
{
    public class ScaleBounceEffect : TextEffectBase
    {
        [SerializeField] private float bounceScale = 1.2f;
        [SerializeField] private float duration = 0.2f;

        private Vector3 _originalScale;
        private Coroutine _bounceCoroutine;

        protected override void OnEnable()
        {
            base.OnEnable();
            _originalScale = RectTransform.localScale;
        }

        public void PlayBounce()
        {
            if (_bounceCoroutine != null) StopCoroutine(_bounceCoroutine);
            _bounceCoroutine = StartCoroutine(DoBounce());
        }

        private IEnumerator DoBounce()
        {
            float elapsed = 0;
            float halfDuration = duration / 2f;

            // Scale up
            while (elapsed < halfDuration)
            {
                elapsed += Time.deltaTime;
                RectTransform.localScale = Vector3.Lerp(_originalScale, _originalScale * bounceScale, elapsed / halfDuration);
                yield return null;
            }

            // Scale down
            elapsed = 0;
            while (elapsed < halfDuration)
            {
                elapsed += Time.deltaTime;
                RectTransform.localScale = Vector3.Lerp(_originalScale * bounceScale, _originalScale, elapsed / halfDuration);
                yield return null;
            }

            RectTransform.localScale = _originalScale;
            _bounceCoroutine = null;
        }
    }
}
