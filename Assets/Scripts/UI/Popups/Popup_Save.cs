using UnityEngine;
using DiveCat.God.UI.Popups;
using System;
using UnityEngine.UI;
using System.Collections.Generic;

public class Popup_Save : PopupBase
{
    [SerializeField] Button CloseBtn;
    [SerializeField] Popup_Save_Group[] Groups;

    [SerializeField] GameObject ItemObj;
    [SerializeField] ScrollRect Scroll;

    List<Popup_Save_Item> _itemList = new List<Popup_Save_Item>();
    int _currentGroup = -1;
    int _openType = -1;//0=main, 1=game
    protected override void Awake()
    {
        CloseBtn.onClick.AddListener(OnClickClose);
    }

    public override void Open(Action onComplete = null)
    {
        base.Open(onComplete);
    }

    public override void Close(Action onComplete = null)
    {
        base.Close(onComplete);
        _currentGroup = -1;
    }

    public void SetPopup(int type)
    {
        _openType = type;
        for (int i = 0; i < Groups.Length; i++)
        {
            Groups[i].SetButton(i, OnClickGroup);
        }

        if (type == 0)//메인화면에서 로드만
        {
            Groups[0].gameObject.SetActive(false);
            Groups[1].gameObject.SetActive(true);
            OnClickGroup(1);
        }
        else
        {
            Groups[0].gameObject.SetActive(true);
            Groups[1].gameObject.SetActive(true);
            OnClickGroup(0);
        }
    }

    void OnClickGroup(int idx)
    {
        if (_currentGroup != -1)
        {
            if (_currentGroup == idx) return;
            
            Groups[_currentGroup].SetSelected(false);
        }
        _currentGroup = idx;
        Groups[_currentGroup].SetSelected(true);
        SetItems();
    }

    void SetItems()
    {
        ResetItems();

        var saveData = Data_Manager.Instance.GetAllSaveData();
        for (int i = 0; i < 20; i++)
        {
            var item = GetItem();
            if (saveData.ContainsKey(i + 1))
            {
                item.SetItem(_currentGroup, saveData[i + 1], OnClickItem, OnClickRemove);
            }
            else
            {
                var newData = new Save_Data();
                newData.SlotIndex = i + 1;
                item.SetItem(_currentGroup, newData, OnClickItem, OnClickRemove);
            }
        }
    }

    void ResetItems()
    {
        for (int i = 0; i < _itemList.Count; i++)
        {
            _itemList[i].gameObject.SetActive(false);
        }
    }

    Popup_Save_Item GetItem()
    {
        for (int i = 0; i < _itemList.Count; i++)
        {
            if (!_itemList[i].gameObject.activeSelf)
            {
                _itemList[i].gameObject.SetActive(true);
                return _itemList[i];
            }
        }

        var newItem = Instantiate(ItemObj, Scroll.content);
        var script = newItem.GetComponent<Popup_Save_Item>();
        _itemList.Add(script);

        return script;
    }

    void OnClickItem(int slotIdx)
    {
        if (_currentGroup == 1)//로드
        {
            var target = Data_Manager.Instance.GetSaveData(slotIdx);
            if (target == null)
            {
                var popup = Resource_Manager.Instance.Get_Yes_Or_No();
                popup.Open();
                popup.SetPopup_One(LanguageManager.Instance.GetText("Save_Warning_4"), () =>
                {
                    
                });
            }
            else
            {
                var popup = Resource_Manager.Instance.Get_Yes_Or_No();
                popup.Open();
                popup.SetPopup(string.Format(LanguageManager.Instance.GetText("Save_Warning_5"), slotIdx), () =>
                {
                    if (_openType == 0)//메인화면
                    {
                        Data_Manager.Instance.SetSaveStory_Index(target.StoryIndex);
                        MainSceneManager.Instance.OnClickGame();
                        Data_Manager.Instance.StartTimer(target.PlayTime);
                    }
                    else//인게임
                    {
                        Data_Manager.Instance.Set_TempIndex(slotIdx);
                        Data_Manager.Instance.StopTimer();
                        GameSceneManager.Instance.StartLoading();
                    }
                });
            }
        }
        else//세이브
        {
            var target = Data_Manager.Instance.GetSaveData(slotIdx);
            if (target == null)
            {
                var popup = Resource_Manager.Instance.Get_Yes_Or_No();
                popup.Open();
                popup.SetPopup(string.Format(LanguageManager.Instance.GetText("Save_Warning_1"), slotIdx), () =>
                {
                    var newData = new Save_Data();
                    newData.SlotIndex = slotIdx;
                    newData.StoryIndex = Data_Manager.Instance.SaveStory_Index;
                    newData.SaveDate = DateTime.Now;
                    newData.PlayTime = Data_Manager.Instance.GetPlayTime();
                    Data_Manager.Instance.SetSaveData(newData);
                    _itemList[slotIdx - 1].SetItem(_currentGroup, newData, OnClickItem, OnClickRemove);
                });
            }
            else
            {
                var popup = Resource_Manager.Instance.Get_Yes_Or_No();
                popup.Open();
                popup.SetPopup(LanguageManager.Instance.GetText("Save_Warning_2"), () =>
                {
                    var newData = new Save_Data();
                    newData.SlotIndex = slotIdx;
                    newData.StoryIndex = Data_Manager.Instance.SaveStory_Index;
                    newData.SaveDate = DateTime.Now;
                    newData.PlayTime = Data_Manager.Instance.GetPlayTime();
                    Data_Manager.Instance.SetSaveData(newData);
                    _itemList[slotIdx - 1].SetItem(_currentGroup, newData, OnClickItem, OnClickRemove);
                });
            }
        }
    }

    void OnClickRemove(int slotIdx)
    {
        var target = Data_Manager.Instance.GetSaveData(slotIdx);
        if (target == null)
        {
            var popup = Resource_Manager.Instance.Get_Yes_Or_No();
            popup.Open();
            popup.SetPopup_One(LanguageManager.Instance.GetText("Save_Warning_4"), () =>
            {
                
            });
        }
        else
        {
            var popup = Resource_Manager.Instance.Get_Yes_Or_No();
            popup.Open();
            popup.SetPopup(string.Format(LanguageManager.Instance.GetText("Save_Warning_3"), slotIdx), () =>
            {
                Data_Manager.Instance.RemoveSaveData(slotIdx);
                var tempData = new Save_Data();
                tempData.SlotIndex = slotIdx;
                _itemList[slotIdx - 1].SetItem(_currentGroup, tempData, OnClickItem, OnClickRemove);
            });
        }
    }

    void OnClickClose()
    {
        Close();
    }
}
