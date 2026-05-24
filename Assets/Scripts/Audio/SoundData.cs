using UnityEngine;
using UnityEngine.Audio;

namespace God.Audio
{
    public enum SoundCategory
    {
        Master,
        BGM,
        SFX,
        UI,
        Opening
    }

    [CreateAssetMenu(fileName = "NewSoundData", menuName = "Audio/Sound Data")]
    public class SoundData : ScriptableObject
    {
        [Header("Identity")]
        public string soundID;
        public SoundCategory category = SoundCategory.SFX;

        [Header("Audio Settings")]
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 1f;
        [Range(0.1f, 3f)] public float pitch = 1f;
        
        [Header("Randomization (SFX Only)")]
        [Range(0f, 0.5f)] public float volumeRandomization = 0f;
        [Range(0f, 0.5f)] public float pitchRandomization = 0f;

        [Header("Spatial Settings")]
        [Range(0f, 1f)] public float spatialBlend = 0f; // 0 for 2D, 1 for 3D

        public float GetRandomVolume() => Mathf.Clamp01(volume + Random.Range(-volumeRandomization, volumeRandomization));
        public float GetRandomPitch() => Mathf.Clamp(pitch + Random.Range(-pitchRandomization, pitchRandomization), 0.1f, 3f);
    }
}
