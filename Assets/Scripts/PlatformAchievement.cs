using UnityEngine;

#if STEAM
using Steamworks;
#endif

public static class PlatformAchievement
{
    public const string Achievement1 = "Achievement_1";
    public const string Achievement2 = "Achievement_2";
    public const string Achievement3 = "Achievement_3";
    public const string Achievement4 = "Achievement_4";
    public const string Achievement5 = "Achievement_5";
    public const string Achievement6 = "Achievement_6";
    public const string Achievement7 = "Achievement_7";
    public const string Achievement8 = "Achievement_8";

    public static void Unlock(string achievementId)
    {
#if STEAM
        UnlockSteam(achievementId);
#elif STOVE
        STOVEPCSDK3Manager.Instance.UnlockAchievement(achievementId);
#endif
    }

#if STEAM
    private static void UnlockSteam(string achievementId)
    {
        if (!SteamManager.Initialized)
        {
            Debug.LogWarning($"Steam이 초기화되지 않아 업적 해금 실패: {achievementId}");
            return;
        }

        if (SteamUserStats.GetAchievement(achievementId, out bool unlocked) && unlocked)
        {
            Debug.Log($"이미 획득한 Steam 업적입니다: {achievementId}");
            return;
        }

        if (!SteamUserStats.SetAchievement(achievementId))
        {
            Debug.LogWarning($"Steam 업적 해금 실패: {achievementId}");
            return;
        }

        if (!SteamUserStats.StoreStats())
        {
            Debug.LogWarning($"Steam 업적 저장 요청 실패: {achievementId}");
            return;
        }

        Debug.Log($"Steam 업적 해금 요청: {achievementId}");
    }
#endif
}