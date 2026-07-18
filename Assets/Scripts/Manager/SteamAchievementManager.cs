using Steamworks;
using UnityEngine;

public static class SteamAchievementManager
{
    public const string Achievement1 = "Achievement_1";
    public const string Achievement2 = "Achievement_2";
    public const string Achievement3 = "Achievement_3";
    public const string Achievement4 = "Achievement_4";
    public const string Achievement5 = "Achievement_5";
    public const string Achievement6 = "Achievement_6";
    public const string Achievement7 = "Achievement_7";
    public const string Achievement8 = "Achievement_8";

    /// <summary>
    /// Steam 업적을 해금합니다.
    /// </summary>
    public static bool Unlock(string achievementId)
    {
        if (!SteamManager.Initialized)
        {
            Debug.LogWarning(
                $"Steam이 초기화되지 않아 업적을 해금하지 못했습니다: {achievementId}"
            );
            return false;
        }

        // 이미 획득한 업적이면 다시 처리하지 않음
        if (SteamUserStats.GetAchievement(achievementId, out bool unlocked) && unlocked)
        {
            Debug.Log($"이미 획득한 Steam 업적입니다: {achievementId}");
            return true;
        }

        if (!SteamUserStats.SetAchievement(achievementId))
        {
            Debug.LogError($"Steam 업적 설정에 실패했습니다: {achievementId}");
            return false;
        }

        if (!SteamUserStats.StoreStats())
        {
            Debug.LogError($"Steam 업적 저장 요청에 실패했습니다: {achievementId}");
            return false;
        }

        Debug.Log($"Steam 업적 해금 요청 완료: {achievementId}");
        return true;
    }
}