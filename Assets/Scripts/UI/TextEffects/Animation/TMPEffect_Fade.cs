using System.Collections;
using TMPro;
using UnityEngine;

namespace UI.TextEffects
{
    /// <summary>
    /// Smoothly fades text alpha.
    /// </summary>
    [RequireComponent(typeof(TMP_Text))]
    public class TMPEffect_Fade : MonoBehaviour
    {
        [SerializeField] private float duration = 1.0f;
        
        private TMP_Text _textComponent;
        private Coroutine _fadeRoutine;

        private void Awake()
        {
            _textComponent = GetComponent<TMP_Text>();
        }

        public void FadeIn() => Play(0, 1);
        public void FadeOut() => Play(1, 0);

        public void Play(float from, float to)
        {
            if (_fadeRoutine != null) StopCoroutine(_fadeRoutine);
            _fadeRoutine = StartCoroutine(FadeRoutine(from, to));
        }

        private IEnumerator FadeRoutine(float from, float to)
        {
            float elapsed = 0;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                float alpha = Mathf.Lerp(from, to, t);
                
                Color c = _textComponent.color;
                c.a = alpha;
                _textComponent.color = c;
                
                yield return null;
            }
            Color final = _textComponent.color;
            final.a = to;
            _textComponent.color = final;
        }
    }
}