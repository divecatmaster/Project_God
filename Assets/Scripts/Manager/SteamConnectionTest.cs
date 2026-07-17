using UnityEngine;
using Steamworks;

public class SteamConnectionTest : MonoBehaviour
{
    private void Start()
    {
        if (!SteamManager.Initialized)
        {
            Debug.LogError("Steam 초기화 실패");
            return;
        }

        string userName = SteamFriends.GetPersonaName();
        CSteamID steamId = SteamUser.GetSteamID();

        Debug.Log($"Steam 연결 성공");
        Debug.Log($"Steam 닉네임: {userName}");
        Debug.Log($"Steam ID: {steamId}");
    }
}