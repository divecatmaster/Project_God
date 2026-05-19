#if UNITY_EDITOR
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class TSVReader : MonoBehaviour
{
    private static string LINE_SPLIT_RE = @"\r\n|\n\r|\n|\r";

    public static List<SheetData> Read(string data)
    {
        List<SheetData> result = new List<SheetData>();
        SheetData temp = new SheetData();
        foreach (string words in Regex.Split(data, LINE_SPLIT_RE))
        {
            string[] text = words.Split('\t');
            result.Add(temp.SetData(text[0], text[1], text[2]));
        }
        return result;
    }
}
#endif