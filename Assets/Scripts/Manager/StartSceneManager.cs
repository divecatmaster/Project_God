using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StartSceneManager : MonoBehaviour
{
    [SerializeField] CanvasGroup LogoCanvasGroup;

    [Header("Logo Sound")]
    [SerializeField] AudioSource LogoAudioSource;
    [SerializeField] AudioClip LogoSound;

    [Header("Logo Timing")]
    [SerializeField] float fadeInTime = 1f;
    [SerializeField] float minimumLogoHoldTime = 1.5f;
    [SerializeField] float fadeOutTime = 1f;

    [Header("Scene")]
    [SerializeField] string mainSceneName = "MainScene";

    Coroutine _routine;

    private void Awake()
    {
        ScreenSettingUtility.InitAndApplyScreenSetting();
        SetLogoAlpha(0f);
    }

    private void Start()
    {
        if (_routine != null)
            StopCoroutine(_routine);

        _routine = StartCoroutine(StartRoutine());
    }

    IEnumerator StartRoutine()
    {
        SetLogoAlpha(0f);

        // 첫 프레임에서 검은 화면이 먼저 그려지게 함
        yield return null;

        // Fade In과 동시에 사운드 시작
        if (LogoAudioSource != null && LogoSound != null)
        {
            LogoAudioSource.clip = LogoSound;
            LogoAudioSource.loop = false;
            LogoAudioSource.playOnAwake = false;
            LogoAudioSource.volume = 1f;
            LogoAudioSource.Play();
        }

        // 로고 Fade In
        yield return FadeLogo(0f, 1f, fadeInTime);

        // 로고가 완전히 보이는 상태로 최소 유지
        yield return new WaitForSecondsRealtime(minimumLogoHoldTime);

        // Data_Manager 생성 대기
        yield return new WaitUntil(() => Data_Manager.Instance != null);

        // 로고가 보이는 상태에서 초기화 시작
        if (!Data_Manager.Instance.IsInit)
        {
            Data_Manager.Instance.Init();
        }

        // Init 완료 대기
        yield return new WaitUntil(() => Data_Manager.Instance.IsInit);

        // 초기화 끝난 뒤 로고 Fade Out
        yield return FadeLogo(1f, 0f, fadeOutTime);

        SceneManager.LoadScene(mainSceneName);
    }

    IEnumerator FadeLogo(float from, float to, float duration)
    {
        if (LogoCanvasGroup == null)
            yield break;

        if (duration <= 0f)
        {
            SetLogoAlpha(to);
            yield break;
        }

        float elapsed = 0f;
        SetLogoAlpha(from);

        while (elapsed < duration)
        {
            // 첫 실행 렉으로 deltaTime이 튀어도 페이드가 한 번에 끝나지 않게 제한
            float dt = Mathf.Min(Time.unscaledDeltaTime, 0.033f);

            elapsed += dt;

            float t = Mathf.Clamp01(elapsed / duration);
            SetLogoAlpha(Mathf.Lerp(from, to, t));

            yield return null;
        }

        SetLogoAlpha(to);
    }

    void SetLogoAlpha(float alpha)
    {
        if (LogoCanvasGroup == null)
            return;

        LogoCanvasGroup.alpha = alpha;
    }
}