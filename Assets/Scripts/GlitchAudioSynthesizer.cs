using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class GlitchAudioSynthesizer : MonoBehaviour
{
    private AudioSource audioSource;
    private double phase;
    private double sampleRate;
    private System.Random systemRandom;

    [Header("Master")]
    [Range(0f, 1f)]
    public float audioIntensity = 0f;

    [Range(0f, 1f)]
    public float volume = 0.15f;

    private float sfxVolume = 1f;

    [Header("Tone Frequency")]
    public float humFrequency = 60f;   // 낮은 웅웅거림
    public float buzzFrequency = 120f; // 전기 잡음

    [Header("Noise Strength")]
    [Range(0f, 2f)]
    public float humStrength = 1f;

    [Range(0f, 2f)]
    public float buzzStrength = 1f;

    [Range(0f, 2f)]
    public float staticNoiseStrength = 1f;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
        audioSource.loop = true;

        sampleRate = AudioSettings.outputSampleRate;
        systemRandom = new System.Random();
    }

    void Start()
    {
        // OnAudioFilterRead를 작동시키기 위해 재생 상태로 둠.
        audioSource.Play();
    }

    void Update()
    {
        if (Data_Manager.Instance != null)
        {
            sfxVolume = Data_Manager.Instance.Sound_Effect * 0.01f;
        }
    }

    public void SetIntensity(float value)
    {
        audioIntensity = Mathf.Clamp01(value);
    }

    public void SetVolume(float value)
    {
        volume = Mathf.Clamp01(value);
    }

    public void SetStaticNoiseStrength(float value)
    {
        staticNoiseStrength = Mathf.Clamp(value, 0f, 2f);
    }

    public void SetHumStrength(float value)
    {
        humStrength = Mathf.Clamp(value, 0f, 2f);
    }

    public void SetBuzzStrength(float value)
    {
        buzzStrength = Mathf.Clamp(value, 0f, 2f);
    }

    void OnAudioFilterRead(float[] data, int channels)
    {
        if (audioIntensity <= 0.001f || volume <= 0.001f || sfxVolume <= 0.001f)
        {
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = 0f;
            }

            return;
        }

        double phaseIncrementHum = (2.0 * System.Math.PI * humFrequency) / sampleRate;

        for (int i = 0; i < data.Length; i += channels)
        {
            phase += phaseIncrementHum;

            if (phase > 2.0 * System.Math.PI)
                phase -= 2.0 * System.Math.PI;

            float hum = (float)System.Math.Sin(phase);

            float buzzPhaseMultiplier = 2f;

            if (humFrequency > 0.001f)
                buzzPhaseMultiplier = buzzFrequency / humFrequency;

            float buzz = (float)(System.Math.Sin(phase * buzzPhaseMultiplier) > 0.0 ? 1.0 : -1.0);

            float noise = (float)(systemRandom.NextDouble() - 0.5) * 2.0f;

            float combinedSignal =
                hum * 0.15f * humStrength +
                buzz * 0.1f * buzzStrength +
                noise * 0.35f * staticNoiseStrength;

            float finalSample = combinedSignal * audioIntensity * volume * sfxVolume;

            finalSample = Mathf.Clamp(finalSample, -1f, 1f);

            for (int c = 0; c < channels; c++)
            {
                if (i + c < data.Length)
                {
                    data[i + c] = finalSample;
                }
            }
        }
    }
}