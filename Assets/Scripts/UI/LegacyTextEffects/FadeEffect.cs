using System.Collections;
using UnityEngine;

namespace LegacyTextEffects
{
    public class FadeEffect : TextEffectBase
    {
        [SerializeField] private float fadeDuration = 0.5f;

        public void FadeIn(System.Action onComplete = null)
        {
            StopAllCoroutines();
            StartCoroutine(DoFade(0, 1, onComplete));
        }

        public void FadeOut(System.Action onComplete = null)
        {
            StopAllCoroutines();
            StartCoroutine(DoFade(1, 0, onComplete));
        }

        private IEnumerator DoFade(float startAlpha, float endAlpha, System.Action onComplete)
        {
            EnsureCanvasGroup();
            
            float elapsed = 0;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / fadeDuration);
                
                if (CanvasGroup != null)
                {
                    CanvasGroup.alpha = alpha;
                }
                else
                {
                    Color c = TextComponent.color;
                    c.a = alpha;
                    TextComponent.color = c;
                }
                yield return null;
            }

            if (CanvasGroup != null) CanvasGroup.alpha = endAlpha;
            onComplete?.Invoke();
        }
    }
}
