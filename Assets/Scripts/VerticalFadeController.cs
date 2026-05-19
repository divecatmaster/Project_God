using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
[AddComponentMenu("UI/Effects/Vertical Fade Controller")]
public class VerticalFadeController : MonoBehaviour
{
    [Header("Settings")]
    [Range(0, 1)]
    [SerializeField] private float fadeProgress = 0f;
    [Range(0.0001f, 1f)]
    [SerializeField] private float featherAmount = 0.1f;
    [SerializeField] private bool reverseDirection = false;

    [Header("References (Auto-detected)")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Graphic uiGraphic;

    private static readonly int FadeProgressProp = Shader.PropertyToID("_FadeProgress");
    private static readonly int FeatherAmountProp = Shader.PropertyToID("_Feather");
    private static readonly int ReverseDirectionProp = Shader.PropertyToID("_ReverseDirection");

    private MaterialPropertyBlock propertyBlock;
    private Material uiMaterialInstance;

    public float FadeProgress
    {
        get => fadeProgress;
        set
        {
            fadeProgress = Mathf.Clamp01(value);
            UpdateProperties();
        }
    }

    public float FeatherAmount
    {
        get => featherAmount;
        set
        {
            featherAmount = Mathf.Max(0.0001f, value);
            UpdateProperties();
        }
    }

    public bool ReverseDirection
    {
        get => reverseDirection;
        set
        {
            reverseDirection = value;
            UpdateProperties();
        }
    }

    private void OnEnable()
    {
        DetectComponents();
        UpdateProperties();
    }

    private void OnValidate()
    {
        UpdateProperties();
    }

    private void OnDestroy()
    {
        if (uiMaterialInstance != null)
        {
            if (Application.isPlaying)
                Destroy(uiMaterialInstance);
            else
                DestroyImmediate(uiMaterialInstance);
        }
    }

    private void DetectComponents()
    {
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (uiGraphic == null) uiGraphic = GetComponent<Graphic>();
    }

    public void UpdateProperties()
    {
        if (spriteRenderer != null)
        {
            if (propertyBlock == null) propertyBlock = new MaterialPropertyBlock();
            spriteRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetFloat(FadeProgressProp, fadeProgress);
            propertyBlock.SetFloat(FeatherAmountProp, featherAmount);
            propertyBlock.SetFloat(ReverseDirectionProp, reverseDirection ? 1f : 0f);
            spriteRenderer.SetPropertyBlock(propertyBlock);
        }

        if (uiGraphic != null)
        {
            // For UI, we must use a material instance for per-object animation
            if (uiMaterialInstance == null)
            {
                if (uiGraphic.material != null && uiGraphic.material.shader.name.Contains("VerticalDissolve"))
                {
                    uiMaterialInstance = new Material(uiGraphic.material);
                    uiMaterialInstance.name += " (Instance)";
                    uiGraphic.material = uiMaterialInstance;
                }
            }

            if (uiMaterialInstance != null)
            {
                uiMaterialInstance.SetFloat(FadeProgressProp, fadeProgress);
                uiMaterialInstance.SetFloat(FeatherAmountProp, featherAmount);
                uiMaterialInstance.SetFloat(ReverseDirectionProp, reverseDirection ? 1f : 0f);
            }
        }
    }
}