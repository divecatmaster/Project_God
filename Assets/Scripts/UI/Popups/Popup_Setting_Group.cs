using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Popup_Setting_Group : MonoBehaviour
{
    [SerializeField] Button Btn;
    [SerializeField] Image Star;
    [SerializeField] RectTransform TextTrans;
    [SerializeField] TextMeshProUGUI Text;
    [SerializeField] Image Line;

    Action<int> _callback;
    int _idx;
    Color _lineColor = new Color(0.6901961f, 0.7607843f, 0.8901961f, 1f);
    Color _textOffColor = new Color(0.8352941f, 0.8352941f, 0.8352941f, 1f);
    Vector2 _textSmall = new Vector2(132.31f, 160);
    Vector2 _textLarge = new Vector2(168.65f, 160);
    private void Awake()
    {
        Btn.onClick.AddListener(OnClickBtn);
    }

    public void SetButton(int idx, Action<int> callback)
    {
        _idx = idx;
        _callback = callback;
        SetSelected(false);
    }

    public void SetSelected(bool isSelected, bool isImmediately = true)
    {
        if (isSelected)
        {
            if (isImmediately)
            {
                Star.color = UIUtility.Common_On_Color;
                Line.color = _lineColor;
                TextTrans.sizeDelta = _textSmall;
                Text.color = UIUtility.YesOrNo_On_Text_Color;
            }
            else
            {
                Star.DOColor(UIUtility.Common_On_Color, 0.5f);
                Line.DOColor(_lineColor, 0.5f);
                TextTrans.DOSizeDelta(_textSmall, 0.5f);
                Text.DOColor(UIUtility.YesOrNo_On_Text_Color, 0.5f);
            }
        }
        else
        {
            if (isImmediately)
            {
                Star.color = UIUtility.Common_Off_Color;
                Line.color = UIUtility.Common_Off_Color;
                TextTrans.sizeDelta = _textLarge;
                Text.color = _textOffColor;
            }
            else
            {
                Star.DOColor(UIUtility.Common_Off_Color, 0.5f);
                Line.DOColor(UIUtility.Common_Off_Color, 0.5f);
                TextTrans.DOSizeDelta(_textLarge, 0.5f);
                Text.DOColor(_textOffColor, 0.5f);
            }
        }
    }

    void OnClickBtn()
    {
        _callback?.Invoke(_idx);
    }
}
