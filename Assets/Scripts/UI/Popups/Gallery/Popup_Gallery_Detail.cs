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
    [SerializeField] Image FadeBG;
    [SerializeField] TextMeshProUGUI IndexText;
    [SerializeField] TextMeshProUGUI TitleText;
    [SerializeField] GameObject[] HideObjects;
    [SerializeField] Image[] Dots;
    [SerializeField] Material[] BG_Fade_Material;
    

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
    Tween _bgFadeTween;
    Tween _bgFeatherTween;
    Material _bgFade_Ver;
    Material _bgFade_Hor;

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
        Material mat = BG_Fade_Material[1];
        BG.material = mat;
        mat.SetFloat("_FadeProgress", 0f);
        mat.SetFloat("_Feather", 0f);
        mat.DisableKeyword("REVERSE_DIRECTION");
        _bgFadeTween = DOTween.To(
            () => mat.GetFloat("_FadeProgress"),
            x => mat.SetFloat("_FadeProgress", x),
            1f,
            0.25f)
        .SetEase(Ease.Linear);
        //.OnComplete(ResetFadeBG);

        _bgFeatherTween = DOTween.To(
                () => mat.GetFloat("_Feather"),
                x => mat.SetFloat("_Feather", x),
                1f,
                0.25f)
            .SetEase(Ease.Linear);

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
            SetUI(true);

            Material mat = BG_Fade_Material[1];
            BG.material = mat;
            mat.SetFloat("_FadeProgress", 1f);
            mat.SetFloat("_Feather", 1f);
            mat.DisableKeyword("REVERSE_DIRECTION");
            _bgFadeTween = DOTween.To(
                () => mat.GetFloat("_FadeProgress"),
                x => mat.SetFloat("_FadeProgress", x),
                0f,
                0.25f)
            .SetEase(Ease.Linear);
            //.OnComplete(ResetFadeBG);

            _bgFeatherTween = DOTween.To(
                    () => mat.GetFloat("_Feather"),
                    x => mat.SetFloat("_Feather", x),
                    0f,
                    0.25f)
                .SetEase(Ease.Linear);
        }
        else
        {
            Close();
        }
    }

    void SetUI(bool isInit = false)
    {
        var targetData = _gallery_Datas[_currentPage];
        if (!isInit)
        {
            var targetStory = Data_Manager.Instance.GetStoryData(targetData.Production);
            if (targetStory != null)
            {
                ChangeBG(targetStory);
            }
            else
            {
                BG.sprite = Resource_Manager.Instance.Get_BG(targetData.BG);
            }
        }
        else
        {
            BG.sprite = Resource_Manager.Instance.Get_BG(targetData.BG);
        }

        IndexText.text = targetData.Index.ToString("00");
        TitleText.text = LanguageManager.Instance.GetText(targetData.TextKey);

        ImageSaveBtn.SetButton(OnClickImageSave);
        MusicBtn.SetButton(LanguageManager.Instance.GetText(targetData.Music), OnClickMusic);
        _isPlayMusic = false;

        SetContext(targetData.Start, targetData.End);
        SetPage();
        OnClickShow();
    }

    void ChangeBG(Story_Data data)
    {
        KillBGTweens();
        var targetData = _gallery_Datas[_currentPage];
        var sprite = Resource_Manager.Instance.Get_BG(targetData.BG);

        // if (sprite == null)
        // {
        //     var targetData = _gallery_Datas[_currentPage];
        //     BG.sprite = Resource_Manager.Instance.Get_BG(targetData.BG);
        //     return;
        // }

        FadeBG.material = null;
        FadeBG.sprite = sprite;
        FadeBG.color = UIUtility.Common_Off_Color;
        FadeBG.rectTransform.localScale = Vector3.one;

        int zoomIndex = data.Appear_Production.IndexOf(3);
        int leftIndex = data.Appear_Production.IndexOf(4);
        int rightIndex = data.Appear_Production.IndexOf(5);
        int upIndex = data.Appear_Production.IndexOf(6);
        int downIndex = data.Appear_Production.IndexOf(7);
        int normalIndex = data.Appear_Production.IndexOf(8);

        if (zoomIndex >= 0 && zoomIndex < data.Appear_Production_Time.Count)
        {
            float zoomDuration = data.Appear_Production_Time[zoomIndex];

            FadeBG.rectTransform.localScale = Vector3.one * 1.05f;

            FadeBG.rectTransform
                .DOScale(1f, zoomDuration)
                .SetEase(Ease.OutQuad);
        }

        if (leftIndex >= 0 && leftIndex < data.Appear_Production_Time.Count)
        {
            PlayDirectionalBGFade(
                sprite,
                ref _bgFade_Hor,
                BG_Fade_Material[1],
                false,
                data.Appear_Production_Time[leftIndex]);

            return;
        }

        if (rightIndex >= 0 && rightIndex < data.Appear_Production_Time.Count)
        {
            PlayDirectionalBGFade(
                sprite,
                ref _bgFade_Hor,
                BG_Fade_Material[1],
                true,
                data.Appear_Production_Time[rightIndex]);

            return;
        }

        if (upIndex >= 0 && upIndex < data.Appear_Production_Time.Count)
        {
            PlayDirectionalBGFade(
                sprite,
                ref _bgFade_Ver,
                BG_Fade_Material[0],
                true,
                data.Appear_Production_Time[upIndex]);

            return;
        }

        if (downIndex >= 0 && downIndex < data.Appear_Production_Time.Count)
        {
            PlayDirectionalBGFade(
                sprite,
                ref _bgFade_Ver,
                BG_Fade_Material[0],
                false,
                data.Appear_Production_Time[downIndex]);

            return;
        }

        if (normalIndex >= 0 && normalIndex < data.Appear_Production_Time.Count)
        {
            PlayNormalBGFade(sprite, data.Appear_Production_Time[normalIndex]);

            return;
        }
        
        PlayNormalBGFade(sprite);
    }

    void PlayDirectionalBGFade(Sprite nextSprite, ref Material fadeMaterial, Material originalMaterial, bool reverse, float duration)
    {
        FadeBG.sprite = BG.sprite;
        BG.sprite = nextSprite;

        if (fadeMaterial == null)
            fadeMaterial = Instantiate(originalMaterial);

        Material mat = fadeMaterial;

        FadeBG.material = mat;
        FadeBG.color = UIUtility.Common_On_Color;

        mat.SetFloat("_FadeProgress", 0f);
        mat.SetFloat("_Feather", 0f);

        if (reverse)
            mat.EnableKeyword("REVERSE_DIRECTION");
        else
            mat.DisableKeyword("REVERSE_DIRECTION");

        _bgFadeTween = DOTween.To(
                () => mat.GetFloat("_FadeProgress"),
                x => mat.SetFloat("_FadeProgress", x),
                1f,
                duration)
            .SetEase(Ease.Linear)
            .OnComplete(ResetFadeBG);

        _bgFeatherTween = DOTween.To(
                () => mat.GetFloat("_Feather"),
                x => mat.SetFloat("_Feather", x),
                1f,
                duration)
            .SetEase(Ease.Linear);
    }

    void PlayNormalBGFade(Sprite sprite, float time = 0.5f)
    {
        FadeBG.material = null;
        FadeBG.sprite = sprite;
        FadeBG.color = UIUtility.Common_Off_Color;

        FadeBG.DOFade(1f, time)
            .OnComplete(() =>
            {
                BG.sprite = sprite;
                ResetFadeBG();
            });
    }

    void ResetFadeBG()
    {
        FadeBG.color = UIUtility.Common_Off_Color;
        FadeBG.material = null;
        FadeBG.rectTransform.localScale = Vector3.one;

        _bgFadeTween = null;
        _bgFeatherTween = null;
    }

    void KillBGTweens()
    {
        _bgFadeTween?.Kill();
        _bgFadeTween = null;

        _bgFeatherTween?.Kill();
        _bgFeatherTween = null;

        FadeBG.DOKill();
        FadeBG.rectTransform.DOKill();
    }

    void SetDots(int totalCount, int currentIdx)
    {
        if (totalCount == 1)
        {
            for (int i = 0; i < Dots.Length; i++)
            {
                Dots[i].gameObject.SetActive(false);
            }
        }
        else
        {
            for (int i = 0; i < Dots.Length; i++)
            {
                Dots[i].gameObject.SetActive(true);
            }

            if (currentIdx == 0)
            {
                Dots[0].color = UIUtility.HexToColor("b8c4e0");
                Dots[1].color = UIUtility.HexToColor("374160");
            }
            else
            {
                Dots[0].color = UIUtility.HexToColor("374160");
                Dots[1].color = UIUtility.HexToColor("b8c4e0");
            }
        }
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
        SetDots(_gallery_Datas.Count, _currentPage);
    }

    void ResetItems()
    {
        for (int i = 0; i < Context_Items.Count; i++)
        {
            Context_Items[i].gameObject.SetActive(false);
        }
        Scroll.verticalNormalizedPosition = 1f;
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
