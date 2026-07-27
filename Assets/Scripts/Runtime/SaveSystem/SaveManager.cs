using System;
using System.IO;
using UnityEngine;
using System.Collections.Generic;

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

    [Serializable]
    public class Global_Save_Data
    {
        public int EndingMask = 0;
        public int LogCount = 0;
        public List<int> GalleryOpenData = new List<int>();
        public string MyName = "";
    }

    [SerializeField] private string globalFileName = "global_save.json";

    private Global_Save_Data _globalData;

    private string GetGlobalSavePath()
    {
        return Path.Combine(
            Application.persistentDataPath,
            globalFileName
        );
    }

    public Global_Save_Data LoadGlobalData()
    {
        string path = GetGlobalSavePath();

        if (!File.Exists(path))
        {
            _globalData = new Global_Save_Data();
            NormalizeGlobalData();
            SaveGlobalData();
            return _globalData;
        }

        try
        {
            string json = File.ReadAllText(path);

            if (string.IsNullOrEmpty(json))
            {
                _globalData = new Global_Save_Data();
                NormalizeGlobalData();
                SaveGlobalData();
                return _globalData;
            }

            _globalData = JsonUtility.FromJson<Global_Save_Data>(json);

            if (_globalData == null)
                _globalData = new Global_Save_Data();

            NormalizeGlobalData();

            return _globalData;
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveManager] Global data load failed: {e.Message}");

            _globalData = new Global_Save_Data();
            NormalizeGlobalData();

            return _globalData;
        }
    }

    public void SaveGlobalData()
    {
        if (_globalData == null)
            _globalData = new Global_Save_Data();

        string json = JsonUtility.ToJson(_globalData, prettyPrint);
        string path = GetGlobalSavePath();

        WriteFileSafe(path, json);

        Debug.Log($"[SaveManager] Global data saved: {path}");
    }

    public int GetEndingMask()
    {
        if (_globalData == null)
            LoadGlobalData();

        return _globalData.EndingMask;
    }

    public void SetEndingMask(int endingMask)
    {
        if (_globalData == null)
            LoadGlobalData();

        _globalData.EndingMask = endingMask;
        SaveGlobalData();
    }

    public int GetLogCount()
    {
        if (_globalData == null)
            LoadGlobalData();

        return _globalData.LogCount;
    }

    public void SetLogCount(int logCount)
    {
        if (_globalData == null)
            LoadGlobalData();

        _globalData.LogCount = logCount;
        SaveGlobalData();
    }

    void NormalizeGlobalData()
    {
        if (_globalData == null)
            _globalData = new Global_Save_Data();

        if (_globalData.GalleryOpenData == null)
            _globalData.GalleryOpenData = new List<int>();

        if (_globalData.MyName == null)
            _globalData.MyName = "";
    }

    public List<int> GetGalleryOpenData()
    {
        if (_globalData == null)
            LoadGlobalData();

        if (_globalData.GalleryOpenData == null)
            _globalData.GalleryOpenData = new List<int>();

        return new List<int>(_globalData.GalleryOpenData);
    }

    public void SetGalleryOpenData(List<int> galleryOpenData)
    {
        if (_globalData == null)
            LoadGlobalData();

        if (galleryOpenData == null)
            galleryOpenData = new List<int>();

        _globalData.GalleryOpenData = new List<int>(galleryOpenData);
        SaveGlobalData();
    }

    public void AddGalleryOpenData(int idx)
    {
        if (idx == 0)
            return;

        if (_globalData == null)
            LoadGlobalData();

        if (_globalData.GalleryOpenData == null)
            _globalData.GalleryOpenData = new List<int>();

        if (_globalData.GalleryOpenData.Contains(idx))
            return;

        _globalData.GalleryOpenData.Add(idx);
        SaveGlobalData();
    }

    public void ResetGlobalData(bool keepLogCount = true)
    {
        if (_globalData == null)
            LoadGlobalData();

        int logCount = 0;

        if (keepLogCount && _globalData != null)
            logCount = _globalData.LogCount;

        _globalData = new Global_Save_Data();

        if (keepLogCount)
            _globalData.LogCount = logCount;

        SaveGlobalData();
    }

    public string GetMyName()
    {
        if (_globalData == null)
            LoadGlobalData();

        if (_globalData == null)
            return "";

        return _globalData.MyName;
    }

    public void SetMyName(string myName)
    {
        if (_globalData == null)
            LoadGlobalData();

        if (_globalData == null)
            _globalData = new Global_Save_Data();

        _globalData.MyName = myName;
        SaveGlobalData();
    }
}