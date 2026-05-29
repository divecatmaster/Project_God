using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Resources;

public class StoryManager : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI Context;
    [SerializeField] TextMeshProUGUI Name_Text;
    [SerializeField] Image Body_Img;
    [SerializeField] Image Face_Img;
    
    [Header("Default")]
    [SerializeField] GameObject Default_Obj;

    [Header("Select")]
    [SerializeField] GameObject Select_Obj;
    [SerializeField] GameObject[] Select_Buttons;
    [SerializeField] TextMeshProUGUI[] Select_Texts;

    [Header("Hide")]
    [SerializeField] GameObject[] HideTargets;

    [Header("Buttons")]
    [SerializeField] Button NextBtn;
    [SerializeField] Button HideBtn;
    [SerializeField] Button AppearBtn;

    Story_Data _currentStory;
    string _currentBody;
    string _currentFace;

    private void Awake()
    {
        NextBtn.onClick.AddListener(OnClickNext);
        HideBtn.onClick.AddListener(OnClickHide);
        AppearBtn.onClick.AddListener(OnClickAppear);
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

    void SetStory()
    {
        if (_currentStory != null)
        {
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
        GetNextStory();
    }
    //------------------------------------------------------------------------------------------------------------------------------------------------
    #region Default_Production
    private Coroutine _typeRoutine;
    private bool _isBusy;
    private float charactersPerSecond = 20f;
    void Play_Typewriter()
    {
        Default_Obj.SetActive(true);
        Select_Obj.SetActive(false);
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

        float waitTime = 1f / Mathf.Max(0.1f, charactersPerSecond);

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
        Select_Obj.SetActive(true);

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
        for (int i = 0; i < HideTargets.Length; i++)
        {
            HideTargets[i].gameObject.SetActive(false);
        }
        AppearBtn.gameObject.SetActive(true);
    }

    void OnClickAppear()
    {
        for (int i = 0; i < HideTargets.Length; i++)
        {
            HideTargets[i].gameObject.SetActive(true);
        }
        AppearBtn.gameObject.SetActive(false);
        if (_currentStory.Select_Index == 0)
        {
            Select_Obj.SetActive(false);
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
            Name_Text.text = "";
        }
        else
        {
            Name_Text.text = LanguageManager.Instance.GetText($"Name_{name}");
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
    #endregion
}
