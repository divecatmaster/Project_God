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
                //정환 여기 수정 필요
                str += "닉네임";
            }
            else
            {
                var name = LanguageManager.Instance.GetText($"Name_{data.Name}");
                var color = UIUtility.ColorToHex(Data_Manager.Instance.GetNameColor(data.Name));
                var result = $"<color=#{color}>{name}</color> : ";
                str += result;
            }
        }

        str += LanguageManager.Instance.GetText(data.Language_Key);
        Text.text = str;
    }
}
