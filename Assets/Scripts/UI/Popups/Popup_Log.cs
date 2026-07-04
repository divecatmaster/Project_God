using UnityEngine;
using DiveCat.God.UI.Popups;
using System;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using God.Audio;

public class Popup_Log : PopupBase
{
    [SerializeField] Button CloseBtn;
    [SerializeField] GameObject ItemObj;
    [SerializeField] ScrollRect Scroll;
    [SerializeField] TextMeshProUGUI MapName;

    List<Popup_Log_Item> _itemList = new List<Popup_Log_Item>();

    protected override void Awake()
    {
        CloseBtn.onClick.AddListener(OnClickClose);
    }

    public override void Open(Action onComplete = null)
    {
        SoundManager.Instance.PlayUI("Popup_Open");
        base.Open(onComplete);
    }

    public void SetItems(int currentIndex)
    {
        ResetItems();

        var target = Data_Manager.Instance.GetLogData(currentIndex);
        MapName.text = LanguageManager.Instance.GetText($"BG_Title_{target[0].BG}");

        int _tempBG = -1;
        for (int i = 0; i < target.Count; i++)
        {
            if (_tempBG != -1 && _tempBG != target[i].BG)
            {
                GetItem().SetItemBG(target[i]);
            }

            _tempBG = target[i].BG;

            if (target[i].Language_Key == "Story_Empty" || target[i].Auto_Next)
                continue;
            
            GetItem().SetItem(target[i]);
        }

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)Scroll.content);
        Scroll.verticalNormalizedPosition = 0f;
    }

    void ResetItems()
    {
        for (int i = 0; i < _itemList.Count; i++)
        {
            _itemList[i].gameObject.SetActive(false);
        }
    }

    Popup_Log_Item GetItem()
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
        var script = newItem.GetComponent<Popup_Log_Item>();
        _itemList.Add(script);

        return script;
    }

    void OnClickClose()
    {
        Close();
    }
}
