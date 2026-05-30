using System;
using System.Collections.Generic;
using UnityEngine;

namespace DiveCat.SaveSystem
{
    /// <summary>
    /// Metadata for a save slot, used for the Load/Save menu.
    /// </summary>
    [Serializable]
    public class SaveMetadata
    {
        public string SlotName;
        public long Timestamp;
        public string Version;
        public string DisplayName;

        public DateTime GetDateTime() => DateTime.FromBinary(Timestamp);
    }

    /// <summary>
    /// The actual game data stored in a save file.
    /// </summary>
    [Serializable]
    public class SaveData
    {
        public int Version = 1;
        public long CreatedAt;
        public long UpdatedAt;

        // Example Fields
        public PlayerData Player = new PlayerData();
        public WorldData World = new WorldData();
        public SettingsData Settings = new SettingsData();

        public SaveData()
        {
            CreatedAt = DateTime.UtcNow.ToBinary();
            UpdatedAt = CreatedAt;
        }
    }

    [Serializable]
    public class PlayerData
    {
        public string Name = "New Player";
        public int Level = 1;
        public int XP = 0;
        public int Gold = 100;
        public int PremiumCurrency = 0;
        public List<string> Inventory = new List<string>();
        public EquipmentData Equipment = new EquipmentData();
    }

    [Serializable]
    public class EquipmentData
    {
        public string WeaponId = "starter_sword";
        public string ArmorId = "cloth_tunic";
    }

    [Serializable]
    public class WorldData
    {
        public string CurrentScene = "MainScene";
        public Vector3 LastPosition = Vector3.zero;
        public List<string> CompletedQuestIds = new List<string>();
    }

    [Serializable]
    public class SettingsData
    {
        public float MasterVolume = 1.0f;
        public float MusicVolume = 1.0f;
        public float SfxVolume = 1.0f;
        public int GraphicsQuality = 2; // High
        public bool VSync = true;
    }
}
