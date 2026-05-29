using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;

static public class UIUtility
{
    #region Color
    static public Color HexToColor(string hexCode)
    {
        Color color;
        if (ColorUtility.TryParseHtmlString($"#{hexCode}", out color))
        {
            return color;
        }
        return Color.white;
    }

    static public string ColorToHex(Color color)
    {
        return ColorUtility.ToHtmlStringRGB(color);
    }
    #endregion
    //------------------------------------------------------------------------------------------------------------------------------------
    #region Converter
    static public string IntToString(int num)
    {
        return num.ToString("#,##0");
    }

    static public int StringToInt(string num)
    {
        var temp = num.Replace(",", "");
        return int.Parse(temp);
    }
    #endregion
    //------------------------------------------------------------------------------------------------------------------------------------
    #region Config
    static public Color Common_Off_Color = new Color(1f, 1f, 1f, 0f);
    static public Color Common_On_Color = new Color(1f, 1f, 1f, 1f);
    static public Color Select_Off_Color = new Color(0.0627451f, 0.07843138f, 0.09411765f, 0.2f);
    static public Color Select_Off_Line_Color = new Color(0.8666667f, 0.9058824f, 0.9607843f, 0.1f);
    static public Color Select_Off_Font_Color = new Color(0.9490196f, 0.9647059f, 1f, 1f);
    static public Color Select_On_Color = new Color(0.2470588f, 0.3490196f, 0.4509804f, 0.6f);
    static public Color Select_On_Line_Color = new Color(1f, 1f, 1f, 0.6f);
    static public Color Select_On_Font_Color = new Color(0.8666667f, 0.9058824f, 1f, 1f);
    #endregion
}