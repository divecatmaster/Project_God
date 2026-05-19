using System.Collections;
using UnityEngine;

namespace LegacyTextEffects
{
    public class FloatingDamageText : TextEffectBase
    {
        [SerializeField] private float duration = 1.0f;
        [SerializeField] private float moveSpeed = 50.0f;
        [SerializeField] private Vector2 randomSpread = new Vector2(20f, 0f);
        [SerializeField] private AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 1, 1, 1);
        [SerializeField] private AnimationCurve alphaCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

        public void Initialize(string value, Color color, bool isCritical = false)
        {
            TextComponent.text = value;
            TextComponent.color = color;
            
            float spreadX = Random.Range(-randomSpread.x, randomSpread.x);
            float spreadY = Random.Range(-randomSpread.y, randomSpread.y);
            RectTransform.anchoredPosition += new Vector2(spreadX, spreadY);

            if (isCritical)
            {
                RectTransform.localScale = Vector3.one * 1.5f;
            }
            else
            {
                RectTransform.localScale = Vector3.one;
            }

            StopAllCoroutines();
            StartCoroutine(Animate());
        }

        private IEnumerator Animate()
        {
            float elapsed = 0;
            Vector2 startPos = RectTransform.anchoredPosition;
            Vector3 startScale = RectTransform.localScale;
            Color startColor = TextComponent.color;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                // Move up
                RectTransform.anchoredPosition = startPos + Vector2.up * (moveSpeed * t);

                // Scale
                RectTransform.localScale = startScale * scaleCurve.Evaluate(t);

                // Fade
                Color c = startColor;
                c.a = alphaCurve.Evaluate(t);
                TextComponent.color = c;

                yield return null;
            }

            // Callback for pooling could be added here
            gameObject.SetActive(false);
        }
    }
}
