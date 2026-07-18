using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CG_Effect_Manager : MonoBehaviour
{
    public static CG_Effect_Manager Instance;

    [SerializeField] float fadeOutDuration = 1.5f;

    Dictionary<string, GameObject> Effect_Dic = new Dictionary<string, GameObject>();
    List<GameObject> ActiveList = new List<GameObject>();

    Dictionary<GameObject, Coroutine> FadeCoroutine_Dic = new Dictionary<GameObject, Coroutine>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        Init();
    }

    void Init()
    {
        Effect_Dic.Clear();
        ActiveList.Clear();
        FadeCoroutine_Dic.Clear();

        for (int i = 0; i < transform.childCount; i++)
        {
            Transform target = transform.GetChild(i);

            if (Effect_Dic.ContainsKey(target.name))
            {
                Debug.LogWarning($"중복된 CG Effect 이름입니다: {target.name}");
                continue;
            }

            GameObject effect = target.gameObject;

            CanvasGroup canvasGroup = effect.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = effect.AddComponent<CanvasGroup>();

            canvasGroup.alpha = 0f;
            effect.SetActive(false);

            Effect_Dic.Add(target.name, effect);
        }
    }

    public void SetEffect(List<string> names)
    {
        if (names == null || names.Count <= 0)
        {
            OffEffects();
            return;
        }

        List<GameObject> nextActiveList = new List<GameObject>();

        for (int i = 0; i < names.Count; i++)
        {
            string effectName = names[i];

            if (string.IsNullOrEmpty(effectName))
                continue;

            if (!Effect_Dic.TryGetValue(effectName, out GameObject effect))
            {
                Debug.LogWarning($"등록되지 않은 CG Effect입니다: {effectName}");
                continue;
            }

            if (effect == null)
                continue;

            if (!nextActiveList.Contains(effect))
                nextActiveList.Add(effect);
        }

        // 기존에 켜져 있던 것 중 이번 목록에 없는 것만 Fade Out
        for (int i = 0; i < ActiveList.Count; i++)
        {
            GameObject activeEffect = ActiveList[i];

            if (activeEffect == null)
                continue;

            if (!nextActiveList.Contains(activeEffect))
            {
                FadeOutEffect(activeEffect);
            }
        }

        // 이번 목록에 있는 것은 켜기
        for (int i = 0; i < nextActiveList.Count; i++)
        {
            GameObject effect = nextActiveList[i];

            if (effect == null)
                continue;

            ShowEffect(effect);
        }

        ActiveList.Clear();
        ActiveList.AddRange(nextActiveList);
    }

    public void OffEffects()
    {
        for (int i = 0; i < ActiveList.Count; i++)
        {
            if (ActiveList[i] != null)
            {
                FadeOutEffect(ActiveList[i]);
            }
        }

        ActiveList.Clear();
    }

    void ShowEffect(GameObject effect)
    {
        if (effect == null)
            return;

        if (FadeCoroutine_Dic.ContainsKey(effect))
        {
            if (FadeCoroutine_Dic[effect] != null)
                StopCoroutine(FadeCoroutine_Dic[effect]);

            FadeCoroutine_Dic.Remove(effect);
        }

        CanvasGroup canvasGroup = effect.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = effect.AddComponent<CanvasGroup>();

        effect.SetActive(true);
        canvasGroup.alpha = 1f;
    }

    void FadeOutEffect(GameObject effect)
    {
        if (effect == null)
            return;

        if (!effect.activeSelf)
            return;

        if (FadeCoroutine_Dic.ContainsKey(effect))
        {
            if (FadeCoroutine_Dic[effect] != null)
                StopCoroutine(FadeCoroutine_Dic[effect]);

            FadeCoroutine_Dic.Remove(effect);
        }

        Coroutine coroutine = StartCoroutine(FadeOutRoutine(effect));
        FadeCoroutine_Dic.Add(effect, coroutine);
    }

    void OffEffectImmediate(GameObject effect)
    {
        if (effect == null)
            return;

        // 진행 중인 Fade 코루틴이 있으면 중지
        if (FadeCoroutine_Dic.ContainsKey(effect))
        {
            if (FadeCoroutine_Dic[effect] != null)
                StopCoroutine(FadeCoroutine_Dic[effect]);

            FadeCoroutine_Dic.Remove(effect);
        }

        CanvasGroup canvasGroup = effect.GetComponent<CanvasGroup>();
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }

        effect.SetActive(false);
    }

    public void OffEffectsImmediate()
    {
        for (int i = 0; i < ActiveList.Count; i++)
        {
            if (ActiveList[i] != null)
            {
                OffEffectImmediate(ActiveList[i]);
            }
        }

        ActiveList.Clear();
    }

    IEnumerator FadeOutRoutine(GameObject effect)
    {
        if (effect == null)
            yield break;

        CanvasGroup canvasGroup = effect.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = effect.AddComponent<CanvasGroup>();

        float startAlpha = canvasGroup.alpha;
        float elapsed = 0f;

        if (fadeOutDuration <= 0f)
        {
            canvasGroup.alpha = 0f;
            effect.SetActive(false);

            FadeCoroutine_Dic.Remove(effect);
            yield break;
        }

        while (elapsed < fadeOutDuration)
        {
            if (effect == null || canvasGroup == null)
                yield break;

            elapsed += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(elapsed / fadeOutDuration);
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, t);

            yield return null;
        }

        if (canvasGroup != null)
            canvasGroup.alpha = 0f;

        if (effect != null)
            effect.SetActive(false);

        FadeCoroutine_Dic.Remove(effect);
    }
}