using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Resources;
using System;
using DG.Tweening;

public class StoryManager : MonoBehaviour
{
    public static StoryManager Instance;
    public Transform Popup_Trans;
    public bool IsOpening;
    [SerializeField] TextMeshProUGUI Context;
    [SerializeField] TextMeshProUGUI Name_Text;
    [SerializeField] CanvasGroup CharacterGroup;
    [SerializeField] Image Body_Img;
    [SerializeField] Image Body_FadeImg;
    [SerializeField] Image Face_Img;
    [SerializeField] Image Face_FadeImg;
    [SerializeField] CanvasGroup NameDeco;

    [Header("BG")]
    [SerializeField] Image CurrentBG;
    [SerializeField] Image FadeBG;
    [SerializeField] Material[] BG_Fade_Material;
    Material _bgFade_Ver;
    Material _bgFade_Hor;

    [Header("Default")]
    [SerializeField] GameObject Default_Obj;

    [Header("Select")]
    [SerializeField] CanvasGroup Select_Obj;
    [SerializeField] GameObject[] Select_Buttons;
    [SerializeField] TextMeshProUGUI[] Select_Texts;

    [Header("Hide")]
    [SerializeField] GameObject[] HideTargets;

    [Header("Buttons")]
    [SerializeField] Button NextBtn;
    [SerializeField] Button HideBtn;
    [SerializeField] Button AppearBtn;
    [SerializeField] Button SaveBtn;
    [SerializeField] Button MenuBtn;
    [SerializeField] Button AutoBtn;
    [SerializeField] Button SkipBtn;
    [SerializeField] Button LogBtn;

    [SerializeField] Story_Data _currentStory;
    string _currentBody;
    string _currentFace;
    int _currentBgIndex;
    int _currentName = -1;
    bool _isHide = false;
    bool _isAuto = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        NextBtn.onClick.AddListener(OnClickNext);
        HideBtn.onClick.AddListener(OnClickHide);
        AppearBtn.onClick.AddListener(OnClickAppear);
        SaveBtn.onClick.AddListener(OnClickSave);
        MenuBtn.onClick.AddListener(OnClickMenu);
        AutoBtn.onClick.AddListener(OnClickAuto);
        SkipBtn.onClick.AddListener(OnClickSkip);
        LogBtn.onClick.AddListener(OnClickLog);
    }

    private void Start()
    {
        Context.font = Font_Manager.Instance.GetFont();
        for (int i = 0; i < Select_Texts.Length; i++)
        {
            Select_Texts[i].font = Font_Manager.Instance.GetFont();
        }
        
        ResetCharacterImmediately();
        if (Data_Manager.Instance.IsNewGame)
        {
            var target = Data_Manager.Instance.GetStoryData(1);
            if (target != null)
            {
                _currentStory = target;
            }
            Data_Manager.Instance.StartTimer(new TimeSpan());
        }
        else
        {
            var target = Data_Manager.Instance.GetStoryData(Data_Manager.Instance.SaveStory_Index);
            if (target != null)
            {
                _currentStory = target;
            }
        }
        Set_BG(true);
        SetStory();
    }

    float _currentAutoTime = 0f;
    private void Update()
    {
        if (IsActivePopup() || IsOpening)
        {
            return;
        }

        if (_isAuto && !_isHide)
        {
            if (_currentStory != null)
            {
                if (_currentStory.Next_Index != 0)
                {
                    if (!_isBusy)
                    {
                        _currentAutoTime += Time.deltaTime;
                        if (_currentAutoTime >= Data_Manager.Instance.AutoSpeed)
                        {
                            GetNextStory();
                            _currentAutoTime = 0f;
                        }
                    }
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            if (_currentStory != null && !_isAuto)
            {
                if (_currentStory.Next_Index != 0)
                {
                    if (!_isHide)
                    {
                        GetNextStory();
                    }
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.LeftControl))//스킵
        {
            OnClickSkip();
        }

        if (Input.GetKeyDown(KeyCode.A))//Auto
        {
            OnClickAuto();
        }

        if (Input.GetKeyDown(KeyCode.S))//Save
        {
            OnClickSave();
        }

        if (Input.GetKeyDown(KeyCode.L))//Load
        {
            OnClickLoad();
        }

        if (Input.GetKeyDown(KeyCode.H))//Hide
        {
            if (_isHide)
            {
                OnClickAppear();
            }
            else
            {
                OnClickHide();
            }
        }

        if (Input.GetKeyDown(KeyCode.PageUp) || Input.mouseScrollDelta.y > 0)//Log
        {
            OnClickLog();
        }

        if (Input.GetKeyDown(KeyCode.T))//Test
        {
            OpenTestTool();    
        }

        float wheel = Input.mouseScrollDelta.y;
    }

    public void LoadGame(Action endCallback)
    {
        ResetCharacterImmediately();

        var saveData = Data_Manager.Instance.Get_TempSavedata();
        var target = Data_Manager.Instance.GetStoryData(saveData.StoryIndex);

        if (target != null)
        {
            _currentStory = target;
        }

        Data_Manager.Instance.StartTimer(saveData.PlayTime);

        Set_BG(true);
        SetStory();

        endCallback?.Invoke();
    }

    void ResetCharacterImmediately()
    {
        CharacterGroup.DOKill();

        Body_Img.DOKill();
        Body_FadeImg.DOKill();
        Face_Img.DOKill();
        Face_FadeImg.DOKill();

        Body_Img.gameObject.SetActive(false);
        Body_FadeImg.gameObject.SetActive(false);
        Face_Img.gameObject.SetActive(false);
        Face_FadeImg.gameObject.SetActive(false);

        Body_Img.color = Color.white;
        Face_Img.color = Color.white;

        Body_FadeImg.color = UIUtility.Common_Off_Color;
        Face_FadeImg.color = UIUtility.Common_Off_Color;

        CharacterGroup.alpha = 0f;
        CharacterGroup.gameObject.SetActive(false);

        _currentBody = "";
        _currentFace = "";
        _currentName = -1;
    }

    void SetStory()
    {
        if (_currentStory != null)
        {
            Set_BG();
            Set_Name();
            Set_Character();
            if (_currentStory.Select_Index == 0)
            {
                Play_Typewriter();
            }
            else
            {
                SetSelect();
            }
            Data_Manager.Instance.SetSaveStory_Index(_currentStory.Index);
            CheckGallery();
        }
    }

    void GetNextStory()
    {
        ForceCompleteCharacterTween();

        if (_isBusy)
        {
            Skip();
            return;
        }

        if (_currentStory != null)
        {
            var nextIndex = _currentStory.Next_Index;
            var target = Data_Manager.Instance.GetStoryData(nextIndex);
            if (target != null)
            {
                _currentStory = target;
                SetStory();
            }
        }
    }

    void OnClickNext()
    {
        if (!_isAuto)
        {
            GetNextStory();
        }
    }
    //------------------------------------------------------------------------------------------------------------------------------------------------
    #region Default_Production
    private Coroutine _typeRoutine;
    private bool _isBusy;
    void Play_Typewriter()
    {
        Default_Obj.SetActive(true);
        Select_Obj.gameObject.SetActive(false);
        Select_Obj.alpha = 0f;
        Stop_Typewriter();
        Context.text = LanguageManager.Instance.GetText(_currentStory.Language_Key);
        Context.maxVisibleCharacters = 0;
        _typeRoutine = StartCoroutine(TypeRoutine());
    }

    void Stop_Typewriter()
    {
        if (_typeRoutine != null)
        {
            StopCoroutine(_typeRoutine);
            _typeRoutine = null;
        }
        _isBusy = false;
    }

    void Skip()
    {
        ForceCompleteCharacterTween();

        if (!_isBusy) return;

        Stop_Typewriter();

        Context.ForceMeshUpdate();
        Context.maxVisibleCharacters = Context.textInfo.characterCount;
    }

    private IEnumerator TypeRoutine()
    {
        _isBusy = true;
        Context.ForceMeshUpdate();

        int totalVisibleCharacters = Context.textInfo.characterCount;

        float waitTime = Mathf.Lerp(0.12f, 0.001f, Data_Manager.Instance.TextSpeed / 100f);

        if (Data_Manager.Instance.TextSpeed >= 100)
        {
            Context.maxVisibleCharacters = totalVisibleCharacters;
            _isBusy = false;
            yield break;
        }

        for (int i = 0; i <= totalVisibleCharacters; i++)
        {
            Context.maxVisibleCharacters = i;
            yield return new WaitForSeconds(waitTime);
        }

        _isBusy = false;
    }
    #endregion

    //------------------------------------------------------------------------------------------------------------------------------------------------
    #region Select
    List<int> _tempSelectIndex = new List<int>();
    void SetSelect()
    {
        Default_Obj.SetActive(false);

        Select_Obj.gameObject.SetActive(true);
        Select_Obj.alpha = 0f;
        Select_Obj.DOKill();
        Select_Obj.DOFade(1f, 0.5f);

        ResetSelect();
        var data = Data_Manager.Instance.GetSelectData(_currentStory.Select_Index);
        if (data != null)
        {
            _tempSelectIndex = new List<int>();
            for (int i = 0; i < data.Next_Index.Count; i++)
            {
                Select_Buttons[i].gameObject.SetActive(true);
                Select_Texts[i].text = LanguageManager.Instance.GetText(data.Language_Key[i]);
                _tempSelectIndex.Add(data.Next_Index[i]);
            }
        }
    }

    void ResetSelect()
    {
        for (int i = 0; i < Select_Buttons.Length; i++)
        {
            Select_Buttons[i].gameObject.SetActive(false);
        }
    }

    public void OnClickSelect(int idx)
    {
        ForceCompleteCharacterTween();

        var target = Data_Manager.Instance.GetStoryData(_tempSelectIndex[idx]);
        if (target != null)
        {
            _currentStory = target;
            SetStory();
        }
    }
    #endregion
    //------------------------------------------------------------------------------------------------------------------------------------------------
    #region Hide
    void OnClickHide()
    {
        _isHide = true;
        for (int i = 0; i < HideTargets.Length; i++)
        {
            HideTargets[i].gameObject.SetActive(false);
        }
        AppearBtn.gameObject.SetActive(true);
    }

    void OnClickAppear()
    {
        _isHide = false;
        for (int i = 0; i < HideTargets.Length; i++)
        {
            HideTargets[i].gameObject.SetActive(true);
        }
        AppearBtn.gameObject.SetActive(false);
        if (_currentStory.Select_Index == 0)
        {
            Select_Obj.gameObject.SetActive(false);
        }
        else
        {
            Default_Obj.SetActive(false);
        }
    }
    #endregion
    //------------------------------------------------------------------------------------------------------------------------------------------------
    #region Resource_Setting
    void Set_Name()
    {
        int name = _currentStory.Name;

        NameDeco.DOKill();
        Name_Text.DOKill();

        // 이름 없음
        if (name == 0)
        {
            _currentName = 0;

            if (NameDeco.gameObject.activeSelf)
            {
                NameDeco.DOFade(0f, 0.5f).SetEase(Ease.Linear)
                    .OnComplete(() =>
                    {
                        NameDeco.gameObject.SetActive(false);
                        NameDeco.alpha = 0f;
                    });

                Name_Text.DOFade(0f, 0.5f).SetEase(Ease.Linear)
                    .OnComplete(() =>
                    {
                        Name_Text.text = "";
                        Name_Text.alpha = 0f;
                    });
            }

            return;
        }

        string nextName = "";
        if (name == 9)
        {
            //정환 여기 수정 필요
            nextName = "닉네임";
        }
        else
        {
            nextName = LanguageManager.Instance.GetText($"Name_{name}");    
        }
        
        Color nextColor = Data_Manager.Instance.GetNameColor(name);

        // 이름창이 꺼져있음 → 페이드인
        if (!NameDeco.gameObject.activeSelf)
        {
            NameDeco.gameObject.SetActive(true);

            NameDeco.alpha = 0f;
            Name_Text.alpha = 0f;

            Name_Text.text = nextName;
            Name_Text.color = nextColor;

            NameDeco.DOFade(1f, 0.5f).SetEase(Ease.Linear);
            Name_Text.DOFade(1f, 0.5f).SetEase(Ease.Linear);

            _currentName = name;
            return;
        }

        // 같은 이름
        if (_currentName == name)
        {
            NameDeco.alpha = 1f;
            Name_Text.alpha = 1f;
            return;
        }

        // 이름만 변경
        Name_Text.text = nextName;
        Name_Text.color = nextColor;

        NameDeco.alpha = 1f;
        Name_Text.alpha = 1f;

        _currentName = name;
    }

    private bool _isCharacterChanging;

    private Sprite _pendingBodySprite;
    private Sprite _pendingFaceSprite;
    private string _pendingBodyKey;
    private string _pendingFaceKey;
    private bool _isFaceChanging;
    private bool _isFaceHiding;
    private bool _isCharacterHiding;
    private bool _isCharacterShowing;

    private Sprite _pendingFaceOnlySprite;
    private string _pendingFaceOnlyKey;

    Tween _characterDelayTween;
    Tween _faceDelayTween;
    Tween _hideFaceDelayTween;
    Tween _hideCharacterDelayTween;
    Tween _showCharacterDelayTween;

    void Set_Character()
    {
        string nextBody = _currentStory.Body;
        string nextFace = _currentStory.Face;

        if (string.IsNullOrEmpty(nextBody))
        {
            HideCharacter();
            return;
        }

        Sprite bodySprite = Resource_Manager.Instance.Get_Body_Image(nextBody);

        if (bodySprite == null)
        {
            HideCharacter();
            return;
        }

        Sprite faceSprite = null;

        if (!string.IsNullOrEmpty(nextFace))
        {
            faceSprite = Resource_Manager.Instance.Get_Face_Image(nextFace);
        }

        bool wasHidden =
            !CharacterGroup.gameObject.activeSelf ||
            CharacterGroup.alpha <= 0.01f;

        bool bodyChanged = _currentBody != nextBody;
        bool faceChanged = _currentFace != nextFace;

        if (!bodyChanged && !faceChanged)
            return;

        // 캐릭터가 없었다가 등장하는 경우
        if (wasHidden)
        {
            SetCharacterInstant(bodySprite, faceSprite, nextBody, nextFace);
            ShowCharacter();
            return;
        }

        // Body가 바뀌면 Body + Face 세트로 크로스페이드
        if (bodyChanged)
        {
            ChangeCharacterFade(bodySprite, faceSprite, nextBody, nextFace);
            return;
        }

        // Body는 그대로고 Face만 사라지는 경우
        if (string.IsNullOrEmpty(nextFace) || faceSprite == null)
        {
            HideFace();
            _currentFace = "";
            return;
        }

        // Body는 그대로고 Face만 바뀌는 경우
        ChangeFaceFade(faceSprite);
        _currentFace = nextFace;
    }

    void HideFace()
    {
        Face_Img.DOKill();
        Face_FadeImg.DOKill();
        _hideFaceDelayTween?.Kill();
        _hideFaceDelayTween = null;

        _isFaceHiding = true;

        Face_FadeImg.gameObject.SetActive(false);

        Face_Img.DOFade(0f, 0.5f);

        _hideFaceDelayTween = DOVirtual.DelayedCall(0.5f, CompleteHideFace);
    }

    void ChangeFaceFade(Sprite nextSprite)
    {
        Face_Img.DOKill();
        Face_FadeImg.DOKill();
        _faceDelayTween?.Kill();
        _faceDelayTween = null;

        _isFaceChanging = true;
        _pendingFaceOnlySprite = nextSprite;
        _pendingFaceOnlyKey = _currentStory.Face;

        Face_Img.gameObject.SetActive(true);
        Face_FadeImg.gameObject.SetActive(true);

        Face_FadeImg.sprite = nextSprite;
        Face_FadeImg.color = UIUtility.Common_Off_Color;

        Face_FadeImg.DOFade(1f, 0.5f);

        _faceDelayTween = DOVirtual.DelayedCall(0.5f, CompleteFaceChange);
    }

    void SetCharacterInstant(Sprite bodySprite, Sprite faceSprite, string bodyKey, string faceKey)
    {
        KillCharacterTweens();

        CharacterGroup.gameObject.SetActive(true);

        Body_Img.gameObject.SetActive(true);
        Body_Img.sprite = bodySprite;
        Body_Img.color = Color.white;

        if (faceSprite != null)
        {
            Face_Img.gameObject.SetActive(true);
            Face_Img.sprite = faceSprite;
            Face_Img.color = Color.white;
            _currentFace = faceKey;
        }
        else
        {
            Face_Img.gameObject.SetActive(false);
            _currentFace = "";
        }

        Body_FadeImg.gameObject.SetActive(false);
        Face_FadeImg.gameObject.SetActive(false);

        _currentBody = bodyKey;
    }

    void ChangeCharacterFade(Sprite bodySprite, Sprite faceSprite, string bodyKey, string faceKey)
    {
        KillCharacterTweens();

        _isCharacterChanging = true;

        _pendingBodySprite = bodySprite;
        _pendingFaceSprite = faceSprite;
        _pendingBodyKey = bodyKey;
        _pendingFaceKey = faceSprite != null ? faceKey : "";

        Body_Img.gameObject.SetActive(true);
        Body_FadeImg.gameObject.SetActive(true);

        Body_Img.color = Color.white;

        Body_FadeImg.sprite = bodySprite;
        Body_FadeImg.color = UIUtility.Common_Off_Color;

        Body_Img.DOFade(0f, 0.5f);
        Body_FadeImg.DOFade(1f, 0.5f);

        if (faceSprite != null)
        {
            Face_FadeImg.sprite = faceSprite;
            Face_FadeImg.color = UIUtility.Common_Off_Color;
            Face_FadeImg.gameObject.SetActive(true);

            if (!string.IsNullOrEmpty(_currentFace))
            {
                Face_Img.gameObject.SetActive(true);
                Face_Img.color = Color.white;
                Face_Img.DOFade(0f, 0.5f);
            }
            else
            {
                Face_Img.gameObject.SetActive(false);
            }

            Face_FadeImg.DOFade(1f, 0.5f);
        }
        else
        {
            Face_FadeImg.gameObject.SetActive(false);

            if (!string.IsNullOrEmpty(_currentFace))
            {
                Face_Img.DOFade(0f, 0.5f);
            }
        }

        _characterDelayTween = DOVirtual.DelayedCall(0.5f, CompleteCharacterChange);

        // KillCharacterTweens();

        // Body_Img.gameObject.SetActive(true);
        // Body_FadeImg.gameObject.SetActive(true);

        // Body_Img.color = Color.white;

        // Body_FadeImg.sprite = bodySprite;
        // Body_FadeImg.color = UIUtility.Common_Off_Color;

        // Body_Img.DOFade(0f, 0.5f);
        // Body_FadeImg.DOFade(1f, 0.5f);

        // if (faceSprite != null)
        // {
        //     Face_Img.gameObject.SetActive(true);
        //     Face_FadeImg.gameObject.SetActive(true);

        //     Face_FadeImg.sprite = faceSprite;
        //     Face_FadeImg.color = UIUtility.Common_Off_Color;

        //     // 이전 얼굴이 있었음
        //     if (!string.IsNullOrEmpty(_currentFace))
        //     {
        //         Face_Img.color = Color.white;

        //         Face_Img.DOFade(0f, 0.5f);
        //         Face_FadeImg.DOFade(1f, 0.5f);
        //     }
        //     // 이전 얼굴이 없었음
        //     else
        //     {
        //         Face_Img.gameObject.SetActive(false);

        //         Face_FadeImg.DOFade(1f, 0.5f);
        //     }
        // }
        // else
        // {
        //     Face_FadeImg.gameObject.SetActive(false);
        //     Face_Img.DOFade(0f, 0.5f);
        // }

        // _characterDelayTween = DOVirtual.DelayedCall(0.5f, () =>
        // {
        //     Body_Img.sprite = bodySprite;
        //     Body_Img.color = Color.white;
        //     Body_Img.gameObject.SetActive(true);

        //     Body_FadeImg.color = UIUtility.Common_Off_Color;
        //     Body_FadeImg.gameObject.SetActive(false);

        //     if (faceSprite != null)
        //     {
        //         Face_Img.sprite = faceSprite;
        //         Face_Img.color = Color.white;
        //         Face_Img.gameObject.SetActive(true);

        //         Face_FadeImg.color = UIUtility.Common_Off_Color;
        //         Face_FadeImg.gameObject.SetActive(false);

        //         _currentFace = faceKey;
        //     }
        //     else
        //     {
        //         Face_Img.gameObject.SetActive(false);
        //         Face_Img.color = Color.white;

        //         Face_FadeImg.color = UIUtility.Common_Off_Color;
        //         Face_FadeImg.gameObject.SetActive(false);

        //         _currentFace = "";
        //     }

        //     _currentBody = bodyKey;
        //     _characterDelayTween = null;
        // });
    }

    void KillCharacterTweens()
    {
        KillCharacterTweenOnly();

        _isCharacterShowing = false;
        _isCharacterChanging = false;
        _isFaceChanging = false;
        _isFaceHiding = false;
        _isCharacterHiding = false;
    }

    void ShowCharacter()
    {
        CharacterGroup.DOKill();

        _showCharacterDelayTween?.Kill();
        _showCharacterDelayTween = null;

        _isCharacterShowing = true;

        CharacterGroup.gameObject.SetActive(true);
        CharacterGroup.alpha = 0f;

        CharacterGroup.DOFade(1f, 0.5f).SetEase(Ease.Linear);

        _showCharacterDelayTween = DOVirtual.DelayedCall(0.5f, CompleteShowCharacter);
    }

    void CompleteShowCharacter()
    {
        if (!_isCharacterShowing)
            return;

        CharacterGroup.gameObject.SetActive(true);
        CharacterGroup.alpha = 1f;

        _showCharacterDelayTween = null;
        _isCharacterShowing = false;
    }

    void HideCharacter()
    {
        KillCharacterTweens();

        if (!CharacterGroup.gameObject.activeSelf)
        {
            _currentBody = "";
            _currentFace = "";
            return;
        }

        _isCharacterHiding = true;

        CharacterGroup.DOFade(0f, 0.5f);

        _hideCharacterDelayTween = DOVirtual.DelayedCall(0.5f, CompleteHideCharacter);
    }


    void Set_BG(bool isForce = false)
    {
        if (isForce)
        {
            CurrentBG.sprite = Resource_Manager.Instance.Get_BG(_currentStory.BG);
            FadeBG.color = UIUtility.Common_Off_Color;
            _currentBgIndex = _currentStory.BG;
            return;
        }

        if (_currentBgIndex != _currentStory.BG)
        {
            ChangeBG(_currentStory);
            _currentBgIndex = _currentStory.BG;
        }
    }

    Tween _bgFadeTween;
    Tween _bgFeatherTween;
    public void ChangeBG(Story_Data data)
    {
        var sprite = Resource_Manager.Instance.Get_BG(data.BG);

        if (sprite == null)
            return;

        KillBGTweens();

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
        FadeBG.sprite = CurrentBG.sprite;
        CurrentBG.sprite = nextSprite;

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
                CurrentBG.sprite = sprite;
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

    void CompleteCharacterChange()
    {
        if (!_isCharacterChanging)
            return;

        Body_Img.sprite = _pendingBodySprite;
        Body_Img.color = Color.white;
        Body_Img.gameObject.SetActive(true);

        Body_FadeImg.color = UIUtility.Common_Off_Color;
        Body_FadeImg.gameObject.SetActive(false);

        if (_pendingFaceSprite != null)
        {
            Face_Img.sprite = _pendingFaceSprite;
            Face_Img.color = Color.white;
            Face_Img.gameObject.SetActive(true);

            Face_FadeImg.color = UIUtility.Common_Off_Color;
            Face_FadeImg.gameObject.SetActive(false);

            _currentFace = _pendingFaceKey;
        }
        else
        {
            Face_Img.gameObject.SetActive(false);
            Face_Img.color = Color.white;

            Face_FadeImg.color = UIUtility.Common_Off_Color;
            Face_FadeImg.gameObject.SetActive(false);

            _currentFace = "";
        }

        _currentBody = _pendingBodyKey;

        _characterDelayTween = null;
        _isCharacterChanging = false;
    }

    void CompleteFaceChange()
    {
        if (!_isFaceChanging)
            return;

        Face_Img.sprite = _pendingFaceOnlySprite;
        Face_Img.color = Color.white;
        Face_Img.gameObject.SetActive(true);

        Face_FadeImg.color = UIUtility.Common_Off_Color;
        Face_FadeImg.gameObject.SetActive(false);

        _currentFace = _pendingFaceOnlyKey;

        _faceDelayTween = null;
        _isFaceChanging = false;
    }

    void CompleteHideFace()
    {
        if (!_isFaceHiding)
            return;

        Face_Img.gameObject.SetActive(false);
        Face_Img.color = Color.white;

        Face_FadeImg.color = UIUtility.Common_Off_Color;
        Face_FadeImg.gameObject.SetActive(false);

        _currentFace = "";

        _hideFaceDelayTween = null;
        _isFaceHiding = false;
    }

    void CompleteHideCharacter()
    {
        if (!_isCharacterHiding)
            return;

        CharacterGroup.gameObject.SetActive(false);
        CharacterGroup.alpha = 0f;

        Body_FadeImg.gameObject.SetActive(false);
        Face_FadeImg.gameObject.SetActive(false);

        Body_Img.color = Color.white;
        Face_Img.color = Color.white;

        _currentBody = "";
        _currentFace = "";

        _hideCharacterDelayTween = null;
        _isCharacterHiding = false;
    }

    void ForceCompleteCharacterTween()
    {
        bool hadPending =
        _isCharacterShowing ||
        _isCharacterChanging ||
        _isFaceChanging ||
        _isFaceHiding ||
        _isCharacterHiding;

        if (!hadPending)
            return;

        KillCharacterTweenOnly();

        if (_isCharacterShowing)
        {
            CompleteShowCharacter();
        }

        if (_isCharacterChanging)
        {
            CompleteCharacterChange();
        }

        if (_isFaceChanging)
        {
            CompleteFaceChange();
        }

        if (_isFaceHiding)
        {
            CompleteHideFace();
        }

        if (_isCharacterHiding)
        {
            CompleteHideCharacter();
        }
    }



    void KillCharacterTweenOnly()
    {
        _showCharacterDelayTween?.Kill();
        _showCharacterDelayTween = null;

        _characterDelayTween?.Kill();
        _characterDelayTween = null;

        _faceDelayTween?.Kill();
        _faceDelayTween = null;

        _hideFaceDelayTween?.Kill();
        _hideFaceDelayTween = null;

        _hideCharacterDelayTween?.Kill();
        _hideCharacterDelayTween = null;

        CharacterGroup.DOKill();

        Body_Img.DOKill();
        Face_Img.DOKill();
        Body_FadeImg.DOKill();
        Face_FadeImg.DOKill();
    }
    #endregion
    //------------------------------------------------------------------------------------------------------------------------------------------------
    #region Save
    Popup_Save _popup_Save;
    public void OnClickSave()
    {
        if (_popup_Save != null && _popup_Save.gameObject.activeSelf)
        {
            return;
        }

        if (_popup_Save == null)
        {
            var target = Resources.Load<GameObject>("Popup/Popup_Save");
            if (target != null)
            {
                var item = Instantiate(target, Popup_Trans);
                _popup_Save = item.GetComponent<Popup_Save>();
            }
        }
        _popup_Save.SetPopup(1, 0);
        _popup_Save.Open();
    }


    public void OnClickLoad()
    {
        if (_popup_Save != null && _popup_Save.gameObject.activeSelf)
        {
            return;
        }

        if (_popup_Save == null)
        {
            var target = Resources.Load<GameObject>("Popup/Popup_Save");
            if (target != null)
            {
                var item = Instantiate(target, Popup_Trans);
                _popup_Save = item.GetComponent<Popup_Save>();
            }
        }
        _popup_Save.SetPopup(1, 1);
        _popup_Save.Open();
    }
    #endregion
    //------------------------------------------------------------------------------------------------------------------------------------------------
    #region Menu
    Popup_Menu _popup_Menu;
    public void OnClickMenu()
    {
        if (_popup_Menu == null)
        {
            var target = Resources.Load<GameObject>("Popup/Popup_Menu");
            if (target != null)
            {
                var item = Instantiate(target, Popup_Trans);
                _popup_Menu = item.GetComponent<Popup_Menu>();
            }
        }
        _popup_Menu.Open();
    }
    #endregion
    //------------------------------------------------------------------------------------------------------------------------------------------------
    #region Auto
    void OnClickAuto()
    {
        if (_isAuto)
        {
            AutoBtn.image.color = new Color(0.8823529f, 0.9215686f, 0.9803922f, 0.8f);
        }
        else
        {
            _currentAutoTime = 0f;
            AutoBtn.image.color = new Color(0.5607843f, 0.7686275f, 1f, 1f);
        }
        _isAuto = !_isAuto;
    }
    #endregion
    //------------------------------------------------------------------------------------------------------------------------------------------------
    #region Skip
    void OnClickSkip()
    {
        if (_currentStory.Select_Index == 0)
        {
            var target = Data_Manager.Instance.GetNextSelect(_currentStory.Index);
            if (target != null)
            {
                var popup = Resource_Manager.Instance.Get_Yes_Or_No();
                popup.Open();
                popup.SetPopup(LanguageManager.Instance.GetText("Skip_Warning_1"), () =>
                {
                    ForceCompleteCharacterTween();

                    _currentStory = target;
                    SetStory();
                    _currentAutoTime = 0f;
                    popup.Close();
                },
                () =>
                {
                    popup.Close();
                });
            }
            else
            {
                var popup = Resource_Manager.Instance.Get_Yes_Or_No();
                popup.Open();
                popup.SetPopup_One(LanguageManager.Instance.GetText("Skip_Warning_2"), () =>
                {
                    popup.Close();
                });
            }
        }
    }
    #endregion
    //------------------------------------------------------------------------------------------------------------------------------------------------
    #region Log
    Popup_Log _popup_Log;
    public void OnClickLog()
    {
        if (_popup_Log == null)
        {
            var target = Resources.Load<GameObject>("Popup/Popup_Log");
            if (target != null)
            {
                var item = Instantiate(target, Popup_Trans);
                _popup_Log = item.GetComponent<Popup_Log>();
            }
        }
        _popup_Log.Open();

        if (_currentStory.Select_Index != 0)//현재 선택지
        {
            var temp = Data_Manager.Instance.GetBeforeStory(_currentStory.Index);
            if (temp != null)
            {
                _popup_Log.SetItems(temp.Index);
            }
            else
            {
                _popup_Log.SetItems(_currentStory.Index);
            }
        }
        else
        {
            _popup_Log.SetItems(_currentStory.Index);
        }
    }
    #endregion
    //------------------------------------------------------------------------------------------------------------------------------------------------
    #region Gallery
    void CheckGallery()
    {
        if (_currentStory.Gallery != 0)
        {
            Data_Manager.Instance.AddGallery(_currentStory.Gallery);
        }
    }
    #endregion
    //------------------------------------------------------------------------------------------------------------------------------------------------

    #region Utility
    public bool IsActivePopup()
    {
        if (_popup_Menu != null && _popup_Menu.gameObject.activeSelf)
        {
            return true;
        }

        if (_popup_Save != null && _popup_Save.gameObject.activeSelf)
        {
            return true;
        }

        if (_popup_Log != null && _popup_Log.gameObject.activeSelf)
        {
            return true;
        }

        if (Resource_Manager.Instance.Get_Yes_Or_No().gameObject.activeSelf)
        {
            return true;
        }

        return false;
    }

    public void SetTest(Story_Data data)
    {
        _currentStory = data;
        SetStory();
    }

    TestTool _testTool;
    void OpenTestTool()
    {
        if (_testTool == null)
        {
            var target = Resources.Load<GameObject>("Popup/Popup_Test");
            if (target != null)
            {
                var item = Instantiate(target, Popup_Trans);
                _testTool = item.GetComponent<TestTool>();
            }
        }
        _testTool.Open();
    }
    #endregion
}
