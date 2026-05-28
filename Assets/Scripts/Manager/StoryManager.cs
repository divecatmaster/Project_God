using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
            Context.text = LanguageManager.Instance.GetText(_currentStory.Language_Key);
        }
    }

    void GetNextStory()
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

    void OnClickNext()
    {
        GetNextStory();
    }
}
