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

    #endregion
}