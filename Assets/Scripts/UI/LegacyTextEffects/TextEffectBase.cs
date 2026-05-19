using UnityEngine;
using UnityEngine.UI;

namespace LegacyTextEffects
{
    [RequireComponent(typeof(Text))]
    public abstract class TextEffectBase : MonoBehaviour
    {
        private Text _text;
        private RectTransform _rectTransform;
        private CanvasGroup _canvasGroup;

        protected Text TextComponent => _text ??= GetComponent<Text>();
        protected RectTransform RectTransform => _rectTransform ??= GetComponent<RectTransform>();
        protected CanvasGroup CanvasGroup => _canvasGroup ??= GetComponent<CanvasGroup>();

        protected virtual void OnEnable() { }
        protected virtual void OnDisable() { StopAllCoroutines(); }

        protected void EnsureCanvasGroup()
        {
            if (_canvasGroup == null)
                _canvasGroup = gameObject.GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
        }
    }
}
