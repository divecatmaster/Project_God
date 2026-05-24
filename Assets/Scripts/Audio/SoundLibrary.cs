using System.Collections.Generic;
using UnityEngine;

namespace God.Audio
{
    [CreateAssetMenu(fileName = "SoundLibrary", menuName = "Audio/Sound Library")]
    public class SoundLibrary : ScriptableObject
    {
        [SerializeField] private List<SoundData> sounds = new List<SoundData>();
        
        private Dictionary<string, SoundData> _soundDict;

        public void Initialize()
        {
            _soundDict = new Dictionary<string, SoundData>();
            foreach (var sound in sounds)
            {
                if (sound != null && !string.IsNullOrEmpty(sound.soundID))
                {
                    _soundDict[sound.soundID] = sound;
                }
            }
        }

        public SoundData GetSound(string soundID)
        {
            if (_soundDict == null) Initialize();
            
            if (_soundDict.TryGetValue(soundID, out SoundData data))
            {
                return data;
            }
            
            Debug.LogWarning($"[SoundLibrary] Sound ID '{soundID}' not found!");
            return null;
        }
    }
}
