using System.Collections.Generic;
using UnityEngine;
using System.Globalization;
using System;
using System.Linq;
using UnityEngine.SceneManagement;
using God.Audio;

public class Data_Manager : MonoBehaviour
{
    public static Data_Manager Instance;
    public static string nextScene;
    public bool IsInit = false;
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
        //Init();
    }

    public void Init()
    {
        IsInit = false;
        SetLanguage();
        InitSettingValue();
        //ScreenSettingUtility.ApplySavedScreen();

        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.LoadVolumeSettings();
        }
        else
        {
            Debug.LogWarning("SoundManager.Instance가 없습니다.");
        }
        LoadStoryData();
        LoadSelectData();
        LoadSaveData();
        InitNameColor();
        InitGallery();
        IsInit = true;
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
    #region Language
    void SetLanguage()
    {
        LanguageManager.Instance.SetLanguage(GetLanguageType());
    }

    LanguageType GetLanguageType()
    {
        LanguageType type = LanguageType.EN;

        var _save = PlayerPrefs.GetInt("SavedLanguage", 0);
        if (_save == 0)
        {
            SystemLanguage language = Application.systemLanguage;
            switch (language)
            {
                case SystemLanguage.Korean: type = LanguageType.KR; break;
                case SystemLanguage.English: type = LanguageType.EN; break;
                case SystemLanguage.Japanese: type = LanguageType.JA; break;
                case SystemLanguage.ChineseSimplified: type = LanguageType.CN; break;
                default: type = LanguageType.EN; break;
            }
            SaveLanguage(type);
        }
        else
        {
            type = (LanguageType)_save;
        }

        return type;
    }

    public void SaveLanguage(LanguageType type)
    {
        PlayerPrefs.SetInt("SavedLanguage", (int)type);
    }
    #endregion
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

            var tempProduction = data[i]["appear_production"].ToString();
            if (!string.IsNullOrEmpty(tempProduction))
            {
                var splitProduction = tempProduction.Split('/');
                for (int a = 0; a < splitProduction.Length; a++)
                {
                    newData.Appear_Production.Add(UIUtility.StringToInt(splitProduction[a]));
                }
            }

            var tempProductionTIme = data[i]["appear_production_time"].ToString();
            if (!string.IsNullOrEmpty(tempProductionTIme))
            {
                var splitProductionTime = tempProductionTIme.Split('/');
                for (int a = 0; a < splitProductionTime.Length; a++)
                {
                    newData.Appear_Production_Time.Add(CSV_float_Checker(splitProductionTime[a]));
                }
            }

            var tempProductionValue = data[i]["appear_production_value"].ToString();
            if (!string.IsNullOrEmpty(tempProductionValue))
            {
                var splitProductionTime = tempProductionValue.Split('/');
                for (int a = 0; a < splitProductionTime.Length; a++)
                {
                    newData.Appear_Production_Value.Add(CSV_float_Checker(splitProductionTime[a]));
                }
            }

            newData.Auto_Next = data[i]["auto_next"].ToString() == "FALSE" ? false : true;
            newData.My_Name = data[i]["my_name"].ToString() == "FALSE" ? false : true;
            newData.Gallery = CSV_Int_Checker(data[i]["gallery"]);

            newData.BGM = data[i]["bgm"].ToString();
            newData.BGM_Fade_Time = CSV_float_Checker(data[i]["bgm_fade_time"].ToString());

            var tempSFX = data[i]["sfx"].ToString();
            if (!string.IsNullOrEmpty(tempSFX))
            {
                var splitProduction = tempSFX.Split('/');
                for (int a = 0; a < splitProduction.Length; a++)
                {
                    newData.SFX.Add(splitProduction[a]);
                }
            }

            var tempSFX_Type = data[i]["sfx_type"].ToString();
            if (!string.IsNullOrEmpty(tempSFX_Type))
            {
                var splitProduction = tempSFX_Type.Split('/');
                for (int a = 0; a < splitProduction.Length; a++)
                {
                    newData.SFX_Type.Add(CSV_Int_Checker(splitProduction[a]));
                }
            }

            var temp_cg_production = data[i]["cg_production"].ToString();
            if (!string.IsNullOrEmpty(temp_cg_production))
            {
                var splitProduction = temp_cg_production.Split('/');
                for (int a = 0; a < splitProduction.Length; a++)
                {
                    newData.cg_production.Add(splitProduction[a]);
                }
            }

            newData.Ending = CSV_Int_Checker(data[i]["ending"]);

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

    public List<Story_Data> GetStoryData(int start, int end)
    {
        if (start > end)
        {
            int temp = start;
            start = end;
            end = temp;
        }

        return Story_Dic.Values
            .Where(x => x.Index >= start && x.Index <= end)
            .OrderBy(x => x.Index)
            .ToList();
    }

    public void SetNewGame(bool isNew = false)
    {
        IsNewGame = isNew;
        if (isNew)
        {
            _tempSelectIndex = Enumerable.Repeat(-1, 9).ToArray();
        }
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

    public List<Story_Data> GetLogData(int currentIndex)
    {
        List<Story_Data> result = new List<Story_Data>();

        // currentIndex에서 Next_Index를 거꾸로 추적해서 실제 루트상 가장 가까운 선택지 찾기
        Story_Data prevSelect = GetPrevSelectDataByReverseTrace(currentIndex);

        // 이전 선택지가 없으면 처음부터 현재 인덱스까지 순서대로 반환
        if (prevSelect == null)
        {
            return Story_Dic.Values
                .Where(x => x.Index <= currentIndex)
                .OrderBy(x => x.Index)
                .ToList();
        }

        int startIndex = prevSelect.Index;
        int selectIndex = prevSelect.Select_Index;

        // 선택지 문장 자체 추가
        if (Story_Dic.ContainsKey(startIndex))
        {
            result.Add(Story_Dic[startIndex]);
        }

        // currentIndex가 선택지 문장 자체면 여기서 끝
        if (startIndex >= currentIndex)
        {
            return result;
        }

        Select_Data selectData = GetSelectData(selectIndex);

        if (selectData == null)
        {
            return result;
        }

        int savedSelectIndex = GetSavedSelectData(selectIndex);

        // 아직 선택 저장값이 없거나 잘못된 경우
        if (savedSelectIndex < 0 || savedSelectIndex >= selectData.Next_Index.Count)
        {
            return result;
        }

        int nextIndex = selectData.Next_Index[savedSelectIndex];

        HashSet<int> visited = new HashSet<int>();

        while (true)
        {
            if (nextIndex <= 0)
                break;

            if (!Story_Dic.ContainsKey(nextIndex))
                break;

            // 무한루프 방지
            if (visited.Contains(nextIndex))
                break;

            visited.Add(nextIndex);

            Story_Data data = Story_Dic[nextIndex];
            result.Add(data);

            if (nextIndex >= currentIndex)
                break;

            nextIndex = data.Next_Index;
        }

        return result;
    }

    Story_Data GetPrevSelectDataByReverseTrace(int currentIndex)
    {
        if (!Story_Dic.ContainsKey(currentIndex))
            return null;

        Story_Data currentData = Story_Dic[currentIndex];

        // 현재 데이터 자체가 선택지면 바로 반환
        if (currentData.Select_Index != 0)
            return currentData;

        int traceIndex = currentIndex;
        HashSet<int> visited = new HashSet<int>();

        while (true)
        {
            if (traceIndex <= 0)
                break;

            if (visited.Contains(traceIndex))
                break;

            visited.Add(traceIndex);

            // 선택지 데이터 중에서 저장된 선택 루트가 현재 traceIndex로 이어지는 것 찾기
            Story_Data prevSelect = Story_Dic.Values
                .Where(x => x.Select_Index != 0 && IsSelectedNextIndex(x.Select_Index, traceIndex))
                .OrderByDescending(x => x.Index)
                .FirstOrDefault();

            if (prevSelect != null)
                return prevSelect;

            // 일반 Story_Data의 Next_Index가 현재 traceIndex인 이전 데이터 찾기
            Story_Data prevStory = Story_Dic.Values
                .Where(x => x.Next_Index == traceIndex)
                .OrderByDescending(x => x.Index)
                .FirstOrDefault();

            if (prevStory == null)
                break;

            traceIndex = prevStory.Index;
        }

        return null;
    }

    bool IsSelectedNextIndex(int selectIndex, int targetIndex)
    {
        Select_Data selectData = GetSelectData(selectIndex);

        if (selectData == null)
            return false;

        int savedSelectIndex = GetSavedSelectData(selectIndex);

        if (savedSelectIndex < 0 || savedSelectIndex >= selectData.Next_Index.Count)
            return false;

        return selectData.Next_Index[savedSelectIndex] == targetIndex;
    }

    public Story_Data GetBeforeStory(int currentIndex)
    {
        return Story_Dic.Values
        .Where(x => x.Index < currentIndex)
        .OrderByDescending(x => x.Index)
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
    int[] _tempSelectIndex = Enumerable.Repeat(-1, 9).ToArray();
    bool _startGame;
    long _currentPlayTime;
    int _tempLoadIndex;
    void LoadSaveData()
    {
        SaveData_Dic = new Dictionary<int, Save_Data>();

        if (SaveManager.Instance == null)
        {
            Debug.LogError("SaveManager.Instance가 없습니다.");
            return;
        }

        for (int i = 1; i <= 20; i++)
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
        var tempData = data;
        tempData.SelectIndex = _tempSelectIndex.ToArray();
        SaveData_Dic[data.SlotIndex] = tempData;
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

    public void AddSelect(int idx, int num)
    {
        _tempSelectIndex[idx] = num;
    }

    public void SetSelectData(int[] data)
    {
        _tempSelectIndex = data.ToArray();
    }

    public int GetSavedSelectData(int idx)
    {
        return _tempSelectIndex[idx - 1];
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
    #region Gallery
    Dictionary<int, Gallery_Data> Gallery_Dic = new Dictionary<int, Gallery_Data>();
    List<int> Gallery_OpenData = new List<int>();
    void InitGallery()
    {
        Gallery_Dic = new Dictionary<int, Gallery_Data>();
        Gallery_OpenData = new List<int>();

        var data = CSVReader.ReadOriginal("Gallery");

        for (int i = 0; i < data.Count; i++)
        {
            var newData = new Gallery_Data();
            newData.Index = (int)data[i]["index"];
            newData.Group = (int)data[i]["group"];
            newData.BG = (int)data[i]["bg"];
            newData.TextKey = data[i]["text"].ToString();
            newData.Music = data[i]["music"].ToString();
            newData.Start = CSV_Int_Checker(data[i]["start"]);
            newData.End = CSV_Int_Checker(data[i]["end"]);
            newData.Production = CSV_Int_Checker(data[i]["production"]);

            Gallery_Dic.Add(newData.Index, newData);
        }

        if (SaveManager.Instance != null)
        {
            Gallery_OpenData = SaveManager.Instance.GetGalleryOpenData();
        }
    }

    public int GetGalleryPercent()
    {
        if (Gallery_Dic == null || Gallery_Dic.Count == 0)
            return 0;

        int totalCount = Gallery_Dic.Count;
        int openCount = Gallery_OpenData.Count;

        return Mathf.RoundToInt((float)openCount / totalCount * 100f);
    }

    public List<Gallery_Data> GetGalleryData()
    {
        return Gallery_Dic.Values
            .OrderBy(x => x.Index)
            .ToList();
    }

    public List<Gallery_Data> GetGalleryGroupData()
    {
        return Gallery_Dic.Values
            .GroupBy(x => x.Group)
            .Select(g => g.OrderBy(x => x.Index).First())
            .OrderBy(x => x.Index)
            .ToList();
    }

    public List<Gallery_Data> GetGalleryGroupData(int group)
    {
        return Gallery_Dic.Values
            .Where(x => x.Group == group)
            .OrderBy(x => x.Index)
            .ToList();
    }

    public bool IsOpenGallery(int idx)
    {
        return Gallery_OpenData.Contains(idx);
    }

    public void AddGallery(int idx)
    {
        if (idx == 0)
            return;

        if (!Gallery_Dic.ContainsKey(idx))
        {
            Debug.LogWarning($"등록되지 않은 갤러리 idx입니다: {idx}");
            return;
        }

        if (Gallery_OpenData.Contains(idx))
            return;

        Gallery_OpenData.Add(idx);

        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.SetGalleryOpenData(Gallery_OpenData);
        }
        else
        {
            Debug.LogError("SaveManager.Instance가 없습니다.");
        }

        if (Gallery_OpenData.Count >= 20)
        {
            SteamAchievementManager.Unlock(SteamAchievementManager.Achievement6);
        }
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

    float CSV_float_Checker(string str)
    {
        if (string.IsNullOrWhiteSpace(str))
        {
            return 0f;
        }

        if (float.TryParse(
            str,
            NumberStyles.Float,
            CultureInfo.InvariantCulture,
            out float result))
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
    //public int Production_Effect { get; private set; } = 1;
    public int ScreenMode { get; private set; } = 2;
    public int Screen_Width { get; private set; } = 1920;
    public int Screen_Height { get; private set; } = 1080;
    public string MyName { get; private set; } = "";

    void InitSettingValue()
    {
        TextSpeed = PlayerPrefs.GetInt("TextSpeed", 50);
        AutoSpeed = PlayerPrefs.GetFloat("AutoSpeed", 3f);

        Sound_BG = PlayerPrefs.GetInt("Sound_BG", 100);
        Sound_Effect = PlayerPrefs.GetInt("Sound_Effect", 100);
        Sound_UI = PlayerPrefs.GetInt("Sound_UI", 100);

        // 처음 실행이면 메인 모니터 해상도 사용
        bool hasScreenSetting =
            PlayerPrefs.HasKey("ScreenMode") &&
            PlayerPrefs.HasKey("Screen_Width") &&
            PlayerPrefs.HasKey("Screen_Height");

        if (!hasScreenSetting)
        {
            ScreenMode = 2; // 테두리 없는 창 모드

            Screen_Width = Display.main.systemWidth;
            Screen_Height = Display.main.systemHeight;

            PlayerPrefs.SetInt("ScreenMode", ScreenMode);
            PlayerPrefs.SetInt("Screen_Width", Screen_Width);
            PlayerPrefs.SetInt("Screen_Height", Screen_Height);
            PlayerPrefs.Save();
        }
        else
        {
            ScreenMode = PlayerPrefs.GetInt("ScreenMode", 2);
            Screen_Width = PlayerPrefs.GetInt("Screen_Width", Display.main.systemWidth);
            Screen_Height = PlayerPrefs.GetInt("Screen_Height", Display.main.systemHeight);
        }

        MyName = PlayerPrefs.GetString("MyName", "");
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
        PlayerPrefs.SetInt("Sound_Effect", value);
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
        if (Mathf.Approximately(AutoSpeed, value))
            return;

        AutoSpeed = value;
        PlayerPrefs.SetFloat("AutoSpeed", value);
    }

    // public void SetProduction_Effect(int value)
    // {
    //     if (Production_Effect == value) return;

    //     Production_Effect = value;
    //     PlayerPrefs.SetInt("Production_Effect", value);
    // }

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

    public void SetMyName(string name)
    {
        if (MyName == name) return;

        MyName = name;
        PlayerPrefs.SetString("MyName", name);
    }

    public bool HasFinalConsonant(string text)
    {
        if (string.IsNullOrEmpty(text))
            return false;

        char lastChar = text[text.Length - 1];

        // 한글 완성형 범위: 가 ~ 힣
        if (lastChar < '가' || lastChar > '힣')
            return false;

        int unicode = lastChar - '가';

        // 종성 인덱스
        int jongseong = unicode % 28;

        return jongseong != 0;
    }
    #endregion

    #region Reset
    public void ResetData()
    {
        Gallery_OpenData = new List<int>();
        _logCount = -1;

        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.ResetGlobalData();

            SaveData_Dic = new Dictionary<int, Save_Data>();

            for (int i = 1; i < 21; i++)
            {
                SaveManager.Instance.Delete(i);
            }

            LoadSaveData();
        }
    }
    #endregion

    #region Achieve
    int _logCount = -1;

    public void AddLogCount()
    {
        if (SaveManager.Instance == null)
        {
            Debug.LogError("SaveManager.Instance가 없습니다.");
            return;
        }

        if (_logCount < 0)
        {
            _logCount = SaveManager.Instance.GetLogCount();
        }

        _logCount++;

        SaveManager.Instance.SetLogCount(_logCount);

        if (_logCount >= 100)
        {
            SteamAchievementManager.Unlock(SteamAchievementManager.Achievement7);
        }
    }

    const int EndingCount = 3;

    public void AddEndingCount(int idx)
    {
        idx = Mathf.Abs(idx);

        if (idx < 1 || idx > EndingCount)
        {
            Debug.LogWarning($"잘못된 엔딩 idx입니다: {idx}");
            return;
        }

        if (SaveManager.Instance == null)
        {
            Debug.LogError("SaveManager.Instance가 없습니다.");
            return;
        }

        int endingMask = SaveManager.Instance.GetEndingMask();

        // idx 1 = 001
        // idx 2 = 010
        // idx 3 = 100
        endingMask |= 1 << (idx - 1);

        SaveManager.Instance.SetEndingMask(endingMask);

        if (IsAllEndingUnlocked(endingMask))
        {
            SteamAchievementManager.Unlock(SteamAchievementManager.Achievement8);
        }
    }

    bool IsAllEndingUnlocked(int endingMask)
    {
        int allEndingMask = GetAllEndingMask();
        return (endingMask & allEndingMask) == allEndingMask;
    }

    int GetAllEndingMask()
    {
        return (1 << EndingCount) - 1;
    }

    public bool IsEndingUnlocked(int idx)
    {
        idx = Mathf.Abs(idx);

        if (idx < 1 || idx > EndingCount)
            return false;

        if (SaveManager.Instance == null)
            return false;

        int endingMask = SaveManager.Instance.GetEndingMask();

        return (endingMask & (1 << (idx - 1))) != 0;
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
    public List<int> Appear_Production = new List<int>();
    public List<float> Appear_Production_Time = new List<float>();
    public List<float> Appear_Production_Value = new List<float>();
    public bool Auto_Next;
    public bool My_Name;
    public int Gallery;
    public string BGM;
    public float BGM_Fade_Time;
    public List<string> SFX = new List<string>();
    public List<int> SFX_Type = new List<int>();
    public List<string> cg_production = new List<string>();
    public int Ending;
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

    public int[] SelectIndex = Enumerable.Repeat(-1, 9).ToArray();

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

public class Gallery_Data
{
    public int Index;
    public int Group;
    public int BG;
    public string TextKey;
    public string Music;
    public int Start;
    public int End;
    public int Production;
}