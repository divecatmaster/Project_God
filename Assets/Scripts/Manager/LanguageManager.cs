using System.Collections.Generic;
using UnityEngine;

public class LanguageManager : MonoBehaviour
{
    public static LanguageManager Instance;

    [SerializeField] LanguageType _currentLanguage = LanguageType.KR;
    Dictionary<string, string> _languageData = new Dictionary<string, string>();

    private void Awake()
    {
        // 싱글톤 중복 방지
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // 씬 변경 시 유지
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        InitLanguage();
    }

    void InitLanguage()
    {
        _languageData = new Dictionary<string, string>();

        var data = CSVReader.ReadOriginal("Language");

        string languageType = _currentLanguage.ToString();

        for (int i = 0; i < data.Count; i++)
        {
            _languageData[data[i]["ID"].ToString()] = data[i][languageType].ToString();
        }
    }

    public LanguageType GetCurrentLanguage()
    {
        return _currentLanguage;
    }

    public string GetText(string id)
    {
        if (_languageData.TryGetValue(id, out string value))
        {
            return value;
        }

        return id;
    }

    public void SetLanguage(LanguageType language)
    {
        _currentLanguage = language;
        InitLanguage();
    }
}

public enum LanguageType
{
    EN = 0,
    KR,
    JA
}
