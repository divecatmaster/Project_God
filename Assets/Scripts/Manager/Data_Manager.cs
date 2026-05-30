using System.Collections.Generic;
using UnityEngine;
using System.Globalization;
using System;

public class Data_Manager : MonoBehaviour
{
    public static Data_Manager Instance;
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
        LoadStoryData();
        LoadSelectData();
        LoadSaveData();
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

    public void SetNewGame()
    {
        IsNewGame = true;
    }

    public void SetSaveStory_Index(int index)
    {
        SaveStory_Index = index;
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

    public void SetSaveData(Save_Data data)
    {
        SaveData_Dic[data.SlotIndex] = data;
    }

    public void StartTimer(TimeSpan savedTime)
    {
        _startGame = true;
        _currentPlayTime = savedTime.Ticks;
        _playTimer = 0f;
    }

    public TimeSpan GetPlayTime()
    {
        return new TimeSpan(_currentPlayTime);
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
}

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