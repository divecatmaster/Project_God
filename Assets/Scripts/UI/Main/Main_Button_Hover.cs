using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;

public class Main_Button_Hover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] Image BG_On;
    [SerializeField] Image BG_Off;
    [SerializeField] TextMeshProUGUI Name;
    [SerializeField] Image[] Deco;

    Color Default_On_Color = new Color(1f, 1f, 1f, 0.45f);
    Color Default_Off_Color = new Color(1f, 1f, 1f, 0.55f);
    Color Common_Off_Color = new Color(1f, 1f, 1f, 0f);
    Color Deco_On_Color = new Color(1f, 1f, 1f, 1f);
    Color Deco_Off_Color = new Color(1f, 1f, 1f, 0.53f);
    void OnEnable()
    {
        BG_On.color = Common_Off_Color;
        BG_Off.color = Default_Off_Color;
        Name.color = UIUtility.HexToColor("d3e4ff");
        Deco[0].color = Deco_Off_Color;
        Deco[1].color = Deco_Off_Color;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        BG_On.DOColor(Default_On_Color, 0.5f);
        BG_Off.DOColor(Common_Off_Color, 0.5f);
        Name.DOColor(Color.white, 0.5f);
        Deco[0].DOColor(Deco_On_Color, 0.5f);
        Deco[1].DOColor(Deco_On_Color, 0.5f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        BG_On.DOColor(Common_Off_Color, 0.5f);
        BG_Off.DOColor(Default_Off_Color, 0.5f);
        Name.DOColor(UIUtility.HexToColor("d3e4ff"), 0.5f);
        Deco[0].DOColor(Deco_Off_Color, 0.5f);
        Deco[1].DOColor(Deco_Off_Color, 0.5f);
    }
}
