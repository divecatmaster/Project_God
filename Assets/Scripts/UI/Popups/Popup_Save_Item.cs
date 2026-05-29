using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Popup_Save_Item : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI Slot_Index;

    [Header("Image")]
    [SerializeField] GameObject Slot;
    [SerializeField] Image BG;
    [SerializeField] Image Character;
    [SerializeField] Image Face;
    
    [Header("Data")]
    [SerializeField] GameObject Saved;
    [SerializeField] GameObject Empty;
    [SerializeField] TextMeshProUGUI Map_Name;
    [SerializeField] TextMeshProUGUI SaveTime;
    [SerializeField] TextMeshProUGUI PlayTime;
    
    [Header("Save")]
    [SerializeField] GameObject Right;
    [SerializeField] TextMeshProUGUI SaveTitle;
    [SerializeField] Button SaveButton;
    [SerializeField] Button RemoveButton;

    Action<int> _callback;
    Save_Data _data;
    private void Awake()
    {
        SaveButton.onClick.AddListener(OnClickSave);
    }

    public void SetItem(int groupType, Save_Data data, Action<int> callback)
    {
        _data = data;
        _callback = callback;
        if (groupType == 0)
        {
            SaveTitle.text = LanguageManager.Instance.GetText("Save_Title");
        }
        else
        {
            SaveTitle.text = LanguageManager.Instance.GetText("Load_Title");
        }

        Slot_Index.text = data.SlotIndex.ToString("00");
        if (data.StoryIndex == -1)//empty
        {
            Slot.gameObject.SetActive(false);
            Saved.SetActive(false);
            Empty.SetActive(true);
            RemoveButton.gameObject.SetActive(false);
            Right.SetActive(false);
        }
        else
        {
            Slot.gameObject.SetActive(true);
            Saved.SetActive(true);
            Empty.SetActive(false);
            RemoveButton.gameObject.SetActive(true);
            Right.SetActive(true);
            var storyData = Data_Manager.Instance.GetStoryData(data.StoryIndex);
            if (storyData != null)
            {
                var bg = Resource_Manager.Instance.Get_BG(storyData.BG);
                if (bg != null)
                {
                    BG.sprite = bg;
                }

                if (string.IsNullOrEmpty(storyData.Body))
                {
                    Character.gameObject.SetActive(false);
                }
                else
                {
                    var body = Resource_Manager.Instance.Get_Body_Image(storyData.Body);
                    if (body != null)
                    {
                        Character.sprite = body;
                        Character.gameObject.SetActive(true);

                        if (string.IsNullOrEmpty(storyData.Face))
                        {
                            Face.gameObject.SetActive(false);
                        }
                        else
                        {
                            var face = Resource_Manager.Instance.Get_Face_Image(storyData.Face);
                            if (face != null)
                            {
                                Face.sprite = face;
                                Face.gameObject.SetActive(true);
                            }
                            else
                            {
                                Face.gameObject.SetActive(false);
                            }
                        }
                    }
                    else
                    {
                        Character.gameObject.SetActive(false);
                    }
                }

                Map_Name.text = LanguageManager.Instance.GetText($"BG_Title_{storyData.BG}");
                SaveTime.text = data.SaveDate.ToString("yyyy.MM.dd   HH:mm");
                PlayTime.text = $"{(int)data.PlayTime.TotalHours:00}:{data.PlayTime.Minutes:00}:{data.PlayTime.Seconds:00}";
            }
        }
    }

    void OnClickSave()
    {
        _callback?.Invoke(_data.SlotIndex);
    }
}
