using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using God.Audio;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Popup_Setting_System : MonoBehaviour
{
    [Header("ScreenMode")]
    [SerializeField] TMP_Dropdown ScreenMode_Dropdown;

    [Header("Resolution")]
    [SerializeField] TMP_Dropdown Resolution_Dropdown;
    private readonly List<Resolution> _resolutionList = new();

    [Header("Language")]
    [SerializeField] TMP_Dropdown Language_Dropdown;

    [Header("Reset")]
    [SerializeField] Button ResetBtn;

    [Header("ResetAchieve")]
    [SerializeField] GameObject ResetAchieve;
    [SerializeField] Button ResetAchieveBtn;

    private void Awake()
    {
        ScreenMode_Dropdown.onValueChanged.AddListener(OnValueChange_ScreenMode);
        Resolution_Dropdown.onValueChanged.AddListener(OnValueChange_Resolution);
        Language_Dropdown.onValueChanged.AddListener(OnValueChange_Language);
        AddDropdownClickSound(ScreenMode_Dropdown);
        AddDropdownClickSound(Resolution_Dropdown);
        AddDropdownClickSound(Language_Dropdown);
        ResetBtn.onClick.AddListener(OnClickReset);

#if STEAM && UNITY_EDITOR
        ResetAchieve.SetActive(true);
        ResetAchieveBtn.onClick.AddListener(OnClickAchieveReset);
#else
        ResetAchieve.SetActive(false);
#endif
    }

    void OnEnable()
    {
        SetScreenMode();
        SetResolutionDropdown();
        SetLanguage();
    }

    void AddDropdownClickSound(TMP_Dropdown dropdown)
    {
        EventTrigger trigger = dropdown.GetComponent<EventTrigger>();

        if (trigger == null)
            trigger = dropdown.gameObject.AddComponent<EventTrigger>();

        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerClick;
        entry.callback.AddListener((data) =>
        {
            SoundManager.Instance.PlayUI("Click");
        });

        trigger.triggers.Add(entry);
    }

    void SetScreenMode()
    {
        ScreenMode_Dropdown.ClearOptions();

        ScreenMode_Dropdown.AddOptions(new List<string>()
        {
            LanguageManager.Instance.GetText("ScreenMode_1"),
            LanguageManager.Instance.GetText("ScreenMode_2"),
            LanguageManager.Instance.GetText("ScreenMode_3")
        });

        ScreenMode_Dropdown.SetValueWithoutNotify(Data_Manager.Instance.ScreenMode);
        ScreenMode_Dropdown.RefreshShownValue();
    }

    void OnValueChange_ScreenMode(int index)
    {
        if (Data_Manager.Instance.ScreenMode == index)
            return;

        Data_Manager.Instance.SetScreenMode(index);
        SetScreen();
    }

    void SetResolutionDropdown()
    {
        Resolution_Dropdown.ClearOptions();
        _resolutionList.Clear();

        List<string> options = new();
        HashSet<string> added = new();

        Resolution[] resolutions = Screen.resolutions;

        int currentIndex = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            Resolution resolution = resolutions[i];

            if (resolution.width < resolution.height)
                continue;

            string key = $"{resolution.width}x{resolution.height}";

            if (!added.Add(key))
                continue;

            _resolutionList.Add(resolution);
            options.Add(GetResolutionText(resolution));

            if (resolution.width == Data_Manager.Instance.Screen_Width &&
                resolution.height == Data_Manager.Instance.Screen_Height)
            {
                currentIndex = _resolutionList.Count - 1;
            }
        }

        Resolution_Dropdown.AddOptions(options);
        Resolution_Dropdown.SetValueWithoutNotify(currentIndex);
        Resolution_Dropdown.RefreshShownValue();
    }

    void OnValueChange_Resolution(int index)
    {
        if (index < 0 || index >= _resolutionList.Count)
            return;

        Resolution resolution = _resolutionList[index];

        Data_Manager.Instance.SetScreenResolution(resolution.width, resolution.height);
        SetScreen();
    }

    void SetScreen()
    {
        ScreenSettingUtility.ApplyScreen(
        Data_Manager.Instance.Screen_Width,
        Data_Manager.Instance.Screen_Height,
        Data_Manager.Instance.ScreenMode
    );
    }

    FullScreenMode GetScreenMode()
    {
        var temp = FullScreenMode.ExclusiveFullScreen;
        switch (Data_Manager.Instance.ScreenMode)
        {
            case 0: temp = FullScreenMode.ExclusiveFullScreen; break;
            case 1: temp = FullScreenMode.Windowed; break;
            case 2: temp = FullScreenMode.FullScreenWindow; break;
        }

        return temp;
    }

    string GetResolutionText(Resolution resolution)
    {
        float ratio = (float)resolution.width / resolution.height;

        if (Mathf.Abs(ratio - (16f / 9f)) < 0.05f)
            return $"{resolution.width} x {resolution.height}";

        if (Mathf.Abs(ratio - (21f / 9f)) < 0.05f)
            return $"{resolution.width} x {resolution.height} (21:9)";

        if (Mathf.Abs(ratio - (32f / 9f)) < 0.05f)
            return $"{resolution.width} x {resolution.height} (32:9)";

        if (Mathf.Abs(ratio - (16f / 10f)) < 0.05f)
            return $"{resolution.width} x {resolution.height} (16:10)";

        return $"{resolution.width} x {resolution.height}";
    }

    void SetLanguage()
    {
        Language_Dropdown.ClearOptions();

        var list = new List<string>();
        for (int i = 0; i < (int)LanguageType.MAX - 1; i++)
        {
            list.Add(LanguageManager.Instance.GetText($"Language_{(LanguageType)i + 1}"));
        }
        Language_Dropdown.AddOptions(list);

        Language_Dropdown.SetValueWithoutNotify((int)LanguageManager.Instance.GetCurrentLanguage() - 1);
        Language_Dropdown.RefreshShownValue();
    }

    void OnValueChange_Language(int index)
    {
        if ((int)LanguageManager.Instance.GetCurrentLanguage() == (index + 1))
            return;

        var popup = Resource_Manager.Instance.Get_Yes_Or_No();
        popup.Open();
        popup.SetPopup(LanguageManager.Instance.GetText("Language_Warning"), () =>
        {
            LanguageManager.Instance.ChangeLanguage((LanguageType)(index + 1));
            this.gameObject.SetActive(false);
            popup.Close();
            //LanguageManager.Instance.SetLanguage((LanguageType)(index + 1));
        },
        () =>
        {
            Language_Dropdown.SetValueWithoutNotify((int)LanguageManager.Instance.GetCurrentLanguage() - 1);
            Language_Dropdown.RefreshShownValue();
            popup.Close();
        });
    }

    void OnClickReset()
    {
        var popup = Resource_Manager.Instance.Get_Yes_Or_No();
        popup.Open();
        popup.SetPopup(LanguageManager.Instance.GetText("Reset_Expl"), () =>
        {
            //save data 초기화
            //갤러리 초기화
            Data_Manager.Instance.ResetData();
            popup.Close();
        },
        () =>
        {
            popup.Close();
        });
    }

    void OnClickAchieveReset()
    {
#if STEAM && (UNITY_EDITOR || DEVELOPMENT_BUILD)
    var popup = Resource_Manager.Instance.Get_Yes_Or_No();
    popup.Open();

    popup.SetPopup(LanguageManager.Instance.GetText("업적 초기화"), () =>
    {
        SteamAchievementManager.ClearAllAchievements();
        popup.Close();
    },
    () =>
    {
        popup.Close();
    });
#else
        Debug.LogWarning("Steam 업적 초기화는 Steam 에디터 또는 개발 빌드에서만 사용할 수 있습니다.");
#endif
    }
}