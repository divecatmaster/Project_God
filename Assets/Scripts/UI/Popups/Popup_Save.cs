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

        var item_1 = GetItem();
        var saveData = new Save_Data();
        saveData.SlotIndex = 1;
        saveData.StoryIndex = 23;
        saveData.SaveDate = new DateTime(2026, 05, 19, 13, 39, 00);
        saveData.PlayTime = new TimeSpan(0,0,30,10);
        item_1.SetItem(_currentGroup, saveData, OnClickItem);

        var item_2 = GetItem();
        saveData = new Save_Data();
        saveData.SlotIndex = 2;
        saveData.StoryIndex = -1;
        saveData.SaveDate = DateTime.MinValue;
        saveData.PlayTime = new TimeSpan();
        item_2.SetItem(_currentGroup, saveData, OnClickItem);

        var item_3 = GetItem();
        saveData = new Save_Data();
        saveData.SlotIndex = 3;
        saveData.StoryIndex = 128;
        saveData.SaveDate = new DateTime(2026, 05, 29, 13, 39, 00);
        saveData.PlayTime = new TimeSpan(0,1,28,10);
        item_3.SetItem(_currentGroup, saveData, OnClickItem);

        var item_4 = GetItem();
        saveData = new Save_Data();
        saveData.SlotIndex = 4;
        saveData.StoryIndex = -1;
        saveData.SaveDate = DateTime.MinValue;
        saveData.PlayTime = new TimeSpan();
        item_4.SetItem(_currentGroup, saveData, OnClickItem);

        var item_5 = GetItem();
        saveData = new Save_Data();
        saveData.SlotIndex = 5;
        saveData.StoryIndex = -1;
        saveData.SaveDate = DateTime.MinValue;
        saveData.PlayTime = new TimeSpan();
        item_5.SetItem(_currentGroup, saveData, OnClickItem);
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
        
    }

    void OnClickClose()
    {
        Close();
    }
}
