using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Resources;
using System;
using DG.Tweening;
using God.Audio;

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
    [SerializeField] Image BG_Dim;
    [SerializeField] Material[] BG_Fade_Material;
    [SerializeField] Image Blink;
    [SerializeField] Material BlinkMaterial;
    [SerializeField] Material BlurMaterial;
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
    bool _isAutoNext = false;

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

        _bgOriginPos = CurrentBG.rectTransform.anchoredPosition;
        _textOriginPos = Context.rectTransform.anchoredPosition;
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
        if (IsActivePopup() || IsOpening || IsSetName || _isAutoNext)
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
        CurrentBG.material = null;
        CurrentBG.rectTransform.localScale = Vector3.one;
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
            _isAutoNext = _currentStory.Auto_Next;
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

            if (_currentStory.Index == 845)
            {
                if (string.IsNullOrEmpty(Data_Manager.Instance.MyName))
                {
                    IsSetName = true;    
                }
            }
            CheckBGM();
            Check_CG_Effect();
            Check_SFX();
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

        if (IsSetName)
        {
            OpenNamePopup();
        }
        else
        {
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
    }

    void OnClickNext()
    {
        if (!_isAuto && !_isAutoNext)
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
        if (_currentStory.My_Name)
        {
            if (LanguageManager.Instance.GetCurrentLanguage() == LanguageType.KR)
            {
                if (!Data_Manager.Instance.HasFinalConsonant(Data_Manager.Instance.MyName))
                {
                    var str = LanguageManager.Instance.GetText($"{_currentStory.Language_Key}_1");
                    if (str == $"{_currentStory.Language_Key}_1")
                    {
                        str = LanguageManager.Instance.GetText(_currentStory.Language_Key);
                        Context.text = string.Format(str, Data_Manager.Instance.MyName);
                    }
                    else
                    {
                        Context.text = string.Format(str, Data_Manager.Instance.MyName);
                    }
                }
                else
                {
                    var str = LanguageManager.Instance.GetText(_currentStory.Language_Key);
                    Context.text = string.Format(str, Data_Manager.Instance.MyName);
                }
            }
            else
            {
                var str = LanguageManager.Instance.GetText(_currentStory.Language_Key);
                Context.text = string.Format(str, Data_Manager.Instance.MyName);
            }
        }
        else
        {
            Context.text = LanguageManager.Instance.GetText(_currentStory.Language_Key);    
        }
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
            Data_Manager.Instance.AddSelect(_currentStory.Select_Index - 1, idx);
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
            nextName = Data_Manager.Instance.MyName;
        }
        else if (name == 6)
        {
            var str = LanguageManager.Instance.GetText($"Name_{name}");
            nextName = string.Format(str, Data_Manager.Instance.MyName);
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

        if (_currentStory.Index == 3036)
        {
            CharacterGroup.alpha = 1f;
            CompleteShowCharacter();
        }
        else
        {
            CharacterGroup.alpha = 0f;
            CharacterGroup.DOFade(1f, 0.5f).SetEase(Ease.Linear);
            _showCharacterDelayTween = DOVirtual.DelayedCall(0.5f, CompleteShowCharacter);
        }
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

        CheckProduction();
    }

    
    void CheckProduction()
    {
        int eyeCloseIndex = _currentStory.Appear_Production.IndexOf(1);
        int eyeOpenIndex = _currentStory.Appear_Production.IndexOf(2);
        int blinkIndex = _currentStory.Appear_Production.IndexOf(9);
        int blurIndex = _currentStory.Appear_Production.IndexOf(10);
        int blurOnceIndex = _currentStory.Appear_Production.IndexOf(11);

        int bgLoopShakeXIndex = _currentStory.Appear_Production.IndexOf(12);
        int bgLoopShakeYIndex = _currentStory.Appear_Production.IndexOf(13);
        int bgLoopShakeXYIndex = _currentStory.Appear_Production.IndexOf(14);

        int bgOnceShakeXIndex = _currentStory.Appear_Production.IndexOf(15);
        int bgOnceShakeYIndex = _currentStory.Appear_Production.IndexOf(16);
        int bgOnceShakeXYIndex = _currentStory.Appear_Production.IndexOf(17);

        int textLoopShakeXIndex = _currentStory.Appear_Production.IndexOf(18);
        int textLoopShakeYIndex = _currentStory.Appear_Production.IndexOf(19);
        int textLoopShakeXYIndex = _currentStory.Appear_Production.IndexOf(20);

        int textOnceShakeXIndex = _currentStory.Appear_Production.IndexOf(21);
        int textOnceShakeYIndex = _currentStory.Appear_Production.IndexOf(22);
        int textOnceShakeXYIndex = _currentStory.Appear_Production.IndexOf(23);

        int bg_FadeIn_Index = _currentStory.Appear_Production.IndexOf(24);
        int bg_FadeOut_Index = _currentStory.Appear_Production.IndexOf(25);
        int bg_WhiteIn_Index = _currentStory.Appear_Production.IndexOf(26);
        int bg_WhiteOut_Index = _currentStory.Appear_Production.IndexOf(27);

        bool hasBlinkProduction = blinkIndex >= 0 || eyeCloseIndex >= 0 || eyeOpenIndex >= 0;
        bool hasBlurProduction = blurIndex >= 0 || blurOnceIndex >= 0;
        bool hasBGShake = bgLoopShakeXIndex >= 0 || bgLoopShakeYIndex >= 0 || bgLoopShakeXYIndex >= 0 || bgOnceShakeXIndex >= 0 || bgOnceShakeYIndex >= 0 || bgOnceShakeXYIndex >= 0;
        bool hasTextShake = textLoopShakeXIndex >= 0 || textLoopShakeYIndex >= 0 || textLoopShakeXYIndex >= 0 || textOnceShakeXIndex >= 0 || textOnceShakeYIndex >= 0 || textOnceShakeXYIndex >= 0;
        bool hasBGFade = bg_FadeIn_Index >= 0 || bg_FadeOut_Index >= 0 || bg_WhiteIn_Index >= 0 || bg_WhiteOut_Index >= 0;

        if (blinkIndex >= 0 && blinkIndex < _currentStory.Appear_Production_Time.Count)
        {
            float blinkProgress = _currentStory.Appear_Production_Value[blinkIndex];
            float blinkTime = _currentStory.Appear_Production_Time[blinkIndex];
            
            if (!Blink.gameObject.activeSelf)
            {
                Blink.gameObject.SetActive(true);
                Blink.DOFade(1f, 0.2f).SetEase(Ease.Linear);
            }
            PlayBlink(blinkTime, 0f, blinkTime, blinkProgress);
        }

        if (eyeCloseIndex >= 0 && eyeCloseIndex < _currentStory.Appear_Production_Time.Count)
        {
            float time = _currentStory.Appear_Production_Time[eyeCloseIndex];
            float blinkProgress = _currentStory.Appear_Production_Value[eyeCloseIndex];
            if (!Blink.gameObject.activeSelf)
            {
                Blink.gameObject.SetActive(true);
                Blink.DOFade(1f, 0.2f).SetEase(Ease.Linear);
            }
            CloseEye(time, blinkProgress);
        }

        if (eyeOpenIndex >= 0 && eyeOpenIndex < _currentStory.Appear_Production_Time.Count)
        {
            float time = _currentStory.Appear_Production_Time[eyeOpenIndex];
            float blinkProgress = _currentStory.Appear_Production_Value[eyeOpenIndex];
            
            if (!Blink.gameObject.activeSelf)
            {
                Blink.gameObject.SetActive(true);
                Blink.DOFade(1f, 0.2f).SetEase(Ease.Linear);
            }
            OpenEye(time, blinkProgress);
        }

        if (blurIndex >= 0 && blurIndex < _currentStory.Appear_Production_Time.Count)
        {
            float time = _currentStory.Appear_Production_Time[blurIndex];
            float blinkProgress = _currentStory.Appear_Production_Value[blurIndex];
            
            if (CurrentBG.material == null)
            {
                if (_runtimeBlurMaterial == null)
                {
                    _runtimeBlurMaterial = Instantiate(BlurMaterial);
                }

                CurrentBG.material = _runtimeBlurMaterial;
                _runtimeBlurMaterial.SetFloat("_BlurSize", 0f);
            }
            BlinkBlur(time, blinkProgress);
        }

        if (blurOnceIndex >= 0 && blurOnceIndex < _currentStory.Appear_Production_Time.Count)
        {
            float time = _currentStory.Appear_Production_Time[blurOnceIndex];
            float blinkProgress = _currentStory.Appear_Production_Value[blurOnceIndex];
            
            if (CurrentBG.material == null)
            {
                if (_runtimeBlurMaterial == null)
                {
                    _runtimeBlurMaterial = Instantiate(BlurMaterial);
                }

                CurrentBG.material = _runtimeBlurMaterial;
                _runtimeBlurMaterial.SetFloat("_BlurSize", 0f);
            }
            BlinkBlurOnce(time, blinkProgress);
        }

        if (bgLoopShakeXIndex >= 0 && bgLoopShakeXIndex < _currentStory.Appear_Production_Time.Count)
        {
            float value = _currentStory.Appear_Production_Value[bgLoopShakeXIndex];

            ShakeBGLoop(20f, value, Vector2.right);
        }
        else if (bgLoopShakeYIndex >= 0 && bgLoopShakeYIndex < _currentStory.Appear_Production_Time.Count)
        {
            float value = _currentStory.Appear_Production_Value[bgLoopShakeYIndex];

            ShakeBGLoop(20f, value, Vector2.up);
        }
        else if (bgLoopShakeXYIndex >= 0 && bgLoopShakeXYIndex < _currentStory.Appear_Production_Time.Count)
        {
            float value = _currentStory.Appear_Production_Value[bgLoopShakeXYIndex];

            ShakeBGLoop(20f, value, Vector2.one);
        }
        else if (bgOnceShakeXIndex >= 0 && bgOnceShakeXIndex < _currentStory.Appear_Production_Time.Count)
        {
            float time = _currentStory.Appear_Production_Time[bgOnceShakeXIndex];
            float value = _currentStory.Appear_Production_Value[bgOnceShakeXIndex];

            ShakeBGOnce(time, value, Vector2.right);
        }
        else if (bgOnceShakeYIndex >= 0 && bgOnceShakeYIndex < _currentStory.Appear_Production_Time.Count)
        {
            float time = _currentStory.Appear_Production_Time[bgOnceShakeYIndex];
            float value = _currentStory.Appear_Production_Value[bgOnceShakeYIndex];

            ShakeBGOnce(time, value, Vector2.up);
        }
        else if (bgOnceShakeXYIndex >= 0 && bgOnceShakeXYIndex < _currentStory.Appear_Production_Time.Count)
        {
            float time = _currentStory.Appear_Production_Time[bgOnceShakeXYIndex];
            float value = _currentStory.Appear_Production_Value[bgOnceShakeXYIndex];

            ShakeBGOnce(time, value, Vector2.one);
        }
        if (textLoopShakeXIndex >= 0 && textLoopShakeXIndex < _currentStory.Appear_Production_Time.Count)
        {
            float value = _currentStory.Appear_Production_Value[textLoopShakeXIndex];

            ShakeTextLoop(20f, value, Vector2.right);
        }
        else if (textLoopShakeYIndex >= 0 && textLoopShakeYIndex < _currentStory.Appear_Production_Time.Count)
        {
            float value = _currentStory.Appear_Production_Value[textLoopShakeYIndex];

            ShakeTextLoop(20f, value, Vector2.up);
        }
        else if (textLoopShakeXYIndex >= 0 && textLoopShakeXYIndex < _currentStory.Appear_Production_Time.Count)
        {
            float value = _currentStory.Appear_Production_Value[textLoopShakeXYIndex];

            ShakeTextLoop(20f, value, Vector2.one);
        }
        else if (textOnceShakeXIndex >= 0 && textOnceShakeXIndex < _currentStory.Appear_Production_Time.Count)
        {
            float time = _currentStory.Appear_Production_Time[textOnceShakeXIndex];
            float value = _currentStory.Appear_Production_Value[textOnceShakeXIndex];

            ShakeTextOnce(time, value, Vector2.right);
        }
        else if (textOnceShakeYIndex >= 0 && textOnceShakeYIndex < _currentStory.Appear_Production_Time.Count)
        {
            float time = _currentStory.Appear_Production_Time[textOnceShakeYIndex];
            float value = _currentStory.Appear_Production_Value[textOnceShakeYIndex];

            ShakeTextOnce(time, value, Vector2.up);
        }
        else if (textOnceShakeXYIndex >= 0 && textOnceShakeXYIndex < _currentStory.Appear_Production_Time.Count)
        {
            float time = _currentStory.Appear_Production_Time[textOnceShakeXYIndex];
            float value = _currentStory.Appear_Production_Value[textOnceShakeXYIndex];

            ShakeTextOnce(time, value, Vector2.one);
        }
        else if (bg_FadeIn_Index >= 0 && bg_FadeIn_Index < _currentStory.Appear_Production_Time.Count)
        {
            float time = _currentStory.Appear_Production_Time[bg_FadeIn_Index];
            float value = _currentStory.Appear_Production_Value[bg_FadeIn_Index];

            FadeIn(new Color(0f, 0f, 0f, BG_Dim.color.a), time, value);
        }
        else if (bg_FadeOut_Index >= 0 && bg_FadeOut_Index < _currentStory.Appear_Production_Time.Count)
        {
            float time = _currentStory.Appear_Production_Time[bg_FadeOut_Index];
            float value = _currentStory.Appear_Production_Value[bg_FadeOut_Index];
            FadeOut(new Color(0f, 0f, 0f, BG_Dim.color.a), time, value);
        }
        else if (bg_WhiteIn_Index >= 0 && bg_WhiteIn_Index < _currentStory.Appear_Production_Time.Count)
        {
            float time = _currentStory.Appear_Production_Time[bg_WhiteIn_Index];
            float value = _currentStory.Appear_Production_Value[bg_WhiteIn_Index];
            FadeIn(new Color(1f, 1f, 1f, BG_Dim.color.a), time, value);
        }
        else if (bg_WhiteOut_Index >= 0 && bg_WhiteOut_Index < _currentStory.Appear_Production_Time.Count)
        {
            float time = _currentStory.Appear_Production_Time[bg_WhiteOut_Index];
            float value = _currentStory.Appear_Production_Value[bg_WhiteOut_Index];
            FadeOut(new Color(1f, 1f, 1f, BG_Dim.color.a), time, value);
        }
        //------------------------------초기화 부분--------------------------------
        if (Blink.gameObject.activeSelf && !hasBlinkProduction)
        {
            Blink.DOKill();
            Blink.DOFade(0f, 0.2f).SetEase(Ease.Linear).OnComplete(() =>
                {
                    Blink.gameObject.SetActive(false);
                });
        }

        if (CurrentBG.material != null && !hasBlurProduction)
        {
            ResetBlur();
        }

        if (!hasBGShake)
        {
            ResetBGShake();
        }

        if (!hasTextShake)
        {
            ResetTextShake();
        }

        if (!hasBGFade)
        {
            ResetFade();
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
    #region Blink
    Tween _blinkTween;
    Material _runtimeBlinkMaterial;
    private static readonly int BlinkProgressID = Shader.PropertyToID("_BlinkProgress");
    private static readonly int FeatherID = Shader.PropertyToID("_Feather");
    private static readonly int OvalPowerID = Shader.PropertyToID("_OvalPower");
    float feather = 0.2f;
    float ovalPower = 1f;

    void KillBlinkProduction()
    {
        _blinkTween?.Kill();
        _blinkTween = null;
    }

    public void PlayBlink(float closeTime, float holdTime, float openTime, float progress)
    {
        KillBlinkProduction();

        if (_runtimeBlinkMaterial == null)
        {
            _runtimeBlinkMaterial = Instantiate(BlinkMaterial);
            Blink.material = _runtimeBlinkMaterial;
        }
        _runtimeBlinkMaterial.SetFloat(FeatherID, feather);
        _runtimeBlinkMaterial.SetFloat(OvalPowerID, ovalPower);
        if (_runtimeBlinkMaterial.GetFloat(BlinkProgressID) != 1f)
        {
            _runtimeBlinkMaterial.SetFloat(BlinkProgressID, 0f);    
        }

        Sequence seq = DOTween.Sequence();

        seq.Append(DOTween.To(
                () => _runtimeBlinkMaterial.GetFloat(BlinkProgressID),
                x => _runtimeBlinkMaterial.SetFloat(BlinkProgressID, x),
                progress,
                closeTime)
            .SetEase(Ease.InOutSine));

        seq.AppendInterval(holdTime);

        seq.Append(DOTween.To(
                () => _runtimeBlinkMaterial.GetFloat(BlinkProgressID),
                x => _runtimeBlinkMaterial.SetFloat(BlinkProgressID, x),
                0f,
                openTime)
            .SetEase(Ease.InOutSine));

        seq.OnComplete(SetOpenImmediate);

        _blinkTween = seq;
    }

    void SetOpenImmediate()
    {
        KillBlinkProduction();

        if (_runtimeBlinkMaterial != null)
            _runtimeBlinkMaterial.SetFloat(BlinkProgressID, 0f);

        //Blink.gameObject.SetActive(false);
    }

    void CloseEye(float duration, float value)
    {
        KillBlinkProduction();

        if (_runtimeBlinkMaterial == null)
        {
            _runtimeBlinkMaterial = Instantiate(BlinkMaterial);
            Blink.material = _runtimeBlinkMaterial;
        }

        _runtimeBlinkMaterial.SetFloat(FeatherID, feather);
        if (value == 0)
        {
            _runtimeBlinkMaterial.SetFloat(OvalPowerID, ovalPower);

            _blinkTween = DOTween.To(
                () => _runtimeBlinkMaterial.GetFloat(BlinkProgressID),
                x => _runtimeBlinkMaterial.SetFloat(BlinkProgressID, x),
                1f,
                duration)
            .SetEase(Ease.InOutSine).OnComplete(CheckAutoNext);
        }
        else
        {
            _blinkTween = DOTween.To(
                () => _runtimeBlinkMaterial.GetFloat(BlinkProgressID),
                x => _runtimeBlinkMaterial.SetFloat(BlinkProgressID, x),
                value,
                duration)
            .SetEase(Ease.InOutSine).OnComplete(CheckAutoNext);
        }
    }

    void OpenEye(float duration, float value)
    {
        if (_runtimeBlinkMaterial == null)
        {
            _runtimeBlinkMaterial = Instantiate(BlinkMaterial);
            Blink.material = _runtimeBlinkMaterial;
        }

        KillBlinkProduction();

        _blinkTween = DOTween.To(
                () => _runtimeBlinkMaterial.GetFloat(BlinkProgressID),
                x => _runtimeBlinkMaterial.SetFloat(BlinkProgressID, x),
                0f,
                duration)
            .SetEase(Ease.InOutSine)
            .OnComplete(() =>
            {
                SetOpenImmediate();
                CheckAutoNext();
            });
    }

    void CheckAutoNext()
    {
        if (_isAutoNext)
        {
            if (_currentStory.Index == 800)
            {
                Invoke("GetNextStory", 1f);
            }
            else
            {
                GetNextStory();    
            }
        }   
    }
    #endregion
    //------------------------------------------------------------------------------------------------------------------------------------------------
    #region Blur
    Material _runtimeBlurMaterial;
    Tween _blurTween;
    float _currentBlurTime = -1f;
    float _currentBlurValue = -1f;

    void BlinkBlur(float totalTime, float value)
    {
        if (_runtimeBlurMaterial == null)
        {
            _runtimeBlurMaterial = Instantiate(BlurMaterial);
        }

        CurrentBG.material = _runtimeBlurMaterial;

        // 이미 같은 설정으로 블러가 돌고 있으면 다시 만들지 않음
        if (_blurTween != null && Mathf.Approximately(_currentBlurTime, totalTime) && Mathf.Approximately(_currentBlurValue, value))
        {
            return;
        }

        float currentBlur = _runtimeBlurMaterial.GetFloat(BlurSizeID);

        KillBlurTween();

        _currentBlurTime = totalTime;
        _currentBlurValue = value;

        float halfTime = totalTime * 0.5f;

        Sequence seq = DOTween.Sequence();

        // 현재 BlurSize에서 새 목표 value까지 자연스럽게 이동
        float firstDuration = halfTime;

        if (value > 0.001f)
        {
            float distanceRate = Mathf.Abs(value - currentBlur) / value;
            firstDuration = Mathf.Max(0.01f, halfTime * distanceRate);
        }

        seq.Append(DOTween.To(
                () => _runtimeBlurMaterial.GetFloat(BlurSizeID),
                x => _runtimeBlurMaterial.SetFloat(BlurSizeID, x),
                value,
                firstDuration)
            .SetEase(Ease.Linear));

        seq.Append(DOTween.To(
                () => _runtimeBlurMaterial.GetFloat(BlurSizeID),
                x => _runtimeBlurMaterial.SetFloat(BlurSizeID, x),
                0f,
                halfTime)
            .SetEase(Ease.Linear));

        seq.Append(DOTween.To(
                () => _runtimeBlurMaterial.GetFloat(BlurSizeID),
                x => _runtimeBlurMaterial.SetFloat(BlurSizeID, x),
                value,
                halfTime)
            .SetEase(Ease.Linear));

        seq.Append(DOTween.To(
                () => _runtimeBlurMaterial.GetFloat(BlurSizeID),
                x => _runtimeBlurMaterial.SetFloat(BlurSizeID, x),
                0f,
                halfTime)
            .SetEase(Ease.Linear));

        seq.SetLoops(-1, LoopType.Yoyo);

        _blurTween = seq;
    }

    void BlinkBlurOnce(float totalTime, float value)
    {
        KillBlurTween();

        if (_runtimeBlurMaterial == null)
        {
            _runtimeBlurMaterial = Instantiate(BlurMaterial);
        }

        CurrentBG.material = _runtimeBlurMaterial;

        _runtimeBlurMaterial.SetFloat(BlurSizeID, 0f);

        float halfTime = totalTime * 0.5f;

        Sequence seq = DOTween.Sequence();

        seq.Append(DOTween.To(
                () => _runtimeBlurMaterial.GetFloat(BlurSizeID),
                x => _runtimeBlurMaterial.SetFloat(BlurSizeID, x),
                value,
                halfTime)
            .SetEase(Ease.Linear));

        seq.Append(DOTween.To(
                () => _runtimeBlurMaterial.GetFloat(BlurSizeID),
                x => _runtimeBlurMaterial.SetFloat(BlurSizeID, x),
                0f,
                halfTime)
            .SetEase(Ease.Linear));

        seq.OnComplete(() =>
        {
            _blurTween = null;

            _runtimeBlurMaterial.SetFloat(BlurSizeID, 0f);
            CurrentBG.material = null;
        });

        _blurTween = seq;
    }

    void KillBlurTween()
    {
        _blurTween?.Kill();
        _blurTween = null;
    }

    void ResetBlur()
    {
        if (CurrentBG.material == null)
        {
            return;
        }

        _currentBlurTime = -1f;
        _currentBlurValue = -1f;

        if (_runtimeBlurMaterial != null)
            _runtimeBlurMaterial.SetFloat(BlurSizeID, 0f);

        CurrentBG.material = null;
    }
    #endregion
    //------------------------------------------------------------------------------------------------------------------------------------------------
    #region Shake
    Tween _bgShakeTween;
    Vector2 _bgOriginPos;
    float _bgShakeTimer;
    float _currentBGShakeSpeed = -1f;
    float _currentBGShakeStrength = -1f;
    Vector2 _currentBGShakeDirection = Vector2.zero;
    bool _isBGShaking;

    Tween _textShakeTween;
    Vector2 _textOriginPos;
    float _textShakeTimer;
    float _currentTextShakeSpeed = -1f;
    float _currentTextShakeStrength = -1f;
    Vector2 _currentTextShakeDirection = Vector2.zero;
    bool _isTextShaking;

    Tween _bgScaleTween;
    void ResetBGShake()
    {
        if (_bgShakeTween == null) return;

        _bgShakeTween?.Kill();
        _bgShakeTween = null;

        CurrentBG.rectTransform.anchoredPosition = _bgOriginPos;

        ResetCurrentBGScale();

        _isBGShaking = false;
        _currentBGShakeSpeed = -1f;
        _currentBGShakeStrength = -1f;
        _currentBGShakeDirection = Vector2.zero;
        _bgShakeTimer = 0f;
    }

    void ResetTextShake()
    {
        if (_textShakeTween == null) return;

        _textShakeTween?.Kill();
        _textShakeTween = null;

        Context.rectTransform.anchoredPosition = _textOriginPos;

        _isTextShaking = false;
        _currentTextShakeSpeed = -1f;
        _currentTextShakeStrength = -1f;
        _currentTextShakeDirection = Vector2.zero;
        _textShakeTimer = 0f;
    }

    void ResetCurrentBGScale()
    {
        _bgScaleTween?.Kill();
        _bgScaleTween = CurrentBG.rectTransform
            .DOScale(Vector3.one, 0.2f)
            .SetEase(Ease.OutQuad);
    }

    void SetCurrentBGShakeScale()
    {
        Vector3 targetScale = new Vector3(1.03f, 1.03f, 1.03f);

        _bgScaleTween?.Kill();
        _bgScaleTween = CurrentBG.rectTransform
            .DOScale(targetScale, 0.15f)
            .SetEase(Ease.OutQuad);
    }

    void ShakeBGLoop(float speed, float strength, Vector2 direction)
    {
        RectTransform rect = CurrentBG.rectTransform;

        if (_isBGShaking &&
            Mathf.Approximately(_currentBGShakeSpeed, speed) &&
            Mathf.Approximately(_currentBGShakeStrength, strength) &&
            _currentBGShakeDirection == direction)
        {
            return;
        }

        _bgShakeTween?.Kill();
        _bgShakeTween = null;

        _isBGShaking = true;
        _currentBGShakeSpeed = speed;
        _currentBGShakeStrength = strength;
        _currentBGShakeDirection = direction;
        SetCurrentBGShakeScale();

        _bgShakeTween = DOVirtual.Float(0f, 1f, 1f, value =>
            {
                _bgShakeTimer += Time.deltaTime;

                float noiseX = Mathf.PerlinNoise(_bgShakeTimer * speed, 0f) * 2f - 1f;
                float noiseY = Mathf.PerlinNoise(0f, _bgShakeTimer * speed) * 2f - 1f;

                Vector2 offset = Vector2.zero;

                if (direction.x != 0)
                    offset.x = noiseX * strength;

                if (direction.y != 0)
                    offset.y = noiseY * strength;

                rect.anchoredPosition = _bgOriginPos + offset;
            })
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Restart);
    }

    void ShakeBGOnce(float duration, float strength, Vector2 direction)
    {
        _bgShakeTween?.Kill();
        _bgShakeTween = null;

        RectTransform rect = CurrentBG.rectTransform;
        rect.anchoredPosition = _bgOriginPos;
        SetCurrentBGShakeScale();

        Vector3 shakeStrength = new Vector3(
            strength * direction.x,
            strength * direction.y,
            0f
        );

        _bgShakeTween = rect.DOShakeAnchorPos(
                duration,
                shakeStrength,
                20,
                90f,
                false,
                true)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                rect.anchoredPosition = _bgOriginPos;
                ResetCurrentBGScale();
                _bgShakeTween = null;
            });
    }

    void ShakeTextLoop(float speed, float strength, Vector2 direction)
    {
        RectTransform rect = Context.rectTransform;

        if (_isTextShaking &&
            Mathf.Approximately(_currentTextShakeSpeed, speed) &&
            Mathf.Approximately(_currentTextShakeStrength, strength) &&
            _currentTextShakeDirection == direction)
        {
            return;
        }

        _textShakeTween?.Kill();
        _textShakeTween = null;

        _isTextShaking = true;
        _currentTextShakeSpeed = speed;
        _currentTextShakeStrength = strength;
        _currentTextShakeDirection = direction;

        _textShakeTween = DOVirtual.Float(0f, 1f, 1f, value =>
            {
                _textShakeTimer += Time.deltaTime;

                float noiseX = Mathf.PerlinNoise(_textShakeTimer * speed, 10f) * 2f - 1f;
                float noiseY = Mathf.PerlinNoise(10f, _textShakeTimer * speed) * 2f - 1f;

                Vector2 offset = Vector2.zero;

                if (direction.x != 0)
                    offset.x = noiseX * strength;

                if (direction.y != 0)
                    offset.y = noiseY * strength;

                rect.anchoredPosition = _textOriginPos + offset;
            })
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Restart);
    }

    void ShakeTextOnce(float duration, float strength, Vector2 direction)
    {
        _textShakeTween?.Kill();
        _textShakeTween = null;

        _isTextShaking = false;

        RectTransform rect = Context.rectTransform;
        rect.anchoredPosition = _textOriginPos;

        Vector3 shakeStrength = new Vector3(
            strength * direction.x,
            strength * direction.y,
            0f
        );

        _textShakeTween = rect.DOShakeAnchorPos(
                duration,
                shakeStrength,
                20,
                90f,
                false,
                true)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                rect.anchoredPosition = _textOriginPos;
                _textShakeTween = null;
            });
    }
    #endregion
    //------------------------------------------------------------------------------------------------------------------------------------------------
    #region Fade
    Tween _fadeTween;
    void FadeIn(Color _color, float duration, float value)
    {
        _fadeTween?.Kill();
        _fadeTween = null;

        BG_Dim.color = _color;
        BG_Dim.gameObject.SetActive(true);

        _fadeTween = BG_Dim
            .DOFade(value, duration)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                _fadeTween = null;
            });
    }

    void FadeOut(Color _color, float duration, float value)
    {
        _fadeTween?.Kill();
        _fadeTween = null;

        BG_Dim.color = _color;
        BG_Dim.gameObject.SetActive(true);

        _fadeTween = BG_Dim
            .DOFade(value, duration)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                _fadeTween = null;

                if (value <= 0.001f)
                {
                    Color color = BG_Dim.color;
                    color.a = 0f;
                    BG_Dim.color = color;
                    
                    BG_Dim.gameObject.SetActive(false);
                }
            });
    }

    void ResetFade()
    {
        _fadeTween?.Kill();
        _fadeTween = null;

        Color color = BG_Dim.color;
        color.a = 0f;
        BG_Dim.color = color;
        
        BG_Dim.gameObject.SetActive(false);
    }
    #endregion
    //------------------------------------------------------------------------------------------------------------------------------------------------
    #region Save
    Popup_Save _popup_Save;
    static readonly int BlurSizeID = Shader.PropertyToID("_BlurSize");
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
        if (_currentStory.Select_Index == 0 && !IsSetName)
        {
            var target = Data_Manager.Instance.GetNextSelect(_currentStory.Index);
            if (target != null)
            {
                if (_currentStory.Index < 845 && target.Index > 845)
                {
                    if (string.IsNullOrEmpty(Data_Manager.Instance.MyName))
                    {
                        target = Data_Manager.Instance.GetStoryData(845);    
                    }
                }
                
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
    #region Name
    bool IsSetName;
    Popup_Name _popup_Name;
    public void OpenNamePopup()
    {
        if (_popup_Name == null)
        {
            var target = Resources.Load<GameObject>("Popup/Popup_Name");
            if (target != null)
            {
                var item = Instantiate(target, Popup_Trans);
                _popup_Name = item.GetComponent<Popup_Name>();
            }
        }
        _popup_Name.Open();
        _popup_Name.SetPopup(NameCallback);
    }

    void NameCallback()
    {
        IsSetName = false;
        GetNextStory();
    }
    #endregion
    //------------------------------------------------------------------------------------------------------------------------------------------------

    #region BGM
    string _currentBGM = "";
    void CheckBGM()
    {
        if (string.IsNullOrEmpty(_currentBGM))//브금 꺼져있음
        {
            if (!string.IsNullOrEmpty(_currentStory.BGM))
            {
                SoundManager.Instance.PlayBGM(_currentStory.BGM, _currentStory.BGM_Fade_Time);
                _currentBGM = _currentStory.BGM;
            }
        }
        else//브금 틀어져있음
        {
            if (string.IsNullOrEmpty(_currentStory.BGM))
            {
                SoundManager.Instance.StopBGM();
                _currentBGM = "";
            }
            else
            {
                if (_currentStory.BGM != _currentBGM)
                {
                    SoundManager.Instance.PlayBGM(_currentStory.BGM, _currentStory.BGM_Fade_Time);
                    _currentBGM = _currentStory.BGM;
                }
            }
        }
    }
    #endregion
    //------------------------------------------------------------------------------------------------------------------------------------------------
    #region CG_Effect
    void Check_CG_Effect()
    {
        if (CG_Effect_Manager.Instance == null)
            return;

        if (_currentStory.cg_production == null || _currentStory.cg_production.Count <= 0)
        {
            CG_Effect_Manager.Instance.OffEffects();
        }
        else
        {
            CG_Effect_Manager.Instance.SetEffect(_currentStory.cg_production);
        }
    }
    #endregion
    //------------------------------------------------------------------------------------------------------------------------------------------------
    #region SFX
    void Check_SFX()
    {
        if (_currentStory.SFX == null || _currentStory.SFX.Count <= 0)
        {
            SoundManager.Instance.StopSFX();
        }
        else
        {
            SoundManager.Instance.PlaySFX(_currentStory.SFX, _currentStory.SFX_Type);
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

        if (_popup_Name != null && _popup_Name.gameObject.activeSelf)
        {
            return true;
        }

        if (Resource_Manager.Instance.Get_Yes_Or_No_Active())
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
