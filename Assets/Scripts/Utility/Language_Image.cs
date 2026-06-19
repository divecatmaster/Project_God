using UnityEngine;
using UnityEngine.UI;

public class Language_Image : MonoBehaviour
{
    [SerializeField] Image Img;
    [SerializeField] Sprite Default;
    [SerializeField] Sprite KR;
    [SerializeField] Sprite JA;
    [SerializeField] Sprite CN;

    private void Start()
    {
        switch (LanguageManager.Instance.GetCurrentLanguage())
        {
            case LanguageType.KR: Img.sprite = KR; break;
            case LanguageType.EN: Img.sprite = Default; break;
            case LanguageType.JA: Img.sprite = JA; break;
            case LanguageType.CN: Img.sprite = CN; break;
            default: Img.sprite = Default; break;
        }
    }
}
