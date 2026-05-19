using UnityEngine;

namespace LegacyTextEffects
{
    public class BlinkingEffect : TextEffectBase
    {
        [SerializeField] private float interval = 0.5f;
        [SerializeField] private bool active = true;

        private float _timer;
        private bool _isVisible = true;

        private void Update()
        {
            if (!active) return;

            _timer += Time.deltaTime;
            if (_timer >= interval)
            {
                _timer = 0;
                _isVisible = !_isVisible;
                SetVisibility(_isVisible);
            }
        }

        private void SetVisibility(bool visible)
        {
            EnsureCanvasGroup();
            if (CanvasGroup != null)
            {
                CanvasGroup.alpha = visible ? 1 : 0;
            }
            else
            {
                Color c = TextComponent.color;
                c.a = visible ? 1 : 0;
                TextComponent.color = c;
            }
        }
    }
}
