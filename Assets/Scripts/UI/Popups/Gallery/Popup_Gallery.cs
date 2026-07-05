using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using DiveCat.God.UI.Popups;
using TMPro;
using System;
using God.Audio;

public class Popup_Gallery : PopupBase
{
    [SerializeField] Button CloseBtn;
    [SerializeField] Transform ItemTrans;
    [SerializeField] GameObject Item_Obj;
    [SerializeField] Button Arrow_Left;
    [SerializeField] Button Arrow_Right;
    [SerializeField] Image Dot_1;
    [SerializeField] Image Dot_2;
    [SerializeField] Popup_Gallery_Detail Detail_Popup;
    [SerializeField] TextMeshProUGUI Percent;

    List<Popup_Gallery_Item> _itemList = new List<Popup_Gallery_Item>();
    List<Gallery_Data> _galleryData = new List<Gallery_Data>();
    int _totalPage = 0;
    int _totalCount = 0;
    int _currentPage = 0;
    protected override void Awake()
    {
        CloseBtn.onClick.AddListener(OnClickExit);
        Arrow_Left.onClick.AddListener(OnClickLeft);
        Arrow_Right.onClick.AddListener(OnClickRight);
        base.Awake();
    }

    public override void Open(Action onComplete = null)
    {
        SoundManager.Instance.PlayUI("Popup_Open");
        SoundManager.Instance.StopBGM();
        base.Open(onComplete);
    }

    public override void Close(Action onComplete = null)
    {
        SoundManager.Instance.PlayBGM("The Shade of a Tree");
        SoundManager.Instance.PlayUI("Click");
        base.Close(onComplete);
    }

    public override void CloseFast(Action onComplete = null)
    {
        SoundManager.Instance.PlayBGM("The Shade of a Tree");
        SoundManager.Instance.PlayUI("Click");
        base.CloseFast(onComplete);
    }

    void OnEnable()
    {
        InitData();
    }

    void InitData()
    {
        if (_galleryData.Count <= 0)
        {
            _galleryData = Data_Manager.Instance.GetGalleryGroupData();    
        }
        _totalCount = _galleryData.Count;
        _totalPage = Mathf.CeilToInt((float)_galleryData.Count / 9f);
        _currentPage = 0;
        Percent.text = $"{Data_Manager.Instance.GetGalleryPercent()}<size=30>%</size>";
        SetPage();
    }

    void SetPage()
    {
        ResetItems();

        for (int i = 0; i < 9; i++)
        {
            var idx = i + 1 + (_currentPage * 9);
            if (idx > _totalCount)
            {
                break;
            }
            else
            {
                GetItem().SetItem(i, _galleryData[idx - 1], OnClickItem);
            }
        }

        SetArrow();
    }

    void SetArrow()
    {
        if (_currentPage == 0)
        {
            Arrow_Left.gameObject.SetActive(false);
            Arrow_Right.gameObject.SetActive(true);

            Dot_1.color = UIUtility.Gallery_Dot_On_Color;
            Dot_2.color = UIUtility.Gallery_Dot_Off_Color;
        }
        else
        {
            Arrow_Left.gameObject.SetActive(true);
            Arrow_Right.gameObject.SetActive(false);

            Dot_1.color = UIUtility.Gallery_Dot_Off_Color;
            Dot_2.color = UIUtility.Gallery_Dot_On_Color;
        }
    }

    void ResetItems()
    {
        for (int i = 0; i < _itemList.Count; i++)
        {
            _itemList[i].gameObject.SetActive(false);
        }
    }

    Popup_Gallery_Item GetItem()
    {
        for (int i = 0; i < _itemList.Count; i++)
        {
            if (!_itemList[i].gameObject.activeSelf)
            {
                _itemList[i].gameObject.SetActive(true);
                return _itemList[i];
            }
        }

        var newItem = Instantiate(Item_Obj, ItemTrans);
        var script = newItem.GetComponent<Popup_Gallery_Item>();
        _itemList.Add(script);

        return script;
    }

    void OnClickItem(int idx)
    {
        var targetData = _galleryData[idx + (_currentPage * 9)];

        if (Data_Manager.Instance.IsOpenGallery(targetData.Index))
        {
            Detail_Popup.SetPopup(targetData.Group);
            Detail_Popup.gameObject.SetActive(true);
            Detail_Popup.Open();
        }
    }

    void OnClickLeft()
    {
        _currentPage--;
        if (_currentPage < 0)
        {
            _currentPage = 0;
        }

        SetPage();
    }

    void OnClickRight()
    {
        _currentPage++;
        if (_currentPage >= 2)
        {
            _currentPage = 1;
        }

        SetPage();
    }

    void OnClickExit()
    {
        Close();
    }
}
