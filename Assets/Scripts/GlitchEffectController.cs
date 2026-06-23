using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class GlitchEffectController : MonoBehaviour
{
    public enum GlitchType
    {
        Random,             // Randomly selects a preset for each burst
        BlockShiftOnly,     // Horizontal block tearing only
        RGBSplitOnly,       // Chromatic aberration only
        StaticNoiseOnly,    // Scanlines & static noise only
        ColorInvertOnly,    // Screen flashing and inversions only
        FullChaos,          // All effects combined
        CustomSliders       // Follow the custom weight sliders below
    }

    public enum GlitchMode
    {
        PeriodicBursts,     // Triggers periodic bursts automatically
        ManualOnly          // Only triggers when requested via TriggerGlitch() code or Inspector button
    }

    [Header("References")]
    public Camera renderCamera;
    public Camera outputCamera;
    public RawImage displayImage;
    public Material glitchMaterial;
    public TMP_Text[] glitchyTexts;
    public GlitchAudioSynthesizer audioSynth;
    
    [Header("Glitch Mode & Type")]
    public GlitchMode playMode = GlitchMode.PeriodicBursts;
    [Tooltip("What kind of glitch preset should be used for bursts?")]
    public GlitchType activeGlitchPreset = GlitchType.Random;

    [Header("Custom Preset Sliders (When 'CustomSliders' type is selected)")]
    [Range(0f, 1f)] public float customBlockWeight = 1f;
    [Range(0f, 1f)] public float customRGBSplitWeight = 1f;
    [Range(0f, 1f)] public float customScanlineWeight = 1f;
    [Range(0f, 1f)] public float customNoiseWeight = 1f;
    [Range(0f, 1f)] public float customColorSpikeWeight = 1f;

    [Header("Periodic Timing Settings")]
    public float minTimeBetweenBursts = 3.0f;
    public float maxTimeBetweenBursts = 7.0f;
    public float minBurstDuration = 0.3f;
    public float maxBurstDuration = 1.2f;
    
    [Header("Glitch Intensity & Influence")]
    [Range(0f, 1f)]
    public float baseGlitchIntensity = 0.04f; // constant background jitter
    public float currentIntensity = 0f;
    
    [Header("Text Glitch Settings")]
    [Range(0f, 1f)]
    public float textGlitchChance = 0.3f;
    
    private RenderTexture rt;
    private int lastWidth = 0;
    private int lastHeight = 0;
    
    private string[] originalTextValues;
    private static readonly string GlitchChars = "█░▒▓▖▘▝▞$#@%&?!01XYZ+-/*_=[]{}";

    private Coroutine activeGlitchCoroutine;

    void Awake()
    {
        // Setup initial texture state
        UpdateRenderTexture();
        
        // Cache original text values
        if (glitchyTexts != null && glitchyTexts.Length > 0)
        {
            originalTextValues = new string[glitchyTexts.Length];
            for (int i = 0; i < glitchyTexts.Length; i++)
            {
                if (glitchyTexts[i] != null)
                {
                    originalTextValues[i] = glitchyTexts[i].text;
                }
            }
        }
    }

    void Start()
    {
        if (playMode == GlitchMode.PeriodicBursts)
        {
            activeGlitchCoroutine = StartCoroutine(PeriodicGlitchRoutine());
        }
    }

    void Update()
    {
        // Monitor resolution changes
        if (Screen.width != lastWidth || Screen.height != lastHeight)
        {
            UpdateRenderTexture();
        }
        
        // If CustomSliders mode is active and not inside a periodic burst, use custom values directly
        if (playMode == GlitchMode.ManualOnly || activeGlitchPreset == GlitchType.CustomSliders)
        {
            ApplyWeightsToMaterial(customBlockWeight, customRGBSplitWeight, customScanlineWeight, customNoiseWeight, customColorSpikeWeight);
        }

        // Apply global intensity to material
        if (glitchMaterial != null)
        {
            glitchMaterial.SetFloat("_GlitchIntensity", currentIntensity);
        }
        
        // Apply intensity to audio
        if (audioSynth != null)
        {
            audioSynth.audioIntensity = currentIntensity;
        }
        
        // Handle text glitching based on current intensity
        UpdateTextGlitches();
    }

    void UpdateRenderTexture()
    {
        lastWidth = Screen.width;
        lastHeight = Screen.height;
        
        if (rt != null)
        {
            rt.Release();
        }
        
        // Create 24-bit depth RenderTexture
        rt = new RenderTexture(lastWidth, lastHeight, 24, RenderTextureFormat.ARGB32);
        rt.name = "GlitchRenderTexture";
        rt.Create();
        
        if (renderCamera != null)
        {
            renderCamera.targetTexture = rt;
        }
        
        if (displayImage != null)
        {
            displayImage.texture = rt;
        }
    }

    // Helper to push weights directly into the material properties
    private void ApplyWeightsToMaterial(float b, float rgb, float s, float n, float cs)
    {
        if (glitchMaterial == null) return;
        glitchMaterial.SetFloat("_BlockWeight", b);
        glitchMaterial.SetFloat("_RGBSplitWeight", rgb);
        glitchMaterial.SetFloat("_ScanlineWeight", s);
        glitchMaterial.SetFloat("_NoiseWeight", n);
        glitchMaterial.SetFloat("_ColorSpikeWeight", cs);
    }

    // Sets specific preset weights
    private void SetWeightsByPreset(GlitchType type)
    {
        GlitchType selectedType = type;
        if (type == GlitchType.Random)
        {
            // Pick any preset except Random & CustomSliders
            selectedType = (GlitchType)Random.Range(1, 6);
        }

        switch (selectedType)
        {
            case GlitchType.BlockShiftOnly:
                ApplyWeightsToMaterial(1.0f, 0.0f, 0.1f, 0.1f, 0.0f);
                break;
            case GlitchType.RGBSplitOnly:
                ApplyWeightsToMaterial(0.0f, 1.0f, 0.2f, 0.0f, 0.0f);
                break;
            case GlitchType.StaticNoiseOnly:
                ApplyWeightsToMaterial(0.0f, 0.1f, 1.0f, 1.0f, 0.0f);
                break;
            case GlitchType.ColorInvertOnly:
                ApplyWeightsToMaterial(0.0f, 0.0f, 0.3f, 0.2f, 1.0f);
                break;
            case GlitchType.FullChaos:
                ApplyWeightsToMaterial(1.0f, 1.0f, 1.0f, 1.0f, 1.0f);
                break;
            case GlitchType.CustomSliders:
                ApplyWeightsToMaterial(customBlockWeight, customRGBSplitWeight, customScanlineWeight, customNoiseWeight, customColorSpikeWeight);
                break;
        }
    }

    private IEnumerator PeriodicGlitchRoutine()
    {
        while (true)
        {
            // Idle phase with base background jitter
            float idleTime = Random.Range(minTimeBetweenBursts, maxTimeBetweenBursts);
            float elapsed = 0f;
            while (elapsed < idleTime)
            {
                // Reset to standard minimal background jitter (very low weights)
                ApplyWeightsToMaterial(0.2f, 0.1f, 0.1f, 0.3f, 0.0f);
                currentIntensity = baseGlitchIntensity + Mathf.PingPong(Time.time * 0.2f, 0.02f);
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            // Set up burst preset
            SetWeightsByPreset(activeGlitchPreset);

            // Trigger intense burst
            float duration = Random.Range(minBurstDuration, maxBurstDuration);
            elapsed = 0f;
            
            while (elapsed < duration)
            {
                float progress = elapsed / duration;
                float intensityMultiplier = 1f;
                if (progress < 0.2f)
                {
                    intensityMultiplier = Mathf.Lerp(0f, 1f, progress / 0.2f);
                }
                else
                {
                    intensityMultiplier = Mathf.Lerp(1f, 0f, (progress - 0.2f) / 0.8f);
                }
                
                float randomSpike = Random.value > 0.4f ? Random.Range(0.4f, 1.0f) : Random.Range(0.15f, 0.35f);
                currentIntensity = randomSpike * intensityMultiplier;
                
                elapsed += Time.deltaTime;
                yield return null;
            }
        }
    }

    /// <summary>
    /// API to manually trigger a specific glitch effect with custom duration and peak intensity.
    /// Very useful for triggering glitches via gameplay events (e.g., getting hit, system hack).
    /// </summary>
    public void TriggerGlitch(GlitchType type, float duration = 0.8f, float peakIntensity = 0.9f)
    {
        if (activeGlitchCoroutine != null)
        {
            StopCoroutine(activeGlitchCoroutine);
        }
        activeGlitchCoroutine = StartCoroutine(ManualGlitchRoutine(type, duration, peakIntensity));
    }

    private IEnumerator ManualGlitchRoutine(GlitchType type, float duration, float peakIntensity)
    {
        SetWeightsByPreset(type);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            float progress = elapsed / duration;
            float intensityMultiplier = 1f;
            if (progress < 0.15f)
            {
                intensityMultiplier = Mathf.Lerp(0f, 1f, progress / 0.15f);
            }
            else
            {
                intensityMultiplier = Mathf.Lerp(1f, 0f, (progress - 0.15f) / 0.85f);
            }

            float randomSpike = Random.value > 0.4f ? Random.Range(0.5f, 1.0f) : Random.Range(0.2f, 0.4f);
            currentIntensity = randomSpike * intensityMultiplier * peakIntensity;

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Return to normal automatic loops if configured, otherwise clean up
        currentIntensity = 0f;
        if (playMode == GlitchMode.PeriodicBursts)
        {
            activeGlitchCoroutine = StartCoroutine(PeriodicGlitchRoutine());
        }
    }

    private void UpdateTextGlitches()
    {
        if (glitchyTexts == null) return;
        
        if (originalTextValues == null || originalTextValues.Length != glitchyTexts.Length)
        {
            originalTextValues = new string[glitchyTexts.Length];
        }
        
        for (int i = 0; i < glitchyTexts.Length; i++)
        {
            if (glitchyTexts[i] == null) continue;
            
            if (currentIntensity > 0.15f)
            {
                // We are in a glitch state.
                // If we haven't cached the original text yet (start of a burst), cache it now.
                if (string.IsNullOrEmpty(originalTextValues[i]))
                {
                    originalTextValues[i] = glitchyTexts[i].text;
                }
                
                string original = originalTextValues[i];
                if (string.IsNullOrEmpty(original)) continue;

                if (Random.value < textGlitchChance)
                {
                    char[] chars = original.ToCharArray();
                    int replacementsCount = Mathf.RoundToInt(chars.Length * currentIntensity * 0.8f);
                    
                    for (int j = 0; j < replacementsCount; j++)
                    {
                        int indexToReplace = Random.Range(0, chars.Length);
                        if (chars[indexToReplace] != ' ' && chars[indexToReplace] != '\n')
                        {
                            chars[indexToReplace] = GlitchChars[Random.Range(0, GlitchChars.Length)];
                        }
                    }
                    
                    glitchyTexts[i].text = new string(chars);
                }
            }
            else
            {
                // No glitch state (normal gameplay).
                // If we have a cached original text, restore it first.
                if (!string.IsNullOrEmpty(originalTextValues[i]))
                {
                    glitchyTexts[i].text = originalTextValues[i];
                    originalTextValues[i] = null; // Clear cache to allow external scripts (Typewriter, Dialogue managers) to update the text freely.
                }
            }
        }
    }

    void OnDestroy()
    {
        if (renderCamera != null)
        {
            renderCamera.targetTexture = null;
        }
        if (displayImage != null)
        {
            displayImage.texture = null;
        }
        if (rt != null)
        {
            rt.Release();
            DestroyImmediate(rt);
        }
    }
}
