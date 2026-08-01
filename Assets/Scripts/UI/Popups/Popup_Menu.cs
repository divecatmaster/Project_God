using System;
using DiveCat.God.UI.Popups;
using God.Audio;
using UnityEngine;
using UnityEngine.UI;

public class Popup_Menu : PopupBase
{
    [SerializeField] Button ExitBtn;
    [SerializeField] Button ContinueBtn;
    [SerializeField] Button SaveBtn;
    [SerializeField] Button SettingBtn;
    [SerializeField] Button MainBtn;
    [SerializeField] Button EndBtn;

    protected override void Awake()
    {
        ExitBtn.onClick.AddListener(() => Close());
        ContinueBtn.onClick.AddListener(OnClickContinue);
        SaveBtn.onClick.AddListener(OnClickSave);
        SettingBtn.onClick.AddListener(OnClickSetting);
        MainBtn.onClick.AddListener(OnClickMainBtn);
        EndBtn.onClick.AddListener(OnClickEnd);

        base.Awake();
    }

    public override void Open(Action onComplete = null)
    {
        SoundManager.Instance.PlayUI("Popup_Open");
        base.Open(onComplete);
        SoundManager.Instance.SetMute(SoundCategory.SFX, true);
        SoundManager.Instance.SetMute(SoundCategory.BGM, true);
    }

    public override void Close(Action onComplete = null)
    {
        SoundManager.Instance.PlayUI("Click");
        SoundManager.Instance.SetMute(SoundCategory.SFX, false);
        SoundManager.Instance.SetMute(SoundCategory.BGM, false);
        base.Close(onComplete);
    }

    void OnClickContinue()
    {
        Close();
    }

    void OnClickSave()
    {
        StoryManager.Instance.OnClickSave();
    }

    Popup_Setting _popup_Setting;
    void OnClickSetting()
    {
        if (_popup_Setting == null)
        {
            var target = Resources.Load<GameObject>("Popup/Popup_Setting");
            if (target != null)
            {
                var item = Instantiate(target, StoryManager.Instance.Popup_Trans);
                _popup_Setting = item.GetComponent<Popup_Setting>();
            }
        }

        if (_popup_Setting != null)
        {
            _popup_Setting.Open();
            _popup_Setting.OpenInGame(true);
            SoundManager.Instance.SetMute(SoundCategory.SFX, false);
            SoundManager.Instance.SetMute(SoundCategory.BGM, false);
        }
    }

    void OnClickMainBtn()
    {
        var popup = Resource_Manager.Instance.Get_Yes_Or_No();
        popup.Open();
        popup.SetPopup(LanguageManager.Instance.GetText("Menu_Warning_1"), ()=>
        {
            SoundManager.Instance.StopBGM();
            SoundManager.Instance.StopSFX();
            SoundManager.Instance.SetMute(SoundCategory.SFX, false);
            SoundManager.Instance.SetMute(SoundCategory.BGM, false);
            GameSceneManager.Instance.GoToMainScene();
        });
    }

    void OnClickEnd()
    {
        var popup = Resource_Manager.Instance.Get_Yes_Or_No();
        popup.Open();
        popup.SetPopup(LanguageManager.Instance.GetText("Menu_Warning_2"), () =>
        {
            Application.Quit();
        });
    }
}
