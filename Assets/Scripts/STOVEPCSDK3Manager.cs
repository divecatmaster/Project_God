#if STOVE

using System.Text;
using System.Collections;
using UnityEngine;

using static Stove.PCSDK.Base;
using static Stove.PCSDK.GameSupport;

public class STOVEPCSDK3Manager : MonoBehaviour
{
    private bool _isInitialized;
    private bool _isInitializing;

    private float _runCallbackInterval = 1.0f;
    private Coroutine _runCallbackCoroutine;

    private static STOVEPCSDK3Manager _instance;
    private static readonly object _lockObject = new object();

    public static STOVEPCSDK3Manager Instance
    {
        get
        {
            lock (_lockObject)
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<STOVEPCSDK3Manager>();

                    if (_instance == null)
                    {
                        _instance = new GameObject("STOVEPCSDK3Manager").AddComponent<STOVEPCSDK3Manager>();
                    }
                }
            }

            return _instance;
        }
    }

    public bool IsInitialized => _isInitialized;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        if (_instance != this)
            return;

        if (_isInitialized)
        {
            UnInitialize();
        }

        _instance = null;
    }

    #region Callback

    private IEnumerator RunCallbackCoroutine()
    {
        var wait = new WaitForSecondsRealtime(_runCallbackInterval);

        while (true)
        {
            Base_RunCallback();
            yield return wait;
        }
    }

    public void StartRunCallbackLoop()
    {
        if (_runCallbackCoroutine != null)
            return;

        Debug.Log("[STOVE] Start RunCallbackLoop");

        _runCallbackCoroutine = StartCoroutine(RunCallbackCoroutine());
    }

    public void StopRunCallbackLoop()
    {
        if (_runCallbackCoroutine == null)
            return;

        Debug.Log("[STOVE] Stop RunCallbackLoop");

        StopCoroutine(_runCallbackCoroutine);
        _runCallbackCoroutine = null;
    }

    #endregion

    #region Result Log

    public void PrintResult(Result result)
    {
        StringBuilder sb = new StringBuilder();

        sb.AppendLine("# STOVE Result");
        sb.AppendLine($"sdkName : {result.sdkName}");
        sb.AppendLine($"methodCode : {result.methodCode}");
        sb.AppendLine($"resultCode : {result.resultCode}");
        sb.AppendLine($"exceptionMessage : {result.exceptionMessage}");

        Debug.Log(sb.ToString());
    }

    public void PrintCallbackResult(CallbackResult callbackResult)
    {
        StringBuilder sb = new StringBuilder();

        sb.AppendLine("# STOVE CallbackResult");
        sb.AppendLine($"sdkName : {callbackResult.result.sdkName}");
        sb.AppendLine($"methodCode : {callbackResult.result.methodCode}");
        sb.AppendLine($"resultCode : {callbackResult.result.resultCode}");
        sb.AppendLine($"exceptionMessage : {callbackResult.result.exceptionMessage}");
        sb.AppendLine($"externalError : {callbackResult.externalError}");

        Debug.Log(sb.ToString());
    }

    #endregion

    #region Initialize

    public void Initialize()
    {
        if (_isInitialized || _isInitializing)
            return;

        _isInitializing = true;

        StartRunCallbackLoop();

        var initParam = new StovePCInitializeParamEx2
        {
            environment = "LIVE",
            gameId = "GM-2AB8-6A681B81_IND",
            applicationKey = "43f3ed7fb2c0aebcb489645ec61b655f23c8cd4f7ff328c17fa32215f1d5e6bf",
            waitTimeMillisec = 60000,
            launchLauncher = true
        };

        Base_RestartAppIfNecessaryAsyncEx2(initParam,
            (CallbackResult callbackResult, bool restartAppIfNecessary) =>
        {
            PrintCallbackResult(callbackResult);

            if (!callbackResult.result.IsSuccessful())
            {
                Debug.LogError("[STOVE] RestartAppIfNecessary 실패");
                InitializeFailed();
                return;
            }

            if (restartAppIfNecessary)
            {
                Debug.Log("[STOVE] STOVE Client를 통해 게임을 다시 실행합니다.");
                Application.Quit();
                return;
            }

            InitializeBase();
        });
    }

    private void InitializeBase()
    {
        Base_InitializeEx((CallbackResult callbackResult) =>
        {
            PrintCallbackResult(callbackResult);

            if (!callbackResult.result.IsSuccessful())
            {
                Debug.LogError("[STOVE] Base SDK 초기화 실패");
                InitializeFailed();
                return;
            }

            Debug.Log("[STOVE] Base SDK 초기화 성공");

            InitializeGameSupport();
        });
    }

    private void InitializeGameSupport()
    {
        Result result = GameSupport_Initialize();

        PrintResult(result);

        if (!result.IsSuccessful())
        {
            Debug.LogError("[STOVE] GameSupport SDK 초기화 실패");
            InitializeFailed();
            return;
        }

        _isInitialized = true;
        _isInitializing = false;

        Debug.Log("[STOVE] Base + GameSupport 초기화 완료");

        //TestCloudSavePath();
    }

    private void InitializeFailed()
    {
        _isInitialized = false;
        _isInitializing = false;

        StopRunCallbackLoop();
    }

    #endregion

    #region UnInitialize

    public void UnInitialize()
    {
        if (!_isInitialized)
            return;

        Result gameSupportResult = GameSupport_UnInitialize();
        PrintResult(gameSupportResult);

        Result baseResult = Base_UnInitialize();
        PrintResult(baseResult);

        StopRunCallbackLoop();

        _isInitialized = false;
        _isInitializing = false;

        Debug.Log("[STOVE] SDK 정리 완료");
    }

    #endregion

    #region Achievement

    public void UnlockAchievement(string achievementId)
    {
        if (!_isInitialized)
        {
            Debug.LogWarning($"[STOVE] SDK가 초기화되지 않아 업적을 처리하지 못했습니다: {achievementId}");
            return;
        }

        string statId;
        int statValue;

        switch (achievementId)
        {
            case PlatformAchievement.Achievement1:
                statId = "ACHIEVEMENT_1";
                statValue = 1;
                break;

            case PlatformAchievement.Achievement2:
                statId = "ACHIEVEMENT_2";
                statValue = 1;
                break;

            case PlatformAchievement.Achievement3:
                statId = "ACHIEVEMENT_3";
                statValue = 1;
                break;

            case PlatformAchievement.Achievement4:
                statId = "ACHIEVEMENT_4";
                statValue = 1;
                break;

            case PlatformAchievement.Achievement5:
                statId = "ACHIEVEMENT_5";
                statValue = 1;
                break;

            case PlatformAchievement.Achievement6:
                statId = "ACHIEVEMENT_6";
                statValue = 22;
                break;

            case PlatformAchievement.Achievement7:
                statId = "ACHIEVEMENT_7";
                statValue = 100;
                break;

            case PlatformAchievement.Achievement8:
                statId = "ACHIEVEMENT_8";
                statValue = 7;
                break;

            default:
                Debug.LogError($"[STOVE] 등록되지 않은 업적 ID입니다: {achievementId}");
                return;
        }

        ModifyAchievementStat(statId, statValue);
    }

    private void ModifyAchievementStat(string statId, int statValue)
    {
        Debug.Log($"[STOVE] 업적 스탯 변경 요청: {statId} = {statValue}");

        GameSupport_ModifyStat(statId, statValue,
            (CallbackResult callbackResult, StovePCModifyStatValue stat) =>
        {
            PrintCallbackResult(callbackResult);

            if (!callbackResult.result.IsSuccessful())
            {
                Debug.LogError($"[STOVE] 업적 스탯 변경 실패: {statId}");
                return;
            }

            Debug.Log($"[STOVE] 업적 스탯 변경 성공: {statId} = {statValue}");
        });
    }

    #endregion

    public bool TryGetUserInfo(out StovePCUser user)
    {
        user = new StovePCUser();

        if (!_isInitialized)
        {
            Debug.LogWarning("[STOVE] SDK가 초기화되지 않아 유저 정보를 가져올 수 없습니다.");
            return false;
        }

        Result result = Base_GetUser(ref user);
        PrintResult(result);

        if (!result.IsSuccessful())
        {
            Debug.LogError("[STOVE] 유저 정보 조회 실패");
            return false;
        }

        Debug.Log("[STOVE] 유저 정보 조회 성공");
        Debug.Log($"[STOVE] 닉네임: {user.nickname}");

        return true;
    }

    public string GetCloudSavePath()
    {
        if (!_isInitialized)
        {
            Debug.LogWarning("[STOVE] SDK가 초기화되지 않아 클라우드 저장 경로를 가져올 수 없습니다.");
            return "";
        }

        string cloudSavingPath = "";
        uint length = 1024;

        Result result = Base_GetCloudSavingPath(ref cloudSavingPath, length);
        PrintResult(result);

        if (!result.IsSuccessful())
        {
            Debug.LogError("[STOVE] 클라우드 저장 경로 조회 실패");
            return "";
        }

        Debug.Log($"[STOVE] 클라우드 저장 경로: {cloudSavingPath}");
        return cloudSavingPath;
    }

    // private void TestCloudSavePath()
    // {
    //     string cloudPath = GetCloudSavePath();

    //     if (string.IsNullOrEmpty(cloudPath))
    //     {
    //         Debug.LogError("[STOVE TEST] 클라우드 세이브 경로 조회 실패");
    //         return;
    //     }

    //     Debug.Log($"[STOVE TEST] 클라우드 세이브 경로 확인 성공: {cloudPath}");
    // }
}

#endif