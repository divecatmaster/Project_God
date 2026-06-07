using TMPro;
using UnityEngine;

public class Popup_Log_Item : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI Name;
    [SerializeField] GameObject Line;
    [SerializeField] GameObject Select;
    [SerializeField] TextMeshProUGUI Context;
    [SerializeField] TextMeshProUGUI TimeText;

    public void SetItem(Story_Data story)
    {
        if (story.Name == 0)
        {
            Name.text = "";
            Line.SetActive(false);
        }
        else
        {
            Name.text = LanguageManager.Instance.GetText($"Name_{story.Name}");
            Name.color = Data_Manager.Instance.GetNameColor(story.Name);
            Line.SetActive(true);
        }

        if (story.Select_Index == 0)
        {
            Select.SetActive(false);
        }
        else
        {
            Select.SetActive(true);
        }

        Context.text = LanguageManager.Instance.GetText(story.Language_Key);
    }
}
