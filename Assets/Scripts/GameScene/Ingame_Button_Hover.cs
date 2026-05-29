using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;

public class Ingame_Button_Hover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] Hover_Type Type;
    [SerializeField] Image Glow;
    [SerializeField] TextMeshProUGUI Text;

    [Header("Select")]
    [SerializeField] Image Button_BG;
    [SerializeField] Image Button_Outline;
    [SerializeField] Sprite[] Button_Outline_Sprites;
    
    Color Glow_Color = new Color(1f, 1f, 1f, 0.29f);
    public void OnPointerEnter(PointerEventData eventData)
    {
        switch (Type)
        {
            case Hover_Type.Button:
                {
                    Text.DOColor(UIUtility.Common_On_Color, 0.7f);
                    Glow.DOColor(Glow_Color, 0.7f);
                }
                break;
            case Hover_Type.Select:
                {
                    Button_BG.DOColor(UIUtility.Select_On_Color, 0.7f);
                    Button_Outline.DOColor(UIUtility.Select_On_Line_Color, 0.7f);
                    Button_Outline.sprite = Button_Outline_Sprites[1];
                    Text.DOColor(UIUtility.Select_On_Font_Color, 0.7f);
                }
                break;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        switch (Type)
        {
            case Hover_Type.Button:
                {
                    Text.DOColor(UIUtility.Common_Off_Color, 0.7f);
                    Glow.DOColor(UIUtility.Common_Off_Color, 0.7f);
                }
                break;
            case Hover_Type.Select:
                {
                    Button_BG.DOColor(UIUtility.Select_Off_Color, 0.7f);
                    Button_Outline.DOColor(UIUtility.Select_Off_Line_Color, 0.7f);
                    Button_Outline.sprite = Button_Outline_Sprites[0];
                    Text.DOColor(UIUtility.Select_Off_Font_Color, 0.7f);
                }
                break;
        }
    }
}

public enum Hover_Type
{
    None,
    Button,
    Select
}
