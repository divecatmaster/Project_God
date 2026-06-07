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
    [SerializeField] TextMeshProUGUI Context;
    [SerializeField] TextMeshProUGUI Name_Text;
    [SerializeField] Image BG;
    [SerializeField] Image Body_Img;
    [SerializeField] Image Face_Img;
    [SerializeField] CanvasGroup NameDeco;
    
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

    [SerializeField] Story_Data _currentStory;
    string _currentBody;
    string _currentFace;
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
    }

    private void Start()
    {
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
        SetStory();
    }

    float _currentAutoTime = 0f;
    private void Update() 
    {
        if (IsActivePopup())
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
    }

    public void LoadGame(Action endCallback)
    {
        var saveData = Data_Manager.Instance.Get_TempSavedata();
        var target = Data_Manager.Instance.GetStoryData(saveData.StoryIndex);
        if (target != null)
        {
            _currentStory = target;
        }
        Data_Manager.Instance.StartTimer(saveData.PlayTime);
        SetStory();
        endCallback.Invoke();
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
        }
    }

    void GetNextStory()
    {
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
        if (!_isBusy) return;
        Stop_Typewriter();
        Context.maxVisibleCharacters = LanguageManager.Instance.GetText(_currentStory.Language_Key).Length;
    }

    private IEnumerator TypeRoutine()
    {
        _isBusy = true;
        Context.ForceMeshUpdate();

        int totalVisibleCharacters = Context.textInfo.characterCount;
        int counter = 0;
        
        float waitTime = Mathf.Lerp(0.12f, 0.001f, Data_Manager.Instance.TextSpeed / 100f);

        if (Data_Manager.Instance.TextSpeed >= 100)
        {
            Context.maxVisibleCharacters = totalVisibleCharacters;
            _isBusy = false;
            yield break;
        }

        while (counter <= totalVisibleCharacters)
        {
            Context.maxVisibleCharacters = counter;

            counter++;
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
        var name = _currentStory.Name;
        if (name == 0)
        {
            if (NameDeco.gameObject.activeSelf)
            {
                NameDeco.DOFade(0f, 0.5f).OnComplete(() => NameDeco.gameObject.SetActive(false));
                Name_Text.DOFade(0f, 0.5f).OnComplete(() => Name_Text.text = "");
            }
        }
        else
        {
            Name_Text.text = LanguageManager.Instance.GetText($"Name_{name}");
            if (!NameDeco.gameObject.activeSelf)
            {
                NameDeco.gameObject.SetActive(true);
                NameDeco.alpha = 0f;
                NameDeco.DOFade(1f, 0.5f);
                
                Name_Text.DOFade(1f, 0.5f);
            }
        }
    }

    void Set_Character()
    {
        if (string.IsNullOrEmpty(_currentStory.Body))
        {
            Body_Img.gameObject.SetActive(false);
            _currentBody = "";
        }
        else
        {
            if (_currentBody != _currentStory.Body)
            {
                var target = Resource_Manager.Instance.Get_Body_Image(_currentStory.Body);
                if (target != null)
                {
                    Body_Img.sprite = target;
                    Body_Img.gameObject.SetActive(true);
                    _currentBody = _currentStory.Body;
                }
                else
                {
                    Body_Img.gameObject.SetActive(false);
                    _currentBody = "";
                }
            }

            if (_currentFace != _currentStory.Face)
            {
                var target = Resource_Manager.Instance.Get_Face_Image(_currentStory.Face);
                if (target != null)
                {
                    Face_Img.sprite = target;
                    Face_Img.gameObject.SetActive(true);
                    _currentFace = _currentStory.Face;
                }
                else
                {
                    Face_Img.gameObject.SetActive(false);
                    _currentFace = "";
                }
            }
        }
    }

    void Set_BG()
    {
        BG.sprite = Resource_Manager.Instance.Get_BG(_currentStory.BG);
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

        if (Resource_Manager.Instance.Get_Yes_Or_No().gameObject.activeSelf)
        {
            return true;
        }

        return false;
    }
    #endregion
}
