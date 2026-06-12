using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using DiveCat.God.UI.Popups;

public class Popup_Gallery : PopupBase
{
    [SerializeField] Button CloseBtn;
    [SerializeField] Transform ItemTrans;
    [SerializeField] GameObject Item_Obj;

    List<Popup_Gallery_Item> _itemList = new List<Popup_Gallery_Item>();
    List<Gallery_Data> _galleryData = new List<Gallery_Data>();
    int _totalPage = 0;
    int _totalCount = 0;
    int _currentPage = 0;
    protected override void Awake()
    {
        CloseBtn.onClick.AddListener(OnClickExit);
        base.Awake();
        InitData();
    }

    void InitData()
    {
        _galleryData = Data_Manager.Instance.GetGalleryData();
        _totalCount = _galleryData.Count;
        _totalPage = Mathf.CeilToInt(_galleryData.Count / 9);
        _currentPage = 1;
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
        
    }

    void OnClickExit()
    {
        Close();
    }
}
