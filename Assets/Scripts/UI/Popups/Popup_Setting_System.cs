using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Popup_Setting_System : MonoBehaviour
{
    [Header("ScreenMode")]
    [SerializeField] TMP_Dropdown ScreenMode_Dropdown;

    [Header("Resolution")]
    [SerializeField] TMP_Dropdown Resolution_Dropdown;
    private readonly List<Resolution> _resolutionList = new();

    private void Awake()
    {
        ScreenMode_Dropdown.onValueChanged.AddListener(OnValueChange_ScreenMode);
        Resolution_Dropdown.onValueChanged.AddListener(OnValueChange_Resolution);

    }

    void OnEnable()
    {
        SetScreenMode();
        SetResolutionDropdown();
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

            string key = $"{resolution.width}x{resolution.height}";

            if (!added.Add(key))
                continue;

            _resolutionList.Add(resolution);
            options.Add(GetResolutionText(resolution));

            if (resolution.width == Data_Manager.Instance.Screen_Width && resolution.height == Data_Manager.Instance.Screen_Height)
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
        FullScreenMode mode = GetScreenMode();

        Screen.SetResolution(
            Data_Manager.Instance.Screen_Width,
            Data_Manager.Instance.Screen_Height,
            mode
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
}