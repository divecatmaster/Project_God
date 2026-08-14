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

        [SerializeField, Min(0f), Tooltip("Overall speed multiplier for film animations (grain, scratches, flicker, jitter). 1.0 is normal speed, 0.5 is slow motion, 2.0 is fast.")]
        private float masterSpeed = 1.0f;

        [Header("Film Components")]
        [SerializeField, Range(0f, 1f)]
        private float grainIntensity = 0.2f;

        [SerializeField, Min(0f)]
        private float grainSpeed = 24.0f;

        [SerializeField, Range(0f, 1f)]
        private float scratchIntensity = 0.35f;

        [SerializeField, Min(0f)]
        private float scratchSpeed = 12.0f;

        [SerializeField, Range(0f, 1f)]
        private float dustDensity = 0.25f;

        [SerializeField, Min(0f)]
        private float dustSpeed = 14.0f;

        [Header("Custom Dust Textures (Optional 1~3 slots)")]
        [SerializeField, Tooltip("Custom dust/speck texture slot 1. If unassigned, procedural noise is used.")]
        private Texture2D dustTex1;

        [SerializeField, Tooltip("Custom dust/speck texture slot 2. Leave empty if not used.")]
        private Texture2D dustTex2;

        [SerializeField, Tooltip("Custom dust/speck texture slot 3. Leave empty if not used.")]
        private Texture2D dustTex3;

        [SerializeField, Range(0.1f, 5.0f), Tooltip("Scale multiplier for custom dust textures.")]
        private float dustTexScale = 1.0f;

        [Header("Vignette")]
        [SerializeField, Range(0f, 1f)]
        private float vignetteIntensity = 0.5f;

        [SerializeField, Range(0f, 1f)]
        private float vignetteSmoothness = 0.5f;

        [SerializeField, Range(0f, 1f)]
        private float flickerIntensity = 0.12f;

        [SerializeField, Min(0f)]
        private float flickerSpeed = 18.0f;

        [SerializeField, Range(0f, 1f)]
        private float jitterIntensity = 0.03f;

        [SerializeField, Min(0f)]
        private float jitterSpeed = 20.0f;

        // Shader Property IDs
        private static readonly int MasterIntensityId = Shader.PropertyToID("_MasterIntensity");
        private static readonly int MasterSpeedId = Shader.PropertyToID("_MasterSpeed");
        private static readonly int GrainIntensityId = Shader.PropertyToID("_GrainIntensity");
        private static readonly int GrainSpeedId = Shader.PropertyToID("_GrainSpeed");
        private static readonly int ScratchIntensityId = Shader.PropertyToID("_ScratchIntensity");
        private static readonly int ScratchSpeedId = Shader.PropertyToID("_ScratchSpeed");
        private static readonly int DustDensityId = Shader.PropertyToID("_DustDensity");
        private static readonly int DustSpeedId = Shader.PropertyToID("_DustSpeed");
        private static readonly int DustTex1Id = Shader.PropertyToID("_DustTex1");
        private static readonly int DustTex2Id = Shader.PropertyToID("_DustTex2");
        private static readonly int DustTex3Id = Shader.PropertyToID("_DustTex3");
        private static readonly int DustTex1AssignedId = Shader.PropertyToID("_DustTex1Assigned");
        private static readonly int DustTex2AssignedId = Shader.PropertyToID("_DustTex2Assigned");
        private static readonly int DustTex3AssignedId = Shader.PropertyToID("_DustTex3Assigned");
        private static readonly int DustTexScaleId = Shader.PropertyToID("_DustTexScale");
        private static readonly int VignetteIntensityId = Shader.PropertyToID("_VignetteIntensity");
        private static readonly int VignetteSmoothnessId = Shader.PropertyToID("_VignetteSmoothness");
        private static readonly int FlickerIntensityId = Shader.PropertyToID("_FlickerIntensity");
        private static readonly int FlickerSpeedId = Shader.PropertyToID("_FlickerSpeed");
        private static readonly int JitterIntensityId = Shader.PropertyToID("_JitterIntensity");
        private static readonly int JitterSpeedId = Shader.PropertyToID("_JitterSpeed");

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

        public float MasterSpeed
        {
            get => masterSpeed;
            set
            {
                masterSpeed = Mathf.Max(0f, value);
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
            _instantiatedMaterial.SetFloat(MasterSpeedId, masterSpeed);
            _instantiatedMaterial.SetFloat(GrainIntensityId, grainIntensity);
            _instantiatedMaterial.SetFloat(GrainSpeedId, grainSpeed);
            _instantiatedMaterial.SetFloat(ScratchIntensityId, scratchIntensity);
            _instantiatedMaterial.SetFloat(ScratchSpeedId, scratchSpeed);
            _instantiatedMaterial.SetFloat(DustDensityId, dustDensity);
            _instantiatedMaterial.SetFloat(DustSpeedId, dustSpeed);

            // Custom Dust Textures binding
            if (dustTex1 != null)
            {
                _instantiatedMaterial.SetTexture(DustTex1Id, dustTex1);
                _instantiatedMaterial.SetFloat(DustTex1AssignedId, 1.0f);
            }
            else
            {
                _instantiatedMaterial.SetFloat(DustTex1AssignedId, 0.0f);
            }

            if (dustTex2 != null)
            {
                _instantiatedMaterial.SetTexture(DustTex2Id, dustTex2);
                _instantiatedMaterial.SetFloat(DustTex2AssignedId, 1.0f);
            }
            else
            {
                _instantiatedMaterial.SetFloat(DustTex2AssignedId, 0.0f);
            }

            if (dustTex3 != null)
            {
                _instantiatedMaterial.SetTexture(DustTex3Id, dustTex3);
                _instantiatedMaterial.SetFloat(DustTex3AssignedId, 1.0f);
            }
            else
            {
                _instantiatedMaterial.SetFloat(DustTex3AssignedId, 0.0f);
            }

            _instantiatedMaterial.SetFloat(DustTexScaleId, dustTexScale);

            _instantiatedMaterial.SetFloat(VignetteIntensityId, vignetteIntensity);
            _instantiatedMaterial.SetFloat(VignetteSmoothnessId, vignetteSmoothness);
            _instantiatedMaterial.SetFloat(FlickerIntensityId, flickerIntensity);
            _instantiatedMaterial.SetFloat(FlickerSpeedId, flickerSpeed);
            _instantiatedMaterial.SetFloat(JitterIntensityId, jitterIntensity);
            _instantiatedMaterial.SetFloat(JitterSpeedId, jitterSpeed);
        }

        /// <summary>
        /// Sets up to 3 custom dust textures. Any null slot will be ignored/cleared.
        /// If all slots are null, falls back to procedural dust/specks noise.
        /// </summary>
        public void SetDustTextures(Texture2D tex1, Texture2D tex2 = null, Texture2D tex3 = null)
        {
            dustTex1 = tex1;
            dustTex2 = tex2;
            dustTex3 = tex3;
            UpdateMaterialProperties();
        }

        /// <summary>
        /// Sets a specific dust texture slot (0, 1, or 2).
        /// </summary>
        public void SetDustTexture(int slotIndex, Texture2D texture)
        {
            switch (slotIndex)
            {
                case 0: dustTex1 = texture; break;
                case 1: dustTex2 = texture; break;
                case 2: dustTex3 = texture; break;
            }
            UpdateMaterialProperties();
        }

        /// <summary>
        /// Sets scale multiplier for custom dust textures.
        /// </summary>
        public void SetDustTexScale(float scale)
        {
            dustTexScale = Mathf.Max(0.01f, scale);
            UpdateMaterialProperties();
        }

        /// <summary>
        /// Public helper to easily adjust the overall intensity (0 to 1).
        /// </summary>
        public void SetMasterIntensity(float intensity)
        {
            MasterIntensity = intensity;
        }

        /// <summary>
        /// Public helper to easily adjust the overall speed multiplier (e.g. 1.0 = normal, 0.5 = slow, 2.0 = fast).
        /// </summary>
        public void SetMasterSpeed(float speed)
        {
            MasterSpeed = speed;
        }

        /// <summary>
        /// Public helper to adjust transparency/alpha via Graphic Color (0.0 to 1.0).
        /// </summary>
        public void SetAlpha(float alpha)
        {
            if (_graphic == null) _graphic = GetComponent<Graphic>();
            if (_graphic != null)
            {
                Color c = _graphic.color;
                c.a = Mathf.Clamp01(alpha);
                _graphic.color = c;
            }
        }

        /// <summary>
        /// Gets current graphic alpha (0.0 to 1.0).
        /// </summary>
        public float GetAlpha()
        {
            if (_graphic == null) _graphic = GetComponent<Graphic>();
            return _graphic != null ? _graphic.color.a : 1f;
        }

        /// <summary>
        /// Public helper to get current intensity.
        /// </summary>
        public float GetMasterIntensity()
        {
            return masterIntensity;
        }

        /// <summary>
        /// Public helper to get current speed.
        /// </summary>
        public float GetMasterSpeed()
        {
            return masterSpeed;
        }
    }
}
