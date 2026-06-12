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
    static public Color Save_On_Star_Color = new Color(0.8196079f, 0.8745098f, 1f, 1f);//D1DFFF
    static public Color Save_Off_Star_Color = new Color(0.5254902f, 0.5882353f, 0.7411765f, 1f);
    static public Color Save_On_Glow_Color = new Color(0.7607843f, 0.8470588f, 1f, 0.8f);
    static public Color Select_On_Remove_Color = new Color(0.6392157f, 0.6705883f, 0.7333333f, 1f);
    static public Color YesOrNo_Off_BG_Color = new Color(0.2941177f, 0.3607843f, 0.5019608f, 1f);
    static public Color YesOrNo_Off_Text_Color = new Color(0.7372549f, 0.7647059f, 0.8156863f, 1f);
    static public Color YesOrNo_On_Glow_Color = new Color(0.4509804f, 0.5333334f, 0.7098039f, 0.6980392f);
    static public Color YesOrNo_On_BG_Color = new Color(0.4509804f, 0.5333334f, 0.7098039f, 1f);
    static public Color YesOrNo_On_Text_Color = new Color(0.7843137f, 0.8431373f, 0.945098f, 1f);
    static public Color Option_On_BG_Color = new Color(0.5764706f, 0.6666667f, 0.8235294f, 0.1f);
    static public Color Slider_On_Star_Color = new Color(0.8745098f, 0.9137255f, 1f, 1f);
    static public Color Slider_Off_Star_Color = new Color(0.5333334f, 0.6196079f, 0.8196079f, 1f);
    static public Color Gallery_Lock_Glow_Color = new Color(0.7647059f, 0.8117647f, 0.9960784f, 0.6f);
    static public Color Gallery_Lock_Text_Color = new Color(0.8627451f, 0.8941177f, 0.9647059f, 1f);//DCE4F6
    static public Color Gallery_Text_On_Color = new Color(0.9254902f, 0.9529412f, 1f, 1f);//ECF3FF
    static public Color Gallery_BG_Glow_Color = new Color(0.7137255f, 0.772549f, 0.9411765f, 1f);
    static public Color Gallery_Star_Off_Color = new Color(0.6039216f, 0.654902f, 0.7764706f, 1f);//9AA7C6
    static public Color Gallery_Dot_On_Color = new Color(0.5960785f, 0.6352941f, 0.7215686f, 1f);//98A2B8
    static public Color Gallery_Dot_Off_Color = new Color(0.1333333f, 0.1607843f, 0.2392157f, 1f);//22293D
    
    #endregion
}