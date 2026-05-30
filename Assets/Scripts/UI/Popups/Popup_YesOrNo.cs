using UnityEngine;
using DiveCat.God.UI.Popups;
using TMPro;
using UnityEngine.UI;
using System;

public class Popup_YesOrNo : PopupBase
{
    [SerializeField] TextMeshProUGUI Title;
    [SerializeField] Button ExitBtn;
    [SerializeField] Button YesBtn;
    [SerializeField] Button NoBtn;

    Action _yesCallback;
    Action _noCallback;

    protected override void Awake()
    {
        ExitBtn.onClick.AddListener(OnClickExit);
        YesBtn.onClick.AddListener(OnClickYes);
        NoBtn.onClick.AddListener(OnClickNo);
    }

    public void SetPopup(string title, Action yesCallback, Action noCallback)
    {
        Title.text = title;
        _yesCallback = yesCallback;
        _noCallback = noCallback;
    }

    public void SetPopup(string title, Action yesCallback)
    {
        Title.text = title;
        _yesCallback = yesCallback;
        _noCallback = null;
    }

    void OnClickExit()
    {
        Close();
    }

    void OnClickYes()
    {
        _yesCallback?.Invoke();
    }

    void OnClickNo()
    {
        if (_noCallback != null)
        {
            _noCallback.Invoke();
        }
        else
        {
            OnClickExit();
        }
    }
}
