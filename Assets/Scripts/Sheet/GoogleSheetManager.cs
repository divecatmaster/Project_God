#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEditor;
using System;

public class SheetData
{
    public string name;
    public string url;
    public string group;
    public SheetData SetData(string _name, string _url, string _group)
    {
        SheetData sheetData = new SheetData();
        sheetData.name = _name;
        sheetData.url = _url;
        sheetData.group = _group;
        return sheetData;
    }
}




public class GoogleSheetManager : MonoBehaviour
{
    //private string url = "https://docs.google.com/spreadsheets/d/1m2NbZP72bcnHJa_o6xj9rESUxQCdCh5gP02FWu0G9UE/export?gid=57827555&format=tsv&range=A3:C";
    private string url = "https://docs.google.com/spreadsheets/d/1kuJM4DpKxa3vVgr0U9Nz8aj5AKTFbYXg_ubdOA1HVsE/export?gid=0&format=tsv&range=A3:C";

    List<string> names = new List<string>();
    List<string> names_enc = new List<string>();
    public void OnClickSheetNameSave()
    {
        names.Clear();
        names_enc.Clear();

        StartCoroutine(SaveGoogleSheetDatasToName(url, names));
        StartCoroutine(SaveGoogleSheetDatasToNameEnc(url, names_enc));
    }

    public void OnClickSave()
    {
        StartCoroutine(SaveGoogleSheetDatas(url));
    }
    public void OnClickSaveEnc()
    {
        StartCoroutine(SaveGoogleSheetDatasEnc(url));
    }

    public void OnClickSave(string _group)
    {
        StartCoroutine(SaveGoogleSheetDatas(url, _group));
    }
    public void OnClickSaveEnc(string _group)
    {
        StartCoroutine(SaveGoogleSheetDatasEnc(url, _group));
    }

    public void OnClickPartSave(string _name)
    {
        StartCoroutine(SaveGoogleSheetDatasToName(url, _name));
    }
    public void OnClickPartSaveEnc(string _name)
    {
        StartCoroutine(SaveGoogleSheetDatasToNameEnc(url, _name));
    }

    private IEnumerator SaveGoogleSheetDatas(string url)
    {
        WWWForm form = new WWWForm();
        Debug.Log(url);
        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            yield return www.SendWebRequest();
            if (www.isDone)
            {
                List<SheetData> a = TSVReader.Read(www.downloadHandler.text);
                foreach (SheetData _data in a)
                {
                    StartCoroutine(SaveGoogleSheetData(_data.name, _data.url));
                }
                Debug.Log("csv Download Complete");
            }
        }
    }
    private IEnumerator SaveGoogleSheetDatasEnc(string url)
    {
        WWWForm form = new WWWForm();
        Debug.Log(url);
        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            yield return www.SendWebRequest();
            if (www.isDone)
            {
                List<SheetData> a = TSVReader.Read(www.downloadHandler.text);
                foreach (SheetData _data in a)
                {
                    StartCoroutine(SaveGoogleSheetDataEnc(_data.name, _data.url));
                }
                Debug.Log("csv Download Complete");
            }
        }
    }

    private IEnumerator SaveGoogleSheetDatasToName(string url, List<string> _names)
    {
        WWWForm form = new WWWForm();
        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            yield return www.SendWebRequest();
            if (www.isDone)
            {
                List<SheetData> a = TSVReader.Read(www.downloadHandler.text);
                foreach (SheetData _data in a)
                {
                    if (_names.Contains(_data.name))
                    {
                        StartCoroutine(SaveGoogleSheetData(_data.name, _data.url));
                        Debug.Log(_data.name + ".csv Download Complete");
                    }
                }
            }
        }
    }

    private IEnumerator SaveGoogleSheetDatasToNameEnc(string url, List<string> _names)
    {
        WWWForm form = new WWWForm();
        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            yield return www.SendWebRequest();
            if (www.isDone)
            {
                List<SheetData> a = TSVReader.Read(www.downloadHandler.text);
                foreach (SheetData _data in a)
                {
                    if (_names.Contains(_data.name))
                    {
                        StartCoroutine(SaveGoogleSheetDataEnc(_data.name, _data.url));
                        Debug.Log(_data.name + ".csv Download Complete");
                    }
                }
            }
        }
    }

    private IEnumerator SaveGoogleSheetDatasToName(string url, string _name)
    {
        WWWForm form = new WWWForm();
        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            yield return www.SendWebRequest();
            if (www.isDone)
            {
                List<SheetData> a = TSVReader.Read(www.downloadHandler.text);
                foreach (SheetData _data in a)
                {
                    if (_name == _data.name)
                    {
                        StartCoroutine(SaveGoogleSheetData(_data.name, _data.url));
                        Debug.Log(_data.name + ".csv Download Complete");
                    }
                }
            }
        }
    }
    private IEnumerator SaveGoogleSheetDatasToNameEnc(string url, string _name)
    {
        WWWForm form = new WWWForm();
        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            yield return www.SendWebRequest();
            if (www.isDone)
            {
                List<SheetData> a = TSVReader.Read(www.downloadHandler.text);
                foreach (SheetData _data in a)
                {
                    if (_name == _data.name)
                    {
                        StartCoroutine(SaveGoogleSheetDataEnc(_data.name, _data.url));
                        Debug.Log(_data.name + ".csv Download Complete");
                    }
                }
            }
        }
    }

    private IEnumerator SaveGoogleSheetDatas(string url, string _group)
    {
        WWWForm form = new WWWForm();
        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            yield return www.SendWebRequest();
            if (www.isDone)
            {
                List<SheetData> a = TSVReader.Read(www.downloadHandler.text);
                foreach (SheetData _data in a)
                {
                    if (_data.group == _group)
                        StartCoroutine(SaveGoogleSheetData(_data.name, _data.url));
                }
                Debug.Log("csv Download Complete");
            }
        }
    }
    private IEnumerator SaveGoogleSheetDatasEnc(string url, string _group)
    {
        WWWForm form = new WWWForm();
        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            yield return www.SendWebRequest();
            if (www.isDone)
            {
                List<SheetData> a = TSVReader.Read(www.downloadHandler.text);
                foreach (SheetData _data in a)
                {
                    if (_data.group == _group)
                        StartCoroutine(SaveGoogleSheetDataEnc(_data.name, _data.url));
                }
                Debug.Log("csv Download Complete");
            }
        }
    }

    private IEnumerator SaveGoogleSheetData(string _name, string _url)
    {
        using (UnityWebRequest www = UnityWebRequest.Get(_url))
        {
            yield return www.SendWebRequest();
            if (www.isDone)
            {
                File.WriteAllBytes(PlatformPath(_name + ".csv"), System.Text.Encoding.UTF8.GetBytes(www.downloadHandler.text));
            }
        }
    }
    private IEnumerator SaveGoogleSheetDataEnc(string _name, string _url)
    {
        using (UnityWebRequest www = UnityWebRequest.Get(_url))
        {
            yield return www.SendWebRequest();
            if (www.isDone)
            {
                byte[] aaa = CSVReader.SetPlayDataString(System.Text.Encoding.UTF8.GetBytes(www.downloadHandler.text));
                //byte[] aaa = System.Text.Encoding.UTF8.GetBytes(www.downloadHandler.text);
                string bbb = Convert.ToBase64String(aaa);

                StreamWriter outStream = System.IO.File.CreateText(PlatformPath(_name + ".csv"));
                outStream.Write(bbb);
                outStream.Close();

                //File.WriteAllBytes(PlatformPath(_name + ".csv"), aaa);
            }
        }
    }
    // Google에서 시트 가져와서 암호화해서 저장하기


    public static string PlatformPath(string filename)
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            if (!Directory.Exists(Application.persistentDataPath + "/Resources/CSV/"))
            {
                Directory.CreateDirectory(Application.persistentDataPath + "/Resources");
                Directory.CreateDirectory(Application.persistentDataPath + "/Resources/CSV/");
                Directory.CreateDirectory(Application.persistentDataPath + "/Resources/CSV/");
            }
            string path = Application.persistentDataPath + "/Resources/CSV/";
            path = path.Substring(0, path.LastIndexOf('/'));
            return Path.Combine(path, filename);
        }
        else
        {
            if (!Directory.Exists(Application.dataPath + "/Resources/CSV/"))
            {
                Directory.CreateDirectory(Application.dataPath + "/Resources");
                Directory.CreateDirectory(Application.dataPath + "/Resources/CSV/");
                Directory.CreateDirectory(Application.dataPath + "/Resources/CSV/");
            }
            string path = Application.dataPath + "/Resources/CSV/";
            path = path.Substring(0, path.LastIndexOf('/'));
            return Path.Combine(path, filename);
        }
    }
}
#endif