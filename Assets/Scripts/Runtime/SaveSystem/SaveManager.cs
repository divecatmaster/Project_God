using System;
using System.IO;
using UnityEngine;

public sealed class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    [Header("Save Settings")]
    [SerializeField] private string fileNamePrefix = "save";
    [SerializeField] private bool prettyPrint = true;
    [SerializeField] private bool autoSaveOnQuit = true;

    private int _currentSlotIndex = 0;

    public int CurrentSlotIndex => _currentSlotIndex;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private string GetSavePath(int slotIndex)
    {
        return Path.Combine(
            Application.persistentDataPath,
            $"{fileNamePrefix}_{slotIndex}.json"
        );
    }

    public void SetSlot(int slotIndex)
    {
        _currentSlotIndex = Mathf.Max(0, slotIndex);
    }

    public void Save()
    {
        Save(_currentSlotIndex);
    }

    public void Save(int slotIndex)
    {
        if (Data_Manager.Instance == null)
        {
            Debug.LogError("[SaveManager] Data_Manager.instance is null.");
            return;
        }

        Save_Data data = Data_Manager.Instance.GetSaveData(slotIndex);

        if (data == null)
        {
            Debug.LogError("[SaveManager] SaveData is null.");
            return;
        }

        data.SlotIndex = slotIndex;
        data.SaveDate = DateTime.Now;

        string json = JsonUtility.ToJson(data, prettyPrint);
        string path = GetSavePath(slotIndex);

        WriteFileSafe(path, json);

        Debug.Log($"[SaveManager] Saved: {path}");
    }

    public bool Load()
    {
        return Load(_currentSlotIndex);
    }

    public bool Load(int slotIndex)
    {
        string path = GetSavePath(slotIndex);

        if (!File.Exists(path))
        {
            Debug.LogWarning($"[SaveManager] Save file not found: {path}");
            return false;
        }

        try
        {
            string json = File.ReadAllText(path);

            if (string.IsNullOrEmpty(json))
            {
                Debug.LogError("[SaveManager] Save file is empty.");
                return false;
            }

            Save_Data loadedData = JsonUtility.FromJson<Save_Data>(json);

            if (loadedData == null)
            {
                Debug.LogError("[SaveManager] Failed to deserialize save data.");
                return false;
            }

            if (Data_Manager.Instance == null)
            {
                Debug.LogError("[SaveManager] Data_Manager.instance is null.");
                return false;
            }

            Data_Manager.Instance.SetData(loadedData);
            _currentSlotIndex = slotIndex;

            Debug.Log($"[SaveManager] Loaded: {path}");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveManager] Load failed: {e.Message}");
            return false;
        }
    }

    public bool Exists(int slotIndex)
    {
        return File.Exists(GetSavePath(slotIndex));
    }

    public void Delete(int slotIndex)
    {
        string path = GetSavePath(slotIndex);

        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log($"[SaveManager] Deleted: {path}");
        }
    }

    private void WriteFileSafe(string path, string json)
    {
        string tempPath = path + ".tmp";
        string backupPath = path + ".bak";

        File.WriteAllText(tempPath, json);

        if (File.Exists(path))
        {
            if (File.Exists(backupPath))
                File.Delete(backupPath);

            File.Move(path, backupPath);
        }

        File.Move(tempPath, path);
    }

    private void OnApplicationQuit()
    {
        if (autoSaveOnQuit)
            Save();
    }
}