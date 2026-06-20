using DiveCat.God.UI.Popups;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Text;
using System;

public class Popup_Name : PopupBase
{
    [SerializeField] Button ConfirmBtn;
    [SerializeField] TMP_InputField Input;
    [SerializeField] int maxByte = 21;
    [SerializeField] int maxLength = 12;

    Action _callback;
    protected override void Awake()
    {
        base.Awake();
        ConfirmBtn.onClick.AddListener(OnClickConfirm);
        Input.onValueChanged.AddListener(OnValueChanged);
    }

    public override void Open(Action onComplete = null)
    {
        base.Open(onComplete);
        PopupManager.Instance.ActiveESC(false);
    }

    public override void Close(Action onComplete = null)
    {
        base.Close(onComplete);
        PopupManager.Instance.ActiveESC(true);
    }

    void OnDestroy()
    {
        Input.onValueChanged.RemoveListener(OnValueChanged);
    }

    public void SetPopup(Action callback)
    {
        _callback = callback;
    }

    void OnClickConfirm()
    {
        var str = Input.text;
        if (string.IsNullOrEmpty(str))
        {
            var popup = Resource_Manager.Instance.Get_Yes_Or_No();
            popup.Open();
            popup.SetPopup_One(LanguageManager.Instance.GetText("Name_Warning_1"), () =>
            {
                popup.Close();
            });
            return;
        }
        else if(IsOverLimit(str))
        {
            var popup = Resource_Manager.Instance.Get_Yes_Or_No();
            popup.Open();
            popup.SetPopup_One(LanguageManager.Instance.GetText("Name_Warning_2"), () =>
            {
                popup.Close();
            });
            return;
        }
        else if (Input.text == Data_Manager.Instance.MyName)
        {
            var popup = Resource_Manager.Instance.Get_Yes_Or_No();
            popup.Open();
            popup.SetPopup_One(LanguageManager.Instance.GetText("Name_Warning_3"), () =>
            {
                popup.Close();
            });
            return;
        }
        else
        {
            Data_Manager.Instance.SetMyName(str);
            _callback?.Invoke();
            Close();
        }
    }

    private bool _isChanging;

    void OnValueChanged(string value)
    {
        if (_isChanging)
            return;

        if (Encoding.UTF8.GetByteCount(value) <= maxByte)
            return;

        _isChanging = true;

        Input.text = CutByLimit(value);
        Input.caretPosition = Input.text.Length;

        _isChanging = false;
    }

    string CutByLimit(string text)
    {
        StringBuilder result = new StringBuilder();

        int currentByte = 0;
        int currentLength = 0;

        foreach (char c in text)
        {
            int charByte = Encoding.UTF8.GetByteCount(c.ToString());

            if (currentByte + charByte > maxByte)
                break;

            if (currentLength + 1 > maxLength)
                break;

            result.Append(c);

            currentByte += charByte;
            currentLength++;
        }

        return result.ToString();
    }

    bool IsOverLimit(string text)
    {
        if (string.IsNullOrEmpty(text))
            return false;

        return text.Length > maxLength ||
               Encoding.UTF8.GetByteCount(text) > maxByte;
    }
}
