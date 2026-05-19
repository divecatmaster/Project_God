using TMPro;
using UnityEngine;

namespace UI.TextEffects
{
    /// <summary>
    /// Animates TMP Glow properties via the material instance.
    /// </summary>
    [RequireComponent(typeof(TMP_Text))]
    public class TMPEffect_GlowPulse : MonoBehaviour
    {
        [Header("Glow Settings")]
        [SerializeField] private Color glowColor = Color.cyan;
        [SerializeField] private float minInner = 0.05f;
        [SerializeField] private float maxInner = 0.4f;
        [SerializeField] private float pulseSpeed = 2f;

        private TMP_Text _textComponent;
        private Material _materialInstance;

        private void Awake()
        {
            _textComponent = GetComponent<TMP_Text>();
            // Accessing fontMaterial creates an instance of the material for this object
            _materialInstance = _textComponent.fontMaterial;
            
            // Ensure glow is enabled
            _materialInstance.EnableKeyword("GLOW_ON");
            _materialInstance.SetColor(ShaderUtilities.ID_GlowColor, glowColor);
        }

        private void OnDestroy()
        {
            if (_materialInstance != null)
            {
                if (Application.isPlaying)
                    Destroy(_materialInstance);
                else
                    DestroyImmediate(_materialInstance);
            }
        }

        private void Update()
        {
            if (_materialInstance == null) return;

            float pulse = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f; // 0 to 1
            float glowInner = Mathf.Lerp(minInner, maxInner, pulse);
            
            _materialInstance.SetFloat(ShaderUtilities.ID_GlowInner, glowInner);
        }
    }
}