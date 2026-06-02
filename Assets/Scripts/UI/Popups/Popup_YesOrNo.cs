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
    [SerializeField] GameObject YesOrNo;
    [SerializeField] GameObject Confirm;
    [SerializeField] Button Confirm_Btn;

    Action _yesCallback;
    Action _noCallback;
    Action _confirmCallback;

    protected override void Awake()
    {
        ExitBtn.onClick.AddListener(OnClickExit);
        YesBtn.onClick.AddListener(OnClickYes);
        NoBtn.onClick.AddListener(OnClickNo);
        Confirm_Btn.onClick.AddListener(OnClickConfirm);

    }

    public override void Open(Action onComplete = null)
    {
        transform.SetAsLastSibling();
        base.Open(onComplete);
    }

    public void SetPopup(string title, Action yesCallback, Action noCallback)
    {
        Title.text = title;
        _yesCallback = yesCallback;
        _noCallback = noCallback;
        _confirmCallback = null;
        YesOrNo.SetActive(true);
        Confirm.SetActive(false);
    }

    public void SetPopup(string title, Action yesCallback)
    {
        Title.text = title;
        _yesCallback = yesCallback;
        _noCallback = null;
        _confirmCallback = null;
        YesOrNo.SetActive(true);
        Confirm.SetActive(false);
    }

    public void SetPopup_One(string title, Action confirmCallback)
    {
        Title.text = title;
        _yesCallback = null;
        _noCallback = null;
        _confirmCallback = confirmCallback;
        YesOrNo.SetActive(false);
        Confirm.SetActive(true);
    }

    public override void Close(Action onComplete = null)
    {
        _yesCallback = null;
        _noCallback = null;
        _confirmCallback = null;

        base.Close(onComplete);
    }

    public override void CloseByEscape()
    {
        Cancel();
    }

    public void Cancel()
    {
        _noCallback?.Invoke();
        Close();
    }

    void OnClickExit()
    {
        _noCallback?.Invoke();
        Close();
    }

    void OnClickYes()
    {
        _yesCallback?.Invoke();
        Close();
    }

    void OnClickNo()
    {
        _noCallback?.Invoke();
        Close();
    }

    void OnClickConfirm()
    {
        _confirmCallback?.Invoke();
        Close();
    }
}
