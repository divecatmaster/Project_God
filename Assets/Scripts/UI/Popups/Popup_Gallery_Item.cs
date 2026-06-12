using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;
using System;

public class Popup_Gallery_Item : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] Button Btn;
    [SerializeField] GameObject Active;
    
    [SerializeField] Image BG;
    [SerializeField] Image BG_Glow;

    [Header("Lock")]
    [SerializeField] GameObject Lock;
    [SerializeField] Image Lock_Glow;
    [SerializeField] TextMeshProUGUI Lock_Text;

    [Header("Bot")]
    [SerializeField] TextMeshProUGUI NumText;
    [SerializeField] TextMeshProUGUI Title;
    [SerializeField] Image Star;
    [SerializeField] GameObject Deco_Bot_1;
    [SerializeField] Image Deco_Bot_2;

    

    Action<int> _callback;
    int _idx;
    bool _isOpen;
    private void Awake() 
    {
        Btn.onClick.AddListener(OnClickBtn);
    }

    public void SetItem(int idx, Gallery_Data data, Action<int> callback)
    {
        _idx = idx;
        _callback = callback;
        KillTweens();

        _isOpen = Data_Manager.Instance.IsOpenGallery(data.Index);
        if (_isOpen)
        {
            Active.SetActive(true);
            Lock.SetActive(false);
            Title.text = LanguageManager.Instance.GetText(data.TextKey);
            NumText.text = data.Index.ToString("00");
            BG.sprite = Resource_Manager.Instance.Get_BG(data.BG);
            Deco_Bot_1.SetActive(true);
            Deco_Bot_2.gameObject.SetActive(true);
            Deco_Bot_2.color = UIUtility.Common_Off_Color;
            Star.gameObject.SetActive(true);
        }
        else
        {
            Active.SetActive(false);
            Lock.SetActive(true);
            Title.text = "???";
            NumText.text = "";
            Deco_Bot_1.SetActive(false);
            Deco_Bot_2.gameObject.SetActive(false);
            Star.gameObject.SetActive(false);
            Lock_Glow.color = UIUtility.Common_Off_Color;
            Lock_Text.alpha = 0f;
        }
    }

    private void KillTweens()
    {
        Lock_Glow.DOKill();
        Lock_Text.DOKill();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        KillTweens();

        Title.DOColor(UIUtility.Gallery_Lock_Text_Color, 0.7f);
        if (_isOpen)
        {
            
        }
        else
        {
            
            Lock_Glow.DOColor(UIUtility.Gallery_Lock_Glow_Color, 0.7f);
            Lock_Text.DOColor(UIUtility.Gallery_Lock_Text_Color, 0.7f);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        KillTweens();

        if (_isOpen)
        {
            
        }
        else
        {
            Title.DOColor(UIUtility.Gallery_Lock_Text_Color, 0.7f);
            Lock_Glow.DOColor(UIUtility.Common_Off_Color, 0.7f);
            Lock_Text.DOColor(UIUtility.Common_Off_Color, 0.7f);
        }
    }

    void OnClickBtn()
    {
        _callback?.Invoke(_idx);
    }
}
