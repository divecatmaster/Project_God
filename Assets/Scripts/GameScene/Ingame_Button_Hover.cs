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
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        KillTweens();

        switch (type)
        {
            case Hover_Type.Button:
                text?.DOColor(UIUtility.Common_Off_Color, tweenDuration);
                glow?.DOColor(UIUtility.Common_Off_Color, tweenDuration);
                break;

            case Hover_Type.Select:
                buttonBG?.DOColor(UIUtility.Select_Off_Color, tweenDuration);
                buttonOutline?.DOColor(UIUtility.Select_Off_Line_Color, tweenDuration);

                if (buttonOutline != null && buttonOutlineSprites != null && buttonOutlineSprites.Length > 0)
                    buttonOutline.sprite = buttonOutlineSprites[0];

                text?.DOColor(UIUtility.Select_Off_Font_Color, tweenDuration);
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