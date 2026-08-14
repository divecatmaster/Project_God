using UnityEngine;
using UnityEngine.UI;

namespace God.UI
{
    /// <summary>
    /// UI Old Film Effect Controller for Opening cutscenes and UI overlays.
    /// Controls film grain, vertical scratches, vignette, projector flicker, and frame jitter
    /// without modifying underlying image colors.
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(Graphic))]
    [AddComponentMenu("UI/Effects/Old Film Effect")]
    public class UIOldFilmEffect : MonoBehaviour
    {
        [Header("Master Controls")]
        [SerializeField, Range(0f, 1f), Tooltip("Overall intensity of the old film effect. Set to 0 to disable all film effects.")]
        private float masterIntensity = 1.0f;

        [Header("Film Components")]
        [SerializeField, Range(0f, 1f)]
        private float grainIntensity = 0.2f;

        [SerializeField, Range(0f, 1f)]
        private float scratchIntensity = 0.35f;

        [SerializeField, Range(0f, 1f)]
        private float dustDensity = 0.25f;

        [SerializeField, Range(0f, 1f)]
        private float vignetteIntensity = 0.5f;

        [SerializeField, Range(0f, 1f)]
        private float vignetteSmoothness = 0.5f;

        [SerializeField, Range(0f, 1f)]
        private float flickerIntensity = 0.12f;

        [SerializeField, Range(0f, 1f)]
        private float jitterIntensity = 0.03f;

        // Shader Property IDs
        private static readonly int MasterIntensityId = Shader.PropertyToID("_MasterIntensity");
        private static readonly int GrainIntensityId = Shader.PropertyToID("_GrainIntensity");
        private static readonly int ScratchIntensityId = Shader.PropertyToID("_ScratchIntensity");
        private static readonly int DustDensityId = Shader.PropertyToID("_DustDensity");
        private static readonly int VignetteIntensityId = Shader.PropertyToID("_VignetteIntensity");
        private static readonly int VignetteSmoothnessId = Shader.PropertyToID("_VignetteSmoothness");
        private static readonly int FlickerIntensityId = Shader.PropertyToID("_FlickerIntensity");
        private static readonly int JitterIntensityId = Shader.PropertyToID("_JitterIntensity");

        private Graphic _graphic;
        private Material _instantiatedMaterial;

        public float MasterIntensity
        {
            get => masterIntensity;
            set
            {
                masterIntensity = Mathf.Clamp01(value);
                UpdateMaterialProperties();
            }
        }

        public float GrainIntensity
        {
            get => grainIntensity;
            set
            {
                grainIntensity = Mathf.Clamp01(value);
                UpdateMaterialProperties();
            }
        }

        public float ScratchIntensity
        {
            get => scratchIntensity;
            set
            {
                scratchIntensity = Mathf.Clamp01(value);
                UpdateMaterialProperties();
            }
        }

        public float VignetteIntensity
        {
            get => vignetteIntensity;
            set
            {
                vignetteIntensity = Mathf.Clamp01(value);
                UpdateMaterialProperties();
            }
        }

        public float FlickerIntensity
        {
            get => flickerIntensity;
            set
            {
                flickerIntensity = Mathf.Clamp01(value);
                UpdateMaterialProperties();
            }
        }

        public float JitterIntensity
        {
            get => jitterIntensity;
            set
            {
                jitterIntensity = Mathf.Clamp01(value);
                UpdateMaterialProperties();
            }
        }

        private void OnEnable()
        {
            _graphic = GetComponent<Graphic>();
            if (_graphic != null)
            {
                _graphic.raycastTarget = false; // Non-interactive overlay
            }

            EnsureMaterial();
            UpdateMaterialProperties();
        }

        private void OnDisable()
        {
            if (_instantiatedMaterial != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(_instantiatedMaterial);
                }
                else
                {
                    DestroyImmediate(_instantiatedMaterial);
                }
                _instantiatedMaterial = null;
            }
        }

        private void Update()
        {
            if (_instantiatedMaterial != null)
            {
                UpdateMaterialProperties();
            }
        }

        private void OnValidate()
        {
            if (_graphic == null)
            {
                _graphic = GetComponent<Graphic>();
            }

            EnsureMaterial();
            UpdateMaterialProperties();
        }

        private void EnsureMaterial()
        {
            if (_graphic == null) return;

            Shader shader = Shader.Find("Custom/UIOldFilmShader");
            if (shader == null)
            {
                Debug.LogError("[UIOldFilmEffect] Custom/UIOldFilmShader shader not found.");
                return;
            }

            if (_instantiatedMaterial == null || _instantiatedMaterial.shader != shader)
            {
                _instantiatedMaterial = new Material(shader);
                _instantiatedMaterial.name = "UIOldFilm_Instance";
                _instantiatedMaterial.hideFlags = HideFlags.DontSave;
                _graphic.material = _instantiatedMaterial;
            }
        }

        public void UpdateMaterialProperties()
        {
            if (_instantiatedMaterial == null)
            {
                EnsureMaterial();
                if (_instantiatedMaterial == null) return;
            }

            _instantiatedMaterial.SetFloat(MasterIntensityId, masterIntensity);
            _instantiatedMaterial.SetFloat(GrainIntensityId, grainIntensity);
            _instantiatedMaterial.SetFloat(ScratchIntensityId, scratchIntensity);
            _instantiatedMaterial.SetFloat(DustDensityId, dustDensity);
            _instantiatedMaterial.SetFloat(VignetteIntensityId, vignetteIntensity);
            _instantiatedMaterial.SetFloat(VignetteSmoothnessId, vignetteSmoothness);
            _instantiatedMaterial.SetFloat(FlickerIntensityId, flickerIntensity);
            _instantiatedMaterial.SetFloat(JitterIntensityId, jitterIntensity);
        }

        /// <summary>
        /// Public helper to easily adjust the overall intensity (0 to 1).
        /// </summary>
        public void SetMasterIntensity(float intensity)
        {
            MasterIntensity = intensity;
        }

        /// <summary>
        /// Public helper to get current intensity.
        /// </summary>
        public float GetMasterIntensity()
        {
            return masterIntensity;
        }
    }
}
