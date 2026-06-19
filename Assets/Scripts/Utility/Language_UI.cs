using TMPro;
using UnityEngine;

public class Language_UI : MonoBehaviour
{
    [SerializeField] string Language_Key;
    [SerializeField] bool IsRegularFont;
    TextMeshProUGUI _text;
    void Start()
    {
        if (_text == null)
        {
            _text = GetComponent<TextMeshProUGUI>();
        }

        if (_text != null)
        {
            _text.text = LanguageManager.Instance.GetText(Language_Key);
            _text.font = Font_Manager.Instance.GetFont(IsRegularFont);
        }
    }
}
