using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;

public class Ingame_Button_Hover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Hover_Type type;

    [Header("Button")]
    [SerializeField] private Image glow;
    [SerializeField] private TextMeshProUGUI text;

    [Header("Select")]
    [SerializeField] private Image buttonBG;
    [SerializeField] private Image buttonOutline;
    [SerializeField] private Sprite[] buttonOutlineSprites;

    [Header("Save")]
    [SerializeField] Image Star;
    [SerializeField] Image Remove_Btn;

    [SerializeField] private float tweenDuration = 0.7f;

    private readonly Color glowOnColor = new Color(1f, 1f, 1f, 0.29f);

    private void Start()
    {
        SetDefault();
    }

    private void OnDisable()
    {
        KillTweens();
        SetDefault();
    }

    private void KillTweens()
    {
        text?.DOKill();
        glow?.DOKill();
        buttonBG?.DOKill();
        buttonOutline?.DOKill();
    }

    private void SetDefault()
    {
        KillTweens();

        switch (type)
        {
            case Hover_Type.Button:
            case Hover_Type.Gallery_Hide:
                if (text != null)
                    text.color = UIUtility.Common_Off_Color;

                if (glow != null)
                    glow.color = UIUtility.Common_Off_Color;
                break;

            case Hover_Type.Select:
                if (buttonBG != null)
                    buttonBG.color = UIUtility.Select_Off_Color;

                if (buttonOutline != null)
                {
                    buttonOutline.color = UIUtility.Select_Off_Line_Color;

                    if (buttonOutlineSprites != null && buttonOutlineSprites.Length > 0)
                        buttonOutline.sprite = buttonOutlineSprites[0];
                }

                if (text != null)
                    text.color = UIUtility.Select_Off_Font_Color;
                break;
            case Hover_Type.Save:
                {
                    Star.color = UIUtility.Save_Off_Star_Color;
                    glow.color = UIUtility.Common_Off_Color;
                    text.color = UIUtility.Save_Off_Star_Color;
                    Remove_Btn.color = UIUtility.Common_Off_Color;
                }
                break;
            case Hover_Type.YesOrNo:
                {
                    glow.color = UIUtility.Common_Off_Color;
                    buttonBG.color = UIUtility.YesOrNo_Off_BG_Color;
                    text.color = UIUtility.YesOrNo_Off_Text_Color;
                }
                break;
            case Hover_Type.Option:
                {
                    buttonBG.color = UIUtility.Common_Off_Color;
                    Star.color = UIUtility.Common_Off_Color;
                    text.color = UIUtility.YesOrNo_Off_Text_Color;
                }
                break;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        KillTweens();

        switch (type)
        {
            case Hover_Type.Button:
                text?.DOColor(UIUtility.Common_On_Color, tweenDuration);
                glow?.DOColor(glowOnColor, tweenDuration);
                break;

            case Hover_Type.Select:
                buttonBG?.DOColor(UIUtility.Select_On_Color, tweenDuration);
                buttonOutline?.DOColor(UIUtility.Select_On_Line_Color, tweenDuration);

                if (buttonOutline != null && buttonOutlineSprites != null && buttonOutlineSprites.Length > 1)
                    buttonOutline.sprite = buttonOutlineSprites[1];

                text?.DOColor(UIUtility.Select_On_Font_Color, tweenDuration);
                break;
            case Hover_Type.Save:
                {
                    Star.DOColor(UIUtility.Save_On_Star_Color, tweenDuration);
                    glow.DOColor(UIUtility.Common_Off_Color, tweenDuration);
                    text.DOColor(UIUtility.Save_On_Star_Color, tweenDuration);
                    Remove_Btn.DOColor(UIUtility.Select_On_Remove_Color, tweenDuration);
                }
                break;
            case Hover_Type.YesOrNo:
                {
                    glow.DOColor(UIUtility.YesOrNo_On_Glow_Color, tweenDuration);
                    buttonBG.DOColor(UIUtility.YesOrNo_On_BG_Color, tweenDuration);
                    text.DOColor(UIUtility.YesOrNo_Off_Text_Color, tweenDuration);
                }
                break;
            case Hover_Type.Option:
                {
                    buttonBG?.DOColor(UIUtility.Option_On_BG_Color, tweenDuration);
                    Star.DOColor(UIUtility.Common_On_Color, tweenDuration);
                    text.DOColor(UIUtility.YesOrNo_On_Text_Color, tweenDuration);
                }
                break;
            case Hover_Type.Gallery_Hide:
                {
                    text?.DOColor(UIUtility.Common_On_Color, tweenDuration);
                    glow?.DOColor(UIUtility.Gallery_Hide_Glow_Color, tweenDuration);
                }
                break;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        KillTweens();

        switch (type)
        {
            case Hover_Type.Button:
            case Hover_Type.Gallery_Hide:
                {
                    text?.DOColor(UIUtility.Common_Off_Color, tweenDuration);
                    glow?.DOColor(UIUtility.Common_Off_Color, tweenDuration);
                }
                break;
            case Hover_Type.Select:
                {
                    buttonBG?.DOColor(UIUtility.Select_Off_Color, tweenDuration);
                    buttonOutline?.DOColor(UIUtility.Select_Off_Line_Color, tweenDuration);

                    if (buttonOutline != null && buttonOutlineSprites != null && buttonOutlineSprites.Length > 0)
                        buttonOutline.sprite = buttonOutlineSprites[0];

                    text?.DOColor(UIUtility.Select_Off_Font_Color, tweenDuration);
                }
                break;
            case Hover_Type.Save:
                {
                    Star.DOColor(UIUtility.Save_Off_Star_Color, tweenDuration);
                    glow.DOColor(UIUtility.Common_Off_Color, tweenDuration);
                    text.DOColor(UIUtility.Save_Off_Star_Color, tweenDuration);
                    Remove_Btn.DOColor(UIUtility.Common_Off_Color, tweenDuration);
                }
                break;
            case Hover_Type.YesOrNo:
                {
                    glow.DOColor(UIUtility.Common_Off_Color, tweenDuration);
                    buttonBG.DOColor(UIUtility.YesOrNo_Off_BG_Color, tweenDuration);
                    text.DOColor(UIUtility.YesOrNo_Off_Text_Color, tweenDuration);
                }
                break;
            case Hover_Type.Option:
                {
                    buttonBG?.DOColor(UIUtility.Common_Off_Color, tweenDuration);
                    Star.DOColor(UIUtility.Common_Off_Color, tweenDuration);
                    text.DOColor(UIUtility.YesOrNo_Off_Text_Color, tweenDuration);
                }
                break;
        }
    }
}

public enum Hover_Type
{
    None,
    Button,
    Select,
    Save,
    YesOrNo,
    Option,
    Gallery_Hide
}