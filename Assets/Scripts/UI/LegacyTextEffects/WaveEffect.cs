using UnityEngine;

namespace LegacyTextEffects
{
    public class WaveEffect : TextEffectBase
    {
        [SerializeField] private float amplitude = 10.0f;
        [SerializeField] private float frequency = 2.0f;
        [SerializeField] private bool horizontal = false;

        private Vector2 _originalPosition;

        protected override void OnEnable()
        {
            base.OnEnable();
            _originalPosition = RectTransform.anchoredPosition;
        }

        private void Update()
        {
            float offset = Mathf.Sin(Time.time * frequency) * amplitude;
            if (horizontal)
            {
                RectTransform.anchoredPosition = _originalPosition + new Vector2(offset, 0);
            }
            else
            {
                RectTransform.anchoredPosition = _originalPosition + new Vector2(0, offset);
            }
        }
    }
}
