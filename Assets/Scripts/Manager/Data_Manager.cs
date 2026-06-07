using System.Collections.Generic;
using UnityEngine;
using System.Globalization;
using System;
using System.Linq;

public class Data_Manager : MonoBehaviour
{
    public static Data_Manager Instance;
    public static string nextScene;
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

    private void Start()
    {
        InitSettingValue();
        LoadStoryData();
        LoadSelectData();
        LoadSaveData();
        InitNameColor();
    }

    float _playTimer;

    private void Update()
    {
        if (!_startGame) return;

        _playTimer += Time.unscaledDeltaTime;

        if (_playTimer >= 1f)
        {
            _currentPlayTime += TimeSpan.FromSeconds(_playTimer).Ticks;
            _playTimer = 0f;
        }
    }
    //------------------------------------------------------------------------------------------------------------------------------------------------
    #region Story
    Dictionary<int, Story_Data> Story_Dic = new Dictionary<int, Story_Data>();
    public bool IsNewGame { get; private set; } = false;
    public int SaveStory_Index { get; private set; } = 1;
    void LoadStoryData()
    {
        Story_Dic = new Dictionary<int, Story_Data>();
        var data = CSVReader.ReadOriginal("Story");
        for (int i = 0; i < data.Count; i++)
        {
            var newData = new Story_Data();
            newData.Index = (int)data[i]["index"];
            newData.Language_Key = data[i]["language_key"].ToString();
            newData.Language_Production = CSV_Int_Checker(data[i]["language_production"]);
            newData.Name = CSV_Int_Checker(data[i]["name"]);
            newData.Body = data[i]["body"].ToString();
            newData.Face = data[i]["face"].ToString();
            newData.Select_Index = CSV_Int_Checker(data[i]["select_index"]);
            newData.BG = (int)data[i]["bg"];
            newData.Next_Index = CSV_Int_Checker(data[i]["next_index"]);
            newData.Appear_Production = CSV_Int_Checker(data[i]["appear_production"]);
            newData.Appear_Production_Time = CSV_float_Checker(data[i]["appear_production_time"]);
            newData.Auto_Next = data[i]["auto_next"].ToString() == "FALSE" ? false : true;
            newData.My_Name = data[i]["my_name"].ToString() == "FALSE" ? false : true;
            Story_Dic.Add(newData.Index, newData);
        }
    }

    public Story_Data GetStoryData(int index)
    {
        if (Story_Dic.ContainsKey(index))
        {
            return Story_Dic[index];
        }
        return null;
    }

    public void SetNewGame(bool isNew = false)
    {
        IsNewGame = isNew;
    }

    public void SetSaveStory_Index(int index)
    {
        SaveStory_Index = index;
    }

    public Story_Data GetNextSelect(int currentIndex)
    {
        return Story_Dic.Values
        .Where(x => x.Index > currentIndex && x.Select_Index != 0)
        .OrderBy(x => x.Index)
        .FirstOrDefault();
    }
    #endregion
//------------------------------------------------------------------------------------------------------------------------------------------------
    #region Select
    Dictionary<int, Select_Data> Select_Dic = new Dictionary<int, Select_Data>();
    void LoadSelectData()
    {
        Select_Dic = new Dictionary<int, Select_Data>();
        var data = CSVReader.ReadOriginal("Select");
        for (int i = 0; i < data.Count; i++)
        {
            var newData = new Select_Data();
            newData.Index = (int)data[i]["index"];
            var indexData = data[i]["story_index"].ToString();
            var split = indexData.Split('/');
            for (int a = 0; a < split.Length; a++)
            {
                newData.Next_Index.Add(UIUtility.StringToInt(split[a]));
                newData.Language_Key.Add($"select_{newData.Index}_{(a + 1)}");
            }
            Select_Dic.Add(newData.Index, newData);
        }
    }

    public Select_Data GetSelectData(int index)
    {
        if (Select_Dic.ContainsKey(index))
        {
            return Select_Dic[index];
        }
        return null;
    }
    #endregion
    //------------------------------------------------------------------------------------------------------------------------------------------------
    #region SaveData
    Dictionary<int, Save_Data> SaveData_Dic = new Dictionary<int, Save_Data>();
    bool _startGame;
    long _currentPlayTime;
    int _tempLoadIndex;
    void LoadSaveData()
    {
        for (int i = 0; i < 20; i++)
        {
            SaveManager.Instance.Load(i);
        }
    }

    public Dictionary<int, Save_Data> GetAllSaveData()
    {
        return SaveData_Dic;
    }

    public Save_Data GetSaveData(int slotIndex)
    {
        if (SaveData_Dic.ContainsKey(slotIndex))
        {
            return SaveData_Dic[slotIndex];
        }
        return null;
    }

    public void SetData(Save_Data data)
    {
        SaveData_Dic[data.SlotIndex] = data;
    }

    public void SetSaveData(Save_Data data)
    {
        SaveData_Dic[data.SlotIndex] = data;
        SaveManager.Instance.Save(data.SlotIndex);
    }

    public void RemoveSaveData(int slotIdx)
    {
        SaveData_Dic.Remove(slotIdx);
        SaveManager.Instance.Delete(slotIdx);
    }

    public void StartTimer(TimeSpan savedTime)
    {
        _startGame = true;
        _currentPlayTime = savedTime.Ticks;
        _playTimer = 0f;
    }

    public void StopTimer()
    {
        _startGame = false;
        _playTimer = 0f;
    }

    public TimeSpan GetPlayTime()
    {
        return new TimeSpan(_currentPlayTime);
    }

    public void Set_TempIndex(int slotIndex)
    {
        _tempLoadIndex = slotIndex;
    }

    public Save_Data Get_TempSavedata()
    {
        if (SaveData_Dic.ContainsKey(_tempLoadIndex))
        {
            return SaveData_Dic[_tempLoadIndex];
        }
        return null;
    }
    #endregion
    //------------------------------------------------------------------------------------------------------------------------------------------------
    #region NameColor
    Dictionary<int, Color> NameColor_Dic = new Dictionary<int, Color>();
    void InitNameColor()
    {
        NameColor_Dic = new Dictionary<int, Color>();

        var data = CSVReader.ReadOriginal("NameColor");
        for (int i = 0; i < data.Count; i++)
        {
            var idx = (int)data[i]["index"];
            var color = data[i]["color_code"].ToString();
            var realColor = UIUtility.HexToColor(color);
            NameColor_Dic.Add(idx, realColor);
        }
    }

    public Color GetNameColor(int idx)
    {
        if (NameColor_Dic.ContainsKey(idx))
        {
            return NameColor_Dic[idx];    
        }
        return Color.white;
    }
    #endregion
    //------------------------------------------------------------------------------------------------------------------------------------------------
    #region CSV_Util
    int CSV_Int_Checker(object obj)
    {
        if (obj == null)
            return 0;

        string temp = obj.ToString();

        if (string.IsNullOrWhiteSpace(temp))
            return 0;

        return int.TryParse(temp, out int result) ? result : 0;
    }

    float CSV_float_Checker(object obj)
    {
        if (obj == null)
        {
            return 0f;
        }

        string temp = obj.ToString();

        if (string.IsNullOrWhiteSpace(temp))
        {
            return 0f;
        }

        if (float.TryParse(temp, NumberStyles.Float, CultureInfo.InvariantCulture, out float result))
        {
            return result;
        }

        return 0f;
    }
    #endregion
    //------------------------------------------------------------------------------------------------------------------------------------------------
    #region Setting
    public int Sound_BG { get; private set; } = 100;
    public int Sound_Effect { get; private set; } = 100;
    public int Sound_UI { get; private set; } = 100;
    public int TextSpeed { get; private set; } = 50;
    public float AutoSpeed { get; private set; } = 3f;
    public int Production_Effect { get; private set; } = 1;
    public int ScreenMode { get; private set; } = 2;
    public int Screen_Width { get; private set; } = 1920;
    public int Screen_Height { get; private set; } = 1080;

    void InitSettingValue()
    {
        TextSpeed = PlayerPrefs.GetInt("TextSpeed", 50);
        AutoSpeed = PlayerPrefs.GetFloat("AutoSpeed", 3f);
        Production_Effect = PlayerPrefs.GetInt("Production_Effect", 1);
        Sound_BG = PlayerPrefs.GetInt("Sound_BG", 100);
        Sound_Effect = PlayerPrefs.GetInt("Sound_Effect", 100);
        Sound_UI = PlayerPrefs.GetInt("Sound_UI", 100);
        ScreenMode = PlayerPrefs.GetInt("ScreenMode", 0);
        Screen_Width = PlayerPrefs.GetInt("Screen_Width", 1920);
        Screen_Height = PlayerPrefs.GetInt("Screen_Height", 1080);
    }

    public void SetSound_BG(int value)
    {
        if (Sound_BG == value) return;
        
        Sound_BG = value;
        PlayerPrefs.SetInt("Sound_BG", value);
    }

    public void SetSound_Effect(int value)
    {
        if (Sound_Effect == value) return;
        
        Sound_Effect = value;
        PlayerPrefs.SetFloat("Sound_Effect", value);
    }

    public void SetSound_UI(int value)
    {
        if (Sound_UI == value) return;

        Sound_UI = value;
        PlayerPrefs.SetInt("Sound_UI", value);
    }

    public void SetTextSpeed(int value)
    {
        if (TextSpeed == value) return;
        
        TextSpeed = value;
        PlayerPrefs.SetInt("TextSpeed", value);
    }

    public void SetAutoSpeed(float value)
    {
        if (TextSpeed == value) return;
        
        AutoSpeed = value;
        PlayerPrefs.SetFloat("AutoSpeed", value);
    }

    public void SetProduction_Effect(int value)
    {
        if (Production_Effect == value) return;

        Production_Effect = value;
        PlayerPrefs.SetInt("Production_Effect", value);
    }

    public void SetScreenMode(int value)
    {
        if (ScreenMode == value) return;

        ScreenMode = value;
        PlayerPrefs.SetInt("ScreenMode", value);
    }

    public void SetScreenResolution(int width, int height)
    {
        if (Screen_Width == width && Screen_Height == height) return;

        Screen_Width = width;
        Screen_Height = height;
        PlayerPrefs.SetInt("Screen_Width", width);
        PlayerPrefs.SetInt("Screen_Height", height);
    }
    #endregion
}

[Serializable]
public class Story_Data
{
    public int Index;
    public string Language_Key;
    public int Language_Production;
    public int Name;
    public string Body;
    public string Face;
    public int Select_Index;
    public int BG;
    public int Next_Index;
    public int Appear_Production;
    public float Appear_Production_Time;
    public bool Auto_Next;
    public bool My_Name;
}

public class Select_Data
{
    public int Index;
    public List<string> Language_Key = new List<string>();
    public List<int> Next_Index = new List<int>();
}

public class Save_Data
{
    public int SlotIndex;
    public int StoryIndex = -1;

    public long SaveDateTicks;
    public long PlayTimeTicks;

    public DateTime SaveDate
    {
        get => SaveDateTicks <= 0 ? DateTime.MinValue : new DateTime(SaveDateTicks);
        set => SaveDateTicks = value.Ticks;
    }

    public TimeSpan PlayTime
    {
        get => PlayTimeTicks <= 0 ? TimeSpan.Zero : new TimeSpan(PlayTimeTicks);
        set => PlayTimeTicks = value.Ticks;
    }
}