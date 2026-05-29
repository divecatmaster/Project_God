using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class StoryManager : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI Context;

    [Header("Buttons")]
    [SerializeField] Button NextBtn;

    Story_Data _currentStory;

    private void Awake()
    {
        NextBtn.onClick.AddListener(OnClickNext);
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
            Play_Typewriter();
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
    #region Production
    private Coroutine _typeRoutine;
    private bool _isBusy;
    private float charactersPerSecond = 20f;
    void Play_Typewriter()
    {
        Stop_Typewriter();
        Context.text = LanguageManager.Instance.GetText(_currentStory.Language_Key);
        Context.maxVisibleCharacters = 0;
        _typeRoutine = StartCoroutine(TypeRoutine());
    }

    public void Stop_Typewriter()
    {
        if (_typeRoutine != null)
        {
            StopCoroutine(_typeRoutine);
            _typeRoutine = null;
        }
        _isBusy = false;
    }

    public void Skip()
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
}
