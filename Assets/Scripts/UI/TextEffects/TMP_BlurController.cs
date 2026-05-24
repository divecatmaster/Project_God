using UnityEngine;
using TMPro;

namespace God.UI.TextEffects
{
    /// <summary>
    /// Production-ready controller for the TMP Blur effect.
    /// Manages material instances and property updates for TextMeshProUGUI.
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(TextMeshProUGUI))]
    [AddComponentMenu("UI/Effects/TMP Blur Controller")]
    public class TMP_BlurController : MonoBehaviour
    {
        [Header("Blur Settings")]
        [Range(0f, 20f)]
        [SerializeField] private float blurStrength = 2f;

        [Range(0f, 1f)]
        [SerializeField] private float softness = 0.2f;

        [Header("Glow Settings")]
        [SerializeField] private bool enableGlow = false;

        [Range(0.5f, 2f)]
        [SerializeField] private float glowIntensity = 1.2f;

        [Header("Advanced")]
        [SerializeField] private bool useSharedMaterial = false;

        private TextMeshProUGUI _textComponent;
        private Material _internalMaterial;
        
        // Property IDs for performance
        private static readonly int BlurStrengthId = Shader.PropertyToID("_BlurStrength");
        private static readonly int BlurSoftnessId = Shader.PropertyToID("_BlurSoftness");
        private static readonly int BlurIntensityId = Shader.PropertyToID("_BlurIntensity");
        private static readonly int BlurGlowModeId = Shader.PropertyToID("BLUR_GLOW_ON");

        private void Awake()
        {
            _textComponent = GetComponent<TextMeshProUGUI>();
        }

        private void OnEnable()
        {
            UpdateEffect();
        }

        private void OnDisable()
        {
            // Reset to default if needed, or just let the material instance be destroyed
        }

        private void OnValidate()
        {
            if (_textComponent == null) _textComponent = GetComponent<TextMeshProUGUI>();
            UpdateEffect();
        }

        private void Update()
        {
            // Optional: Support real-time updates if properties are animated via other scripts
            // For production, we usually update only on change or in OnValidate
            #if UNITY_EDITOR
            if (!Application.isPlaying) UpdateEffect();
            #endif
        }

        public void UpdateEffect()
        {
            if (_textComponent == null) return;

            // Ensure we are using the correct shader
            Material mat = useSharedMaterial ? _textComponent.fontSharedMaterial : _textComponent.fontMaterial;
            
            if (mat == null) return;

            if (mat.shader.name != "TextMeshPro/Mobile/Distance Field Blur")
            {
                Debug.LogWarning($"[TMP_BlurController] Material on {gameObject.name} is not using the Blur shader. Please assign the 'TextMeshPro/Mobile/Distance Field Blur' shader to the material preset.");
                return;
            }

            // Apply properties
            mat.SetFloat(BlurStrengthId, blurStrength);
            mat.SetFloat(BlurSoftnessId, softness);
            mat.SetFloat(BlurIntensityId, glowIntensity);

            if (enableGlow)
                mat.EnableKeyword("BLUR_GLOW_ON");
            else
                mat.DisableKeyword("BLUR_GLOW_ON");

            // TMP specific: Need to notify the component that properties changed
            _textComponent.SetVerticesDirty();
            _textComponent.SetMaterialDirty();
        }

        /// <summary>
        /// Public API to adjust blur at runtime
        /// </summary>
        public void SetBlurStrength(float strength)
        {
            blurStrength = strength;
            UpdateEffect();
        }

        public void SetGlow(bool enabled)
        {
            enableGlow = enabled;
            UpdateEffect();
        }
    }
}
