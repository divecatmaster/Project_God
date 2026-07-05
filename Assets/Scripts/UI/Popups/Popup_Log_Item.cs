using TMPro;
using UnityEngine;

public class Popup_Log_Item : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI Name;
    [SerializeField] GameObject Line;
    [SerializeField] GameObject Select;
    [SerializeField] TextMeshProUGUI Context;

    private void Start() 
    {
        Name.font = Font_Manager.Instance.GetFont();
        Context.font = Font_Manager.Instance.GetFont();
    }

    public void SetItem(Story_Data story)
    {
        if (story.Name == 0)
        {
            Name.text = "";
            Line.SetActive(false);
        }
        else if (story.Name == 9)
        {
            Name.text = Data_Manager.Instance.MyName;
            Name.color = Data_Manager.Instance.GetNameColor(story.Name);
            Line.SetActive(true);
        }
        else if (story.Name == 6)
        {
            var name = Data_Manager.Instance.MyName;
            Name.text = string.Format(LanguageManager.Instance.GetText($"Name_{story.Name}"), name);
            Name.color = Data_Manager.Instance.GetNameColor(story.Name);
            Line.SetActive(true);
        }
        else
        {
            Name.text = LanguageManager.Instance.GetText($"Name_{story.Name}");
            Name.color = Data_Manager.Instance.GetNameColor(story.Name);
            Line.SetActive(true);
        }

        if (story.My_Name)
        {
            Context.text = string.Format(LanguageManager.Instance.GetText(story.Language_Key), Data_Manager.Instance.MyName);
        }
        else
        {
            Context.text = LanguageManager.Instance.GetText(story.Language_Key);
        }

        if (story.Select_Index == 0)
        {
            Select.SetActive(false);
        }
        else
        {
            var selected = Data_Manager.Instance.GetSavedSelectData(story.Select_Index);
            Context.text = LanguageManager.Instance.GetText($"select_{story.Select_Index}_{selected + 1}");
            Select.SetActive(true);
        }
    }

    public void SetItemBG(Story_Data story)
    {
        Name.text = $"[{LanguageManager.Instance.GetText($"BG_Title_{story.BG}")}]";
        Name.color = Color.white;
        Line.SetActive(false);
        Select.SetActive(false);
        Context.text = "";
    }
}
