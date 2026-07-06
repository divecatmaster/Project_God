using System.Collections.Generic;
using UnityEngine;

public class CG_Effect_Manager : MonoBehaviour
{
    public static CG_Effect_Manager Instance;
    Dictionary<string, GameObject> Effect_Dic = new Dictionary<string, GameObject>();
    List<GameObject> ActiveList = new List<GameObject>();
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
        Effect_Dic = new Dictionary<string, GameObject>();

        for (int i = 0; i < transform.childCount; i++)
        {
            var target = transform.GetChild(i);

            if (Effect_Dic.ContainsKey(target.name))
            {
                continue;
            }

            Effect_Dic.Add(target.name, target.gameObject);
        }
    }

    public void SetEffect(List<string> names)
    {
        if (names == null || names.Count <= 0)
        {
            OffEffects();
            return;
        }

        // 이번에 켜야 하는 이펙트 목록
        List<GameObject> nextActiveList = new List<GameObject>();

        for (int i = 0; i < names.Count; i++)
        {
            string effectName = names[i];

            if (string.IsNullOrEmpty(effectName))
                continue;

            if (!Effect_Dic.ContainsKey(effectName))
            {
                Debug.LogWarning($"등록되지 않은 CG Effect입니다: {effectName}");
                continue;
            }

            GameObject effect = Effect_Dic[effectName];

            if (!nextActiveList.Contains(effect))
            {
                nextActiveList.Add(effect);
            }
        }

        // 기존에 켜져 있던 것 중, 이번 목록에 없는 것은 끄기
        for (int i = 0; i < ActiveList.Count; i++)
        {
            GameObject activeEffect = ActiveList[i];

            if (activeEffect == null)
                continue;

            if (!nextActiveList.Contains(activeEffect))
            {
                activeEffect.SetActive(false);
            }
        }

        // 이번 목록에 있는 것은 켜기
        for (int i = 0; i < nextActiveList.Count; i++)
        {
            if (nextActiveList[i] != null)
            {
                nextActiveList[i].SetActive(true);
            }
        }

        ActiveList.Clear();
        ActiveList.AddRange(nextActiveList);
    }

    public void OffEffects()
    {
        for (int i = 0; i < ActiveList.Count; i++)
        {
            ActiveList[i].SetActive(false);
        }
        ActiveList.Clear();
    }
}
