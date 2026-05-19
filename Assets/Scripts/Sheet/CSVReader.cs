using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
public class CSVReader
{
    static string SPLIT_RE = @",(?=(?:[^""]*""[^""]*"")*(?![^""]*""))";
    static string LINE_SPLIT_RE = @"\r\n|\n\r|\n|\r";
    static char[] TRIM_CHARS = { '\"' };

    public static string GetPlayDataString(byte[] _secret, TextAsset file)
    {
        byte[] bytes = Convert.FromBase64String(file.text);
        // Decrypt '_value' with 3DES.  
        TripleDES des = new TripleDESCryptoServiceProvider();
        des.Key = _secret;
        des.Padding = PaddingMode.PKCS7;
        des.Mode = CipherMode.ECB;
        //des.Padding = PaddingMode.PKCS7;
        ICryptoTransform xform = des.CreateDecryptor();
        byte[] decrypted = xform.TransformFinalBlock(bytes, 0, bytes.Length);
        // decrypte_value as a proper string.  
        string decryptedString = System.Text.Encoding.UTF8.GetString(decrypted);
        return decryptedString;

    }
    public static byte[] SetPlayDataString(byte[] file)
    {
        string userName = "Ahn Jerkins";
        MD5 md5Hash = new MD5CryptoServiceProvider();
        byte[] secret;

        secret = md5Hash.ComputeHash(System.Text.Encoding.UTF8.GetBytes(userName));

        byte[] bytes = file;
        // Decrypt '_value' with 3DES.  
        TripleDES des = new TripleDESCryptoServiceProvider();
        des.Key = secret;
        des.Padding = PaddingMode.PKCS7;
        des.Mode = CipherMode.ECB;
        //des.Padding = PaddingMode.PKCS7;
        ICryptoTransform xform = des.CreateEncryptor();
        byte[] enrt = xform.TransformFinalBlock(bytes, 0, bytes.Length);
        //string encstr = Convert.ToBase64String(enrt);

        return enrt;

    }
    public static List<Dictionary<string, object>> Read(string file)
    {
        string userName = "Ahn Jerkins";
        MD5 md5Hash = new MD5CryptoServiceProvider();
        byte[] secret;

        secret = md5Hash.ComputeHash(System.Text.Encoding.UTF8.GetBytes(userName));

        var list = new List<Dictionary<string, object>>();
        TextAsset data = Resources.Load("CSV/" + file) as TextAsset; //암호화된것을 불러온다.
        string aaa = GetPlayDataString(secret, data);

        //복호화시키고
        //string aaa= SetPlayDataString(secret, data);

        //MonoBehaviour.print(aaa);

        //return null;

        var lines = Regex.Split(aaa, LINE_SPLIT_RE);

        if (lines.Length <= 1) return list;

        var header = Regex.Split(lines[0], SPLIT_RE);
        for (var i = 1; i < lines.Length; i++)
        {

            var values = Regex.Split(lines[i], SPLIT_RE);
            if (values.Length == 0 || values[0] == "") continue;

            var entry = new Dictionary<string, object>();
            for (var j = 0; j < header.Length && j < values.Length; j++)
            {
                string value = values[j];
                value = value.TrimStart(TRIM_CHARS).TrimEnd(TRIM_CHARS).Replace("\\", "");
                object finalvalue = value;
                int n;
                float f;
                if (int.TryParse(value, out n))
                {
                    finalvalue = n;
                }
                else if (float.TryParse(value, out f))
                {
                    finalvalue = f;
                }
                entry[header[j]] = finalvalue;
            }
            list.Add(entry);
        }
        return list;
    }
    public static bool JP = false;

    public static List<Dictionary<string, object>> ReadOriginal(string file)
    {

        var list = new List<Dictionary<string, object>>();
        TextAsset data = Resources.Load("CSV/" + file) as TextAsset; //암호화된것을 불러온다.

        string aaaa = data.text;
        //복호화시키고



        var lines = Regex.Split(aaaa, LINE_SPLIT_RE);

        if (lines.Length <= 1) return list;

        var header = Regex.Split(lines[0], SPLIT_RE);
        for (var i = 1; i < lines.Length; i++)
        {

            var values = Regex.Split(lines[i], SPLIT_RE);
            if (values.Length == 0 || values[0] == "") continue;

            var entry = new Dictionary<string, object>();
            for (var j = 0; j < header.Length && j < values.Length; j++)
            {
                string value = values[j];
                value = value.TrimStart(TRIM_CHARS).TrimEnd(TRIM_CHARS).Replace("\\", "");
                value = value.Replace("@n", "\n");


                /*
                if (JP)
                    if(file!= "ingame_tuto" && file != "Start_Tuto" && file != "freeze_news" && file != "tutorial" && file != "daily_quest_info")
                    {
                        //value = JapaneseLineBreaker.Split(value);

                    }
                */
                object finalvalue = value;
                int n;
                float f;
                if (int.TryParse(value, out n))
                {
                    finalvalue = n;
                }
                else if (float.TryParse(value, out f))
                {
                    finalvalue = f;
                }



                entry[header[j]] = finalvalue;
            }
            list.Add(entry);
        }
        return list;
    }
}
