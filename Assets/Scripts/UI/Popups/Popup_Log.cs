using UnityEngine;
using DiveCat.God.UI.Popups;
using System;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using God.Audio;

public class Popup_Log : PopupBase
{
    private struct LogItemData
    {
        public Story_Data story;
        public bool isBG;
    }

    [SerializeField] Button CloseBtn;
    [SerializeField] GameObject ItemObj;
    [SerializeField] ScrollRect Scroll;
    //[SerializeField] TextMeshProUGUI MapName;

    List<Popup_Log_Item> _itemList = new List<Popup_Log_Item>();
    private List<LogItemData> _displayData = new List<LogItemData>();
    private Dictionary<int, Popup_Log_Item> _activeItems = new Dictionary<int, Popup_Log_Item>();

    private float _itemHeight = 100f;
    private float _spacing = 21f;
    private float _paddingTop = 2f;
    private float _paddingBottom = 20f;

    private Vector2 _lastContentPos;
    private float _lastViewportHeight;
    private bool _needScrollToBottom;
    private bool _isLayoutCached;

    protected override void Awake()
    {
        CloseBtn.onClick.AddListener(OnClickClose);
        CacheLayoutValues();
    }

    private void CacheLayoutValues()
    {
        if (_isLayoutCached) return;

        if (Scroll != null && Scroll.content != null)
        {
            var contentGroup = Scroll.content.GetComponent<VerticalLayoutGroup>();
            if (contentGroup != null)
            {
                _spacing = contentGroup.spacing;
                _paddingTop = contentGroup.padding.top;
                _paddingBottom = contentGroup.padding.bottom;
                contentGroup.enabled = false;
            }

            var contentFitter = Scroll.content.GetComponent<ContentSizeFitter>();
            if (contentFitter != null)
            {
                contentFitter.enabled = false;
            }
        }

        if (ItemObj != null)
        {
            var rect = ItemObj.GetComponent<RectTransform>();
            if (rect != null)
            {
                _itemHeight = rect.sizeDelta.y;
            }
        }

        _isLayoutCached = true;
    }

    public override void Open(Action onComplete = null)
    {
        SoundManager.Instance.PlayUI("Popup_Open");
        SoundManager.Instance.SetMute(SoundCategory.SFX, true);
        SoundManager.Instance.SetMute(SoundCategory.BGM, true);
        Data_Manager.Instance.AddLogCount();
        base.Open(onComplete);
    }

    public override void Close(Action onComplete = null)
    {
        SoundManager.Instance.PlayUI("Click");
        SoundManager.Instance.SetMute(SoundCategory.SFX, false);
        SoundManager.Instance.SetMute(SoundCategory.BGM, false);
        base.Close(onComplete);
    }

    public void SetItems(int currentIndex)
    {
        CacheLayoutValues();

        _displayData.Clear();

        var target = Data_Manager.Instance.GetLogData(currentIndex);
        if (target == null || target.Count == 0)
        {
            if (Scroll != null && Scroll.content != null)
            {
                Scroll.content.sizeDelta = new Vector2(Scroll.content.sizeDelta.x, 0f);
            }
            ClearActiveItems();
            return;
        }

        _displayData.Add(new LogItemData { story = target[0], isBG = true });
        int _tempBG = target[0].BG;
        for (int i = 0; i < target.Count; i++)
        {
            if (_tempBG != -1 && _tempBG != target[i].BG)
            {
                if (target[i].BG < 100 && _tempBG < 100)
                {
                    _displayData.Add(new LogItemData { story = target[i], isBG = true });
                }
            }

            _tempBG = target[i].BG;

            if (target[i].Language_Key == "Story_Empty" || target[i].Auto_Next)
                continue;
            
            _displayData.Add(new LogItemData { story = target[i], isBG = false });
        }

        // Calculate total content height
        int N = _displayData.Count;
        float totalHeight = 0f;
        if (N > 0)
        {
            totalHeight = _paddingTop + _paddingBottom + (N * _itemHeight) + ((N - 1) * _spacing);
        }

        if (Scroll != null && Scroll.content != null)
        {
            Scroll.content.sizeDelta = new Vector2(Scroll.content.sizeDelta.x, totalHeight);
        }

        ClearActiveItems();

        _needScrollToBottom = true;
        UpdateVisibleItems();
    }

    private void ClearActiveItems()
    {
        foreach (var item in _itemList)
        {
            if (item != null)
            {
                item.gameObject.SetActive(false);
            }
        }
        _activeItems.Clear();
    }

    private void UpdateVisibleItems()
    {
        if (Scroll == null || Scroll.content == null) return;

        if (_displayData.Count == 0)
        {
            ClearActiveItems();
            return;
        }

        float viewportHeight = Scroll.viewport != null ? Scroll.viewport.rect.height : ((RectTransform)Scroll.transform).rect.height;
        if (viewportHeight <= 0)
        {
            viewportHeight = 1080f; // safe default
        }

        float topDistance = Scroll.content.anchoredPosition.y;
        float maxScroll = Mathf.Max(0f, Scroll.content.rect.height - viewportHeight);
        topDistance = Mathf.Clamp(topDistance, 0f, maxScroll);

        float buffer = 150f; // slightly larger than one item to avoid gaps

        int startIndex = Mathf.FloorToInt((topDistance - buffer - _paddingTop - _itemHeight) / (_itemHeight + _spacing));
        int endIndex = Mathf.FloorToInt((topDistance + viewportHeight + buffer - _paddingTop) / (_itemHeight + _spacing));

        startIndex = Mathf.Clamp(startIndex, 0, _displayData.Count - 1);
        endIndex = Mathf.Clamp(endIndex, 0, _displayData.Count - 1);

        // Recycle out-of-bounds items
        List<int> keysToRemove = new List<int>();
        foreach (var kp in _activeItems)
        {
            int index = kp.Key;
            if (index < startIndex || index > endIndex)
            {
                if (kp.Value != null)
                {
                    kp.Value.gameObject.SetActive(false);
                }
                keysToRemove.Add(index);
            }
        }
        foreach (int key in keysToRemove)
        {
            _activeItems.Remove(key);
        }

        // Bind and position visible items
        for (int i = startIndex; i <= endIndex; i++)
        {
            if (!_activeItems.ContainsKey(i))
            {
                Popup_Log_Item item = GetUnusedItem();
                if (item == null) continue;

                var data = _displayData[i];
                if (data.isBG)
                {
                    item.SetItemBG(data.story);
                }
                else
                {
                    item.SetItem(data.story);
                }

                RectTransform itemRt = (RectTransform)item.transform;
                itemRt.anchorMin = new Vector2(0.5f, 1f);
                itemRt.anchorMax = new Vector2(0.5f, 1f);
                itemRt.pivot = new Vector2(0.5f, 1f);
                itemRt.sizeDelta = new Vector2(1620f, _itemHeight);

                float posY = - (_paddingTop + i * (_itemHeight + _spacing));
                itemRt.anchoredPosition = new Vector2(0f, posY);

                item.gameObject.SetActive(true);
                _activeItems[i] = item;
            }
        }
    }

    private Popup_Log_Item GetUnusedItem()
    {
        for (int i = 0; i < _itemList.Count; i++)
        {
            if (_itemList[i] != null && !_itemList[i].gameObject.activeSelf && !_activeItems.ContainsValue(_itemList[i]))
            {
                return _itemList[i];
            }
        }

        if (ItemObj == null || Scroll == null || Scroll.content == null) return null;

        var newItem = Instantiate(ItemObj, Scroll.content);
        var script = newItem.GetComponent<Popup_Log_Item>();
        if (script != null)
        {
            _itemList.Add(script);
        }
        return script;
    }

    private void Update()
    {
        if (Scroll != null && Scroll.content != null)
        {
            float currentViewportHeight = Scroll.viewport != null ? Scroll.viewport.rect.height : ((RectTransform)Scroll.transform).rect.height;

            if (_needScrollToBottom && currentViewportHeight > 0)
            {
                _needScrollToBottom = false;
                Canvas.ForceUpdateCanvases();
                Scroll.verticalNormalizedPosition = 0f;
                _lastContentPos = Scroll.content.anchoredPosition;
                _lastViewportHeight = currentViewportHeight;
                UpdateVisibleItems();
                return;
            }

            Vector2 currentPos = Scroll.content.anchoredPosition;
            if (currentPos != _lastContentPos || currentViewportHeight != _lastViewportHeight)
            {
                _lastContentPos = currentPos;
                _lastViewportHeight = currentViewportHeight;
                UpdateVisibleItems();
            }
        }
    }

    void OnClickClose()
    {
        Close();
    }
}
