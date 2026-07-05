using System;
using DiveCat.God.UI.Popups;
using God.Audio;
using UnityEngine;
using UnityEngine.UI;

public class Popup_Setting : PopupBase
{
    [SerializeField] Button ExitBtn;
    [SerializeField] Popup_Setting_Group[] GroupBtns;
    [SerializeField] GameObject[] Panels;
    

    int _currentGroup = -1;
    bool _isInit;

    protected override void Awake()
    {
        ExitBtn.onClick.AddListener(OnClickExit);
        base.Awake();
    }

    public override void Open(Action onComplete = null)
    {
        SoundManager.Instance.PlayUI("Popup_Open");
        SetGroups();
        base.Open(onComplete);
    }

    public override void Close(Action onComplete = null)
    {
        SoundManager.Instance.PlayUI("Click");
        base.Close(onComplete);
    }

    void SetGroups()
    {
        _currentGroup = -1;
        _isInit = true;
        for (int i = 0; i < GroupBtns.Length; i++)
        {
            GroupBtns[i].SetButton(i, OnClickGroup);
            Panels[i].SetActive(false);
        }
        
        OnClickGroup(0);
    }

    void OnClickGroup(int idx)
    {
        if (idx == _currentGroup) return;

        if (_currentGroup != -1)
        {
            GroupBtns[_currentGroup].SetSelected(false, false);
            Panels[_currentGroup].SetActive(false);
        }
        _currentGroup = idx;

        if (_isInit)
        {
            GroupBtns[idx].SetSelected(true);
        }
        else
        {
            GroupBtns[idx].SetSelected(true, false);
        }
        _isInit = false;

        Panels[idx].SetActive(true);
    }

    void OnClickExit()
    {
        //변경사항 있는지 체크
        Close();
    }
}

