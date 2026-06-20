using UnityEngine;
using TMPro;
using System;

public class Font_Manager : MonoBehaviour
{
    public static Font_Manager Instance;

    [Header("Font")]
    [SerializeField] TMP_FontAsset Number_Font;
    [SerializeField] TMP_FontAsset Korean_Font;
    [SerializeField] TMP_FontAsset Korean_Font_Regular;
    [SerializeField] TMP_FontAsset Japanese_Font;

    [Header("Material")]
    [SerializeField] Material Korean_Default;
    [SerializeField] Material Korean_Blur;
    [SerializeField] Material Korean_Shadow;
    [SerializeField] Material Other_Default;
    [SerializeField] Material Other_Blur;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public TMP_FontAsset GetFont(bool IsRegular = false)
    {
        LanguageType language = LanguageManager.Instance.GetCurrentLanguage();
        switch (language)
        {
            case LanguageType.KR:
            case LanguageType.EN:
                {
                    if (IsRegular)
                    {
                        return Korean_Font_Regular;
                    }
                    else
                    {
                        return Korean_Font;
                    }
                }
            case LanguageType.JA: return Japanese_Font;
            case LanguageType.MAX: return Number_Font;
            default: 
            {
                if (IsRegular)
                    {
                        return Korean_Font_Regular;
                    }
                    else
                    {
                        return Korean_Font;
                    }
            }
        }
    }

    public Material GetFontMaterial(int type)
    {
        switch (type)
        {
            case 0: return Korean_Default;
            case 1: return Korean_Blur;
            case 2: return Korean_Shadow;
            case 3: return Other_Default;
            case 4: return Other_Blur;
            default: return null;
        }
    }
}
