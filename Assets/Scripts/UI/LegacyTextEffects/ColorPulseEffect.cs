using UnityEngine;

namespace LegacyTextEffects
{
    public class ColorPulseEffect : TextEffectBase
    {
        [SerializeField] private Color pulseColor = Color.yellow;
        [SerializeField] private float speed = 2.0f;
        [SerializeField] private bool active = true;

        private Color _originalColor;

        protected override void OnEnable()
        {
            base.OnEnable();
            _originalColor = TextComponent.color;
        }

        private void Update()
        {
            if (!active) return;

            float t = (Mathf.Sin(Time.time * speed) + 1.0f) / 2.0f;
            TextComponent.color = Color.Lerp(_originalColor, pulseColor, t);
        }

        public void SetActive(bool isActive)
        {
            active = isActive;
            if (!active) TextComponent.color = _originalColor;
        }
    }
}
