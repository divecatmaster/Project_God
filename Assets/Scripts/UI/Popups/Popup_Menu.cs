using DiveCat.God.UI.Popups;
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

    void OnClickContinue()
    {
        Close();
    }

    void OnClickSave()
    {
        StoryManager.Instance.OnClickSave();
    }

    void OnClickSetting()
    {

    }

    void OnClickMainBtn()
    {
        var popup = Resource_Manager.Instance.Get_Yes_Or_No();
        popup.Open();
        popup.SetPopup(LanguageManager.Instance.GetText("Menu_Warning_1"), ()=>
        {
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
