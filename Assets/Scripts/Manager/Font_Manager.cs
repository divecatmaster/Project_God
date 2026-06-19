using UnityEngine;
using TMPro;

public class Font_Manager : MonoBehaviour
{
    public static Font_Manager Instance;

    [SerializeField] TMP_FontAsset Number_Font;
    [SerializeField] TMP_FontAsset Korean_Font;
    [SerializeField] TMP_FontAsset Korean_Font_Regular;
    [SerializeField] TMP_FontAsset Japanese_Font;

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
}
