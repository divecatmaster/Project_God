using UnityEngine;

public static class ScreenSettingUtility
{
    const string ScreenModeKey = "ScreenMode";
    const string ScreenWidthKey = "Screen_Width";
    const string ScreenHeightKey = "Screen_Height";

    const int DefaultWidth = 1920;
    const int DefaultHeight = 1080;

    public static void InitAndApplyScreenSetting()
    {
        bool hasScreenSetting =
            PlayerPrefs.HasKey(ScreenModeKey) &&
            PlayerPrefs.HasKey(ScreenWidthKey) &&
            PlayerPrefs.HasKey(ScreenHeightKey);

        int screenMode;
        int width;
        int height;

        if (!hasScreenSetting)
        {
            SetDefaultLandscapeSetting(out screenMode, out width, out height);

            PlayerPrefs.SetInt(ScreenModeKey, screenMode);
            PlayerPrefs.SetInt(ScreenWidthKey, width);
            PlayerPrefs.SetInt(ScreenHeightKey, height);
            PlayerPrefs.Save();
        }
        else
        {
            screenMode = PlayerPrefs.GetInt(ScreenModeKey, 2);
            width = PlayerPrefs.GetInt(ScreenWidthKey, DefaultWidth);
            height = PlayerPrefs.GetInt(ScreenHeightKey, DefaultHeight);

            // 저장된 값이 세로 해상도면 가로로 보정
            if (width < height)
            {
                int temp = width;
                width = height;
                height = temp;

                PlayerPrefs.SetInt(ScreenWidthKey, width);
                PlayerPrefs.SetInt(ScreenHeightKey, height);
                PlayerPrefs.Save();
            }
        }

        ApplyScreen(width, height, screenMode);
    }

    static void SetDefaultLandscapeSetting(out int screenMode, out int width, out int height)
    {
        int monitorWidth = Display.main.systemWidth;
        int monitorHeight = Display.main.systemHeight;

        // 일반 가로 모니터
        if (monitorWidth >= monitorHeight)
        {
            screenMode = 2; // 테두리 없는 전체화면
            width = monitorWidth;
            height = monitorHeight;
            return;
        }

        // 세로 모니터
        // 세로 모니터에서는 FullScreenWindow를 쓰면 세로 화면으로 꽉 차기 때문에
        // 기본값은 창 모드로 가로 비율을 유지하는 게 안전함
        screenMode = 1; // 창 모드

        GetBestLandscapeWindowSize(monitorWidth, monitorHeight, out width, out height);
    }

    static void GetBestLandscapeWindowSize(int monitorWidth, int monitorHeight, out int width, out int height)
    {
        // 세로 모니터에 들어가는 16:9 가로 창 크기 계산
        width = monitorWidth;
        height = Mathf.RoundToInt(width * 9f / 16f);

        // 너무 작거나 이상하면 기본값 보정
        if (width <= 0 || height <= 0)
        {
            width = 1280;
            height = 720;
        }

        // 최소 크기 보정
        if (width < 960)
        {
            width = 960;
            height = 540;
        }
    }

    public static void ApplySavedScreen()
    {
        int screenMode = PlayerPrefs.GetInt(ScreenModeKey, 2);
        int width = PlayerPrefs.GetInt(ScreenWidthKey, DefaultWidth);
        int height = PlayerPrefs.GetInt(ScreenHeightKey, DefaultHeight);

        if (width < height)
        {
            int temp = width;
            width = height;
            height = temp;
        }

        ApplyScreen(width, height, screenMode);
    }

    public static void ApplyScreen(int width, int height, int screenMode)
    {
        if (width < height)
        {
            int temp = width;
            width = height;
            height = temp;
        }

        FullScreenMode mode = GetFullScreenMode(screenMode);

        Screen.SetResolution(width, height, mode);
    }

    static FullScreenMode GetFullScreenMode(int screenMode)
    {
        switch (screenMode)
        {
            case 0:
                return FullScreenMode.ExclusiveFullScreen;

            case 1:
                return FullScreenMode.Windowed;

            case 2:
                return FullScreenMode.FullScreenWindow;

            default:
                return FullScreenMode.FullScreenWindow;
        }
    }
}