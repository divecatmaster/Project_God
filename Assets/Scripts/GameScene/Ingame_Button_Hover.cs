using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;

public class Ingame_Button_Hover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] Image Glow;
    [SerializeField] TextMeshProUGUI Text;

    Color Common_Off_Color = new Color(1f, 1f, 1f, 0f);
    Color Common_On_Color = new Color(1f, 1f, 1f, 1f);
    Color Glow_Color = new Color(1f, 1f, 1f, 0.29f);
    public void OnPointerEnter(PointerEventData eventData)
    {
        Text.DOColor(Common_On_Color, 0.7f);
        Glow.DOColor(Glow_Color, 0.7f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Text.DOColor(Common_Off_Color, 0.7f);
        Glow.DOColor(Common_Off_Color, 0.7f);
    }
}
