using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Resource_Manager : MonoBehaviour
{
    public static Resource_Manager Instance;
    [SerializeField] Transform Common_Canvas;
    [SerializeField] GameObject Yes_Or_No_Obj;

    [SerializeField] Texture2D CursorTexture;
    [SerializeField] Vector2 hotSpot = Vector2.zero;

    Popup_YesOrNo _YesOrNo;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        Cursor.SetCursor(CursorTexture, hotSpot, CursorMode.Auto);
    }

    public Popup_YesOrNo Get_Yes_Or_No()
    {
        if (_YesOrNo == null)
        {
            var item = Instantiate(Yes_Or_No_Obj, Common_Canvas);
            _YesOrNo = item.GetComponent<Popup_YesOrNo>();
        }

        return _YesOrNo;
    }

    Dictionary<string, Sprite> _body_dic = new Dictionary<string, Sprite>();
    Dictionary<string, Sprite> _face_dic = new Dictionary<string, Sprite>();
    Dictionary<int, Sprite> _bg_dic = new Dictionary<int, Sprite>();

    public Sprite Get_Body_Image(string resName)
    {
        if (_body_dic.ContainsKey(resName))
        {
            return _body_dic[resName];
        }
        else
        {
            var res = Resources.Load<Sprite>($"Character/{resName}");
            if (res != null)
            {
                _body_dic.Add(resName, res);
                return res;
            }
            else
            {
                return null;
            }
        }
    }

    public Sprite Get_Face_Image(string resName)
    {
        if (_face_dic.ContainsKey(resName))
        {
            return _face_dic[resName];
        }
        else
        {
            var res = Resources.Load<Sprite>($"Character/{resName}");
            if (res != null)
            {
                _face_dic.Add(resName, res);
                return res;
            }
            else
            {
                return null;
            }
        }
    }

    public Sprite Get_BG(int index)
    {
        if (_bg_dic.ContainsKey(index))
        {
            return _bg_dic[index];
        }
        else
        {
            var res = Resources.Load<Sprite>($"BG/Background_{index}");
            if (res != null)
            {
                _bg_dic.Add(index, res);
                return res;
            }
            else
            {
                return null;
            }
        }
    }
}
