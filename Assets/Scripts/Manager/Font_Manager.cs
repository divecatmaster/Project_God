using UnityEngine;
using TMPro;

public class Font_Manager : MonoBehaviour
{
    public static Font_Manager Instance;

    [SerializeField] TMP_FontAsset Number_Font;
    [SerializeField] TMP_FontAsset Korean_Font;
    [SerializeField] TMP_FontAsset English_Font;
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

    public TMP_FontAsset GetFont(LanguageType language)
    {
        switch (language)
        {
            case LanguageType.KR: return Korean_Font;
            case LanguageType.EN: return English_Font;
            case LanguageType.JA: return Japanese_Font;
            case LanguageType.MAX: return Number_Font;
            default: return English_Font;
        }
    }
}
