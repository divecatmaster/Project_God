using TMPro;
using UnityEngine;

public class Language_UI : MonoBehaviour
{
    [SerializeField] string Language_Key;
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
        }
    }
}
