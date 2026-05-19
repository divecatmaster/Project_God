using System.Collections;
using UnityEngine;

namespace UI.TextEffects
{
    /// <summary>
    /// Punches the scale of the object. Useful for UI feedback on clicks or important info.
    /// </summary>
    public class TMPEffect_ScaleBounce : MonoBehaviour
    {
        [SerializeField] private float bounceStrength = 1.2f;
        [SerializeField] private float duration = 0.3f;
        [SerializeField] private AnimationCurve bounceCurve = AnimationCurve.EaseInOut(0, 1, 1, 1);

        private Vector3 _originalScale;
        private Coroutine _bounceRoutine;

        private void Awake()
        {
            _originalScale = transform.localScale;
        }

        [ContextMenu("Play Bounce")]
        public void Play()
        {
            if (_bounceRoutine != null) StopCoroutine(_bounceRoutine);
            _bounceRoutine = StartCoroutine(BounceRoutine());
        }

        private IEnumerator BounceRoutine()
        {
            float elapsed = 0;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                
                // Sin wave for a simple bounce
                float s = 1f + Mathf.Sin(t * Mathf.PI) * (bounceStrength - 1f);
                transform.localScale = _originalScale * s;
                
                yield return null;
            }
            transform.localScale = _originalScale;
        }
    }
}