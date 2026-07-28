using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class GlitchEffectController : MonoBehaviour
{
    public enum GlitchType
    {
        Random,
        BlockShiftOnly,
        RGBSplitOnly,
        StaticNoiseOnly,
        ColorInvertOnly,
        FullChaos,
        CustomSliders
    }

    public enum GlitchMode
    {
        PeriodicBursts,
        ManualOnly
    }

    [Header("Master Toggle")]
    public bool isEffectActive = true;
    private bool lastEffectActiveState = true;

    [Header("References")]
    public Camera renderCamera;
    public Camera outputCamera;
    public RawImage displayImage;
    public Material glitchMaterial;
    public TMP_Text[] glitchyTexts;
    public GlitchAudioSynthesizer audioSynth;

    [Header("Glitch Mode & Type")]
    public GlitchMode playMode = GlitchMode.PeriodicBursts;
    public GlitchType activeGlitchPreset = GlitchType.Random;

    [Header("Custom Screen Preset Sliders")]
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

    [Header("Screen Glitch Intensity")]
    [Range(0f, 1f)]
    public float baseGlitchIntensity = 0.04f;

    [Range(0f, 1f)]
    public float currentIntensity = 0f;

    [Range(0f, 2f)]
    public float screenIntensityMultiplier = 1f;

    [Header("Audio Glitch Intensity")]
    [Range(0f, 1f)]
    public float currentAudioIntensity = 0f;

    [Range(0f, 2f)]
    public float audioIntensityMultiplier = 0.7f;

    [Range(0f, 1f)]
    public float audioVolume = 0.1f;

    [Range(0f, 2f)]
    public float audioHumStrength = 0.3f;

    [Range(0f, 2f)]
    public float audioBuzzStrength = 0.6f;

    [Range(0f, 2f)]
    public float audioStaticNoiseStrength = 0.8f;

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
        lastEffectActiveState = isEffectActive;

        UpdateRenderTexture();

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

        SyncEffectState();
    }

    void Update()
    {
        if (isEffectActive != lastEffectActiveState)
        {
            SyncEffectState();
            lastEffectActiveState = isEffectActive;
        }

        if (!isEffectActive)
            return;

        if (Screen.width != lastWidth || Screen.height != lastHeight)
        {
            UpdateRenderTexture();
        }

        if (playMode == GlitchMode.ManualOnly || activeGlitchPreset == GlitchType.CustomSliders)
        {
            ApplyWeightsToMaterial(
                customBlockWeight,
                customRGBSplitWeight,
                customScanlineWeight,
                customNoiseWeight,
                customColorSpikeWeight
            );
        }

        ApplyScreenIntensity();
        ApplyAudioIntensity();
        UpdateTextGlitches();
    }

    public void SetGlitchActive(bool active)
    {
        isEffectActive = active;
        SyncEffectState();
    }

    private void SyncEffectState()
    {
        if (renderCamera == null)
            return;

        if (isEffectActive)
        {
            if (rt == null)
                UpdateRenderTexture();

            renderCamera.targetTexture = rt;

            if (outputCamera != null)
                outputCamera.gameObject.SetActive(true);

            if (displayImage != null)
                displayImage.gameObject.SetActive(true);
        }
        else
        {
            renderCamera.targetTexture = null;

            currentIntensity = 0f;
            currentAudioIntensity = 0f;

            if (glitchMaterial != null)
                glitchMaterial.SetFloat("_GlitchIntensity", 0f);

            if (outputCamera != null)
                outputCamera.gameObject.SetActive(false);

            if (displayImage != null)
                displayImage.gameObject.SetActive(false);

            UpdateTextGlitches();
        }

        ApplyAudioIntensity();
    }

    void UpdateRenderTexture()
    {
        if (!isEffectActive)
            return;

        lastWidth = Screen.width;
        lastHeight = Screen.height;

        if (rt != null)
        {
            rt.Release();
        }

        rt = new RenderTexture(lastWidth, lastHeight, 24, RenderTextureFormat.ARGB32);
        rt.name = "GlitchRenderTexture";
        rt.Create();

        if (renderCamera != null)
            renderCamera.targetTexture = rt;

        if (displayImage != null)
            displayImage.texture = rt;
    }

    private void ApplyScreenIntensity()
    {
        if (glitchMaterial == null)
            return;

        float finalScreenIntensity = currentIntensity * screenIntensityMultiplier;
        finalScreenIntensity = Mathf.Clamp01(finalScreenIntensity);

        glitchMaterial.SetFloat("_GlitchIntensity", finalScreenIntensity);
    }

    private void ApplyAudioIntensity()
    {
        if (audioSynth == null)
            return;

        if (!isEffectActive)
        {
            audioSynth.audioIntensity = 0f;
            return;
        }

        float finalAudioIntensity = currentAudioIntensity * audioIntensityMultiplier;
        finalAudioIntensity = Mathf.Clamp01(finalAudioIntensity);

        audioSynth.audioIntensity = finalAudioIntensity;
        audioSynth.volume = audioVolume;
        audioSynth.humStrength = audioHumStrength;
        audioSynth.buzzStrength = audioBuzzStrength;
        audioSynth.staticNoiseStrength = audioStaticNoiseStrength;
    }

    private void ApplyWeightsToMaterial(float b, float rgb, float s, float n, float cs)
    {
        if (glitchMaterial == null)
            return;

        glitchMaterial.SetFloat("_BlockWeight", b);
        glitchMaterial.SetFloat("_RGBSplitWeight", rgb);
        glitchMaterial.SetFloat("_ScanlineWeight", s);
        glitchMaterial.SetFloat("_NoiseWeight", n);
        glitchMaterial.SetFloat("_ColorSpikeWeight", cs);
    }

    private void SetWeightsByPreset(GlitchType type)
    {
        GlitchType selectedType = type;

        if (type == GlitchType.Random)
        {
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
                ApplyWeightsToMaterial(
                    customBlockWeight,
                    customRGBSplitWeight,
                    customScanlineWeight,
                    customNoiseWeight,
                    customColorSpikeWeight
                );
                break;
        }
    }

    private IEnumerator PeriodicGlitchRoutine()
    {
        while (true)
        {
            float idleTime = Random.Range(minTimeBetweenBursts, maxTimeBetweenBursts);
            float elapsed = 0f;

            while (elapsed < idleTime)
            {
                ApplyWeightsToMaterial(0.2f, 0.1f, 0.1f, 0.3f, 0.0f);

                float idleIntensity = baseGlitchIntensity + Mathf.PingPong(Time.time * 0.2f, 0.02f);

                currentIntensity = idleIntensity;
                currentAudioIntensity = idleIntensity;

                elapsed += Time.deltaTime;
                yield return null;
            }

            SetWeightsByPreset(activeGlitchPreset);

            float duration = Random.Range(minBurstDuration, maxBurstDuration);
            elapsed = 0f;

            while (elapsed < duration)
            {
                float progress = elapsed / duration;

                float intensityMultiplier;

                if (progress < 0.2f)
                {
                    intensityMultiplier = Mathf.Lerp(0f, 1f, progress / 0.2f);
                }
                else
                {
                    intensityMultiplier = Mathf.Lerp(1f, 0f, (progress - 0.2f) / 0.8f);
                }

                float randomSpike =
                    Random.value > 0.4f
                        ? Random.Range(0.4f, 1.0f)
                        : Random.Range(0.15f, 0.35f);

                float intensity = randomSpike * intensityMultiplier;

                currentIntensity = intensity;
                currentAudioIntensity = intensity;

                elapsed += Time.deltaTime;
                yield return null;
            }

            currentIntensity = 0f;
            currentAudioIntensity = 0f;
        }
    }

    public void TriggerGlitch(
        GlitchType type,
        float duration = 0.8f,
        float peakIntensity = 0.9f
    )
    {
        TriggerGlitch(type, duration, peakIntensity, peakIntensity);
    }

    public void TriggerGlitch(
        GlitchType type,
        float duration,
        float peakScreenIntensity,
        float peakAudioIntensity
    )
    {
        if (activeGlitchCoroutine != null)
        {
            StopCoroutine(activeGlitchCoroutine);
        }

        activeGlitchCoroutine = StartCoroutine(
            ManualGlitchRoutine(
                type,
                duration,
                peakScreenIntensity,
                peakAudioIntensity
            )
        );
    }

    private IEnumerator ManualGlitchRoutine(
        GlitchType type,
        float duration,
        float peakScreenIntensity,
        float peakAudioIntensity
    )
    {
        SetWeightsByPreset(type);

        float elapsed = 0f;

        while (elapsed < duration)
        {
            float progress = elapsed / duration;

            float intensityMultiplier;

            if (progress < 0.15f)
            {
                intensityMultiplier = Mathf.Lerp(0f, 1f, progress / 0.15f);
            }
            else
            {
                intensityMultiplier = Mathf.Lerp(1f, 0f, (progress - 0.15f) / 0.85f);
            }

            float randomScreenSpike =
                Random.value > 0.4f
                    ? Random.Range(0.5f, 1.0f)
                    : Random.Range(0.2f, 0.4f);

            float randomAudioSpike =
                Random.value > 0.4f
                    ? Random.Range(0.4f, 0.9f)
                    : Random.Range(0.1f, 0.3f);

            currentIntensity = randomScreenSpike * intensityMultiplier * peakScreenIntensity;
            currentAudioIntensity = randomAudioSpike * intensityMultiplier * peakAudioIntensity;

            elapsed += Time.deltaTime;
            yield return null;
        }

        currentIntensity = 0f;
        currentAudioIntensity = 0f;

        if (playMode == GlitchMode.PeriodicBursts)
        {
            activeGlitchCoroutine = StartCoroutine(PeriodicGlitchRoutine());
        }
        else
        {
            activeGlitchCoroutine = null;
        }
    }

    private void UpdateTextGlitches()
    {
        if (glitchyTexts == null)
            return;

        if (originalTextValues == null || originalTextValues.Length != glitchyTexts.Length)
        {
            originalTextValues = new string[glitchyTexts.Length];
        }

        for (int i = 0; i < glitchyTexts.Length; i++)
        {
            if (glitchyTexts[i] == null)
                continue;

            if (currentIntensity > 0.15f)
            {
                if (string.IsNullOrEmpty(originalTextValues[i]))
                {
                    originalTextValues[i] = glitchyTexts[i].text;
                }

                string original = originalTextValues[i];

                if (string.IsNullOrEmpty(original))
                    continue;

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
                if (!string.IsNullOrEmpty(originalTextValues[i]))
                {
                    glitchyTexts[i].text = originalTextValues[i];
                    originalTextValues[i] = null;
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