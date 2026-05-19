using UnityEngine;

[ExecuteAlways]
public class EnergyOrbController : MonoBehaviour
{
    [Header("Core Settings")]
    public Color energyColor = new Color(0, 0.6f, 1f, 1f);
    [Range(0, 10)] public float intensity = 2.0f;
    [Range(0, 5)] public float pulseSpeed = 1.5f;
    [Range(0, 0.5f)] public float pulseAmount = 0.15f;

    [Header("Emission Settings")]
    public float emissionRate = 20f;
    public float particleLifetime = 1.5f;
    public float flowSpeed = 2f;

    [Header("References")]
    public MeshRenderer coreRenderer;
    public ParticleSystem flowSystem;
    public ParticleSystem sparksSystem;
    public Transform auraTransform;
    public float auraRotationSpeed = 30f;

    private MaterialPropertyBlock _propBlock;
    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    private static readonly int IntensityId = Shader.PropertyToID("_Intensity");
    private static readonly int PulseSpeedId = Shader.PropertyToID("_PulseSpeed");
    private static readonly int PulseAmountId = Shader.PropertyToID("_PulseAmount");

    void OnEnable()
    {
        _propBlock = new MaterialPropertyBlock();
        UpdateVisuals();
    }

    void Update()
    {
        UpdateVisuals();
        
        if (auraTransform != null)
        {
            auraTransform.Rotate(Vector3.forward, auraRotationSpeed * Time.deltaTime);
        }
    }

    public void UpdateVisuals()
    {
        if (coreRenderer != null)
        {
            coreRenderer.GetPropertyBlock(_propBlock);
            _propBlock.SetColor(BaseColorId, energyColor);
            _propBlock.SetFloat(IntensityId, intensity);
            _propBlock.SetFloat(PulseSpeedId, pulseSpeed);
            _propBlock.SetFloat(PulseAmountId, pulseAmount);
            coreRenderer.SetPropertyBlock(_propBlock);
        }

        if (flowSystem != null)
        {
            var main = flowSystem.main;
            main.startColor = energyColor;
            main.startLifetime = particleLifetime;
            main.startSpeed = flowSpeed;

            var emission = flowSystem.emission;
            emission.rateOverTime = emissionRate;
        }

        if (sparksSystem != null)
        {
            var main = sparksSystem.main;
            main.startColor = energyColor;
        }
    }

    // Call this when using as a projectile
    public void InitializeAsProjectile(Vector3 direction, float speed)
    {
        // Add projectile movement logic here if needed
        // Or handle via a separate movement script
    }
}