using UnityEngine;
using DiveCat.God.UI.Popups;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using System.Collections.Generic;
using System;
using System.Collections;

public class Popup_Gallery_Detail : PopupBase
{
    [SerializeField] Image BG;
    [SerializeField] TextMeshProUGUI IndexText;
    [SerializeField] TextMeshProUGUI TitleText;
    [SerializeField] GameObject[] HideObjects;
    

    [Header("Context")]
    [SerializeField] ScrollRect Scroll;
    [SerializeField] GameObject Context_Obj;
    List<Popup_Gallery_Detail_ContextItem> Context_Items = new List<Popup_Gallery_Detail_ContextItem>();

    [Header("Buttons")]
    [SerializeField] Button HideBtn;
    [SerializeField] Button ExitBtn;
    [SerializeField] Button ShowBtn;
    [SerializeField] Button LeftBtn;
    [SerializeField] Button RightBtn;
    [SerializeField] Popup_Gallery_Detail_ImageSaveBtn ImageSaveBtn;
    [SerializeField] Popup_Gallery_Detail_MusicBtn MusicBtn;

    List<Gallery_Data> _gallery_Datas = new List<Gallery_Data>();
    bool _isPlayMusic = false;
    int _currentPage;

    protected override void Awake()
    {
        ExitBtn.onClick.AddListener(() => Close());
        RightBtn.onClick.AddListener(OnClickNextPage);
        LeftBtn.onClick.AddListener(OnClickBeforePage);
        HideBtn.onClick.AddListener(OnClickHide);
        ShowBtn.onClick.AddListener(OnClickShow);
        base.Awake();
    }

    public override void Open(Action onComplete = null)
    {
        base.Open(onComplete);
    }

    public override void Close(Action onComplete = null)
    {
        base.Close(onComplete);
    }
    public void SetPopup(int group)
    {
        _gallery_Datas = new List<Gallery_Data>();
        var data = Data_Manager.Instance.GetGalleryGroupData(group);
        if (data != null)
        {
            for (int i = 0; i < data.Count; i++)
            {
                if (Data_Manager.Instance.IsOpenGallery(data[i].Index))
                {
                    _gallery_Datas.Add(data[i]);
                }
            }
            _currentPage = 0;
            SetUI();
        }
        else
        {
            Close();
        }
    }

    void SetUI()
    {
        var targetData = _gallery_Datas[_currentPage];
        BG.sprite = Resource_Manager.Instance.Get_BG(targetData.BG);
        IndexText.text = targetData.Index.ToString("00");
        TitleText.text = LanguageManager.Instance.GetText(targetData.TextKey);

        ImageSaveBtn.SetButton(OnClickImageSave);
        MusicBtn.SetButton(LanguageManager.Instance.GetText(targetData.Music), OnClickMusic);
        _isPlayMusic = false;

        SetContext(targetData.Start, targetData.End);
        SetPage();
        OnClickShow();
    }

    void SetContext(int start, int end)
    {
        ResetItems();

        var targets = Data_Manager.Instance.GetStoryData(start, end);
        for (int i = 0; i < targets.Count; i++)
        {
            GetItem().SetText(targets[i]);
        }
    }

    void SetPage()
    {
        if (_gallery_Datas.Count > 1)
        {
            if (_currentPage <= 0)
            {
                LeftBtn.gameObject.SetActive(false);
                RightBtn.gameObject.SetActive(true);
            }
            else if (_currentPage + 1 >= _gallery_Datas.Count)
            {
                LeftBtn.gameObject.SetActive(true);
                RightBtn.gameObject.SetActive(false);
            }
            else
            {
                LeftBtn.gameObject.SetActive(true);
                RightBtn.gameObject.SetActive(true);
            }
        }
        else
        {
            LeftBtn.gameObject.SetActive(false);
            RightBtn.gameObject.SetActive(false);
        }
    }

    void ResetItems()
    {
        for (int i = 0; i < Context_Items.Count; i++)
        {
            Context_Items[i].gameObject.SetActive(false);
        }
    }

    Popup_Gallery_Detail_ContextItem GetItem()
    {
        for (int i = 0; i < Context_Items.Count; i++)
        {
            if (!Context_Items[i].gameObject.activeSelf)
            {
                Context_Items[i].gameObject.SetActive(true);
                return Context_Items[i];
            }
        }

        var newItem = Instantiate(Context_Obj, Scroll.content);
        var script = newItem.GetComponent<Popup_Gallery_Detail_ContextItem>();
        Context_Items.Add(script);

        return script;
    }

    IEnumerator TakeScreenshotRoutine()
    {
        OnClickHide();
        HideBtn.gameObject.SetActive(false);
        ShowBtn.gameObject.SetActive(false);

        yield return new WaitForEndOfFrame();

        string fileName = $"Screenshot_{System.DateTime.Now:yyyyMMdd_HHmmss}.png";
        string path = System.IO.Path.Combine(Application.persistentDataPath, fileName);

        ScreenCapture.CaptureScreenshot(path);

        yield return new WaitForEndOfFrame();

        OnClickShow();

        var popup = Resource_Manager.Instance.Get_Yes_Or_No();
        if (popup != null)
        {
            //정환 로컬라이즈
            popup.Open();
            popup.SetPopup_One($"{path}에 이미지가 저장되었습니다.", () => popup.Close());
        }
        //Debug.Log($"스크린샷 저장 완료: {path}");
    }
//-------------------------------------------------------------------------------------------------------------------------------------
    #region ButtonAction
    void OnClickImageSave()
    {
        StartCoroutine(TakeScreenshotRoutine());
    }

    void OnClickMusic()
    {
        _isPlayMusic = !_isPlayMusic;
        MusicBtn.SetPlay(_isPlayMusic);
    }

    void OnClickNextPage()
    {
        _currentPage++;
        if (_currentPage >= _gallery_Datas.Count)
        {
            _currentPage = _gallery_Datas.Count - 1;
        }
        SetUI();
    }

    void OnClickBeforePage()
    {
        _currentPage--;
        if (_currentPage <= 0)
        {
            _currentPage = 0;
        }
        SetUI();
    }

    void OnClickShow()
    {
        HideBtn.gameObject.SetActive(true);
        ShowBtn.gameObject.SetActive(false);
        for (int i = 0; i < HideObjects.Length; i++)
        {
            HideObjects[i].gameObject.SetActive(true);
        }
    }

    void OnClickHide()
    {
        HideBtn.gameObject.SetActive(false);
        ShowBtn.gameObject.SetActive(true);
        for (int i = 0; i < HideObjects.Length; i++)
        {
            HideObjects[i].gameObject.SetActive(false);
        }
    }
    #endregion
}
