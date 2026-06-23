using TMPro;
using UnityEngine;

public class Popup_Gallery_Detail_ContextItem : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI Text;

    public void SetText(Story_Data data)
    {
        string str = "";
        if (data.Name != 0)
        {
            if (data.Name == 9)
            {
                var name = Data_Manager.Instance.MyName;
                var color = UIUtility.ColorToHex(Data_Manager.Instance.GetNameColor(data.Name));
                var result = $"<color=#{color}>{name}</color> : ";
                str += result;
            }
            else if (data.Name == 6)
            {
                var name = LanguageManager.Instance.GetText($"Name_{data.Name}");
                var color = UIUtility.ColorToHex(Data_Manager.Instance.GetNameColor(data.Name));
                var result = $"<color=#{color}>{string.Format(name, Data_Manager.Instance.MyName)}</color> : ";
                str += result;
            }
            else
            {
                var name = LanguageManager.Instance.GetText($"Name_{data.Name}");
                var color = UIUtility.ColorToHex(Data_Manager.Instance.GetNameColor(data.Name));
                var result = $"<color=#{color}>{name}</color> : ";
                str += result;
            }
        }

        if (data.My_Name)
        {
            var temp = string.Format(LanguageManager.Instance.GetText(data.Language_Key), Data_Manager.Instance.MyName);
            str += temp;
        }
        else
        {
            str += LanguageManager.Instance.GetText(data.Language_Key);
        }
        Text.text = str;
    }
}
