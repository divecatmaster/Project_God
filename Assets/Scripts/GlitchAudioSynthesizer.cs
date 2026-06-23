using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class GlitchAudioSynthesizer : MonoBehaviour
{
    private AudioSource audioSource;
    private double phase;
    private double sampleRate;
    private System.Random systemRandom;
    
    [Range(0f, 1f)]
    public float audioIntensity = 0f;
    
    public float humFrequency = 60f; // low frequency electrical hum
    public float buzzFrequency = 120f; // higher buzz frequency
    
    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f; // 2D audio
        audioSource.loop = true;
        sampleRate = AudioSettings.outputSampleRate;
        systemRandom = new System.Random();
    }
    
    void Start()
    {
        // We need to play the audio source to trigger OnAudioFilterRead
        audioSource.Play();
    }
    
    void OnAudioFilterRead(float[] data, int channels)
    {
        if (audioIntensity <= 0.001f)
        {
            // Clear buffer
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = 0f;
            }
            return;
        }
        
        double phaseIncrementHum = (2.0 * System.Math.PI * humFrequency) / sampleRate;
        double phaseIncrementBuzz = (2.0 * System.Math.PI * buzzFrequency) / sampleRate;
        
        for (int i = 0; i < data.Length; i += channels)
        {
            phase += phaseIncrementHum;
            if (phase > 2.0 * System.Math.PI) phase -= 2.0 * System.Math.PI;
            
            // Generate low frequency hum
            float hum = (float)System.Math.Sin(phase);
            
            // Generate harsh buzzing (sawtooth/square wave)
            float buzz = (float)(System.Math.Sin(phase * 2.0) > 0.0 ? 1.0 : -1.0) * 0.3f;
            
            // Generate white noise (digital static) using System.Random
            float noise = (float)(systemRandom.NextDouble() - 0.5) * 2.0f;
            
            // Combine based on glitch intensity
            // Lower intensity: subtle hum and crackle. Higher intensity: loud static and buzz.
            float combinedSignal = hum * 0.15f + buzz * 0.1f + noise * 0.35f;
            
            // Scale by intensity
            float finalSample = combinedSignal * audioIntensity * 0.2f; // Keep volume reasonable
            
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
