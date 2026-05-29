using System;
using UnityEngine;
using UnityEngine.UI;

public class Popup_Save_Group : MonoBehaviour
{
    [SerializeField] GameObject Selected;
    [SerializeField] GameObject Unselected;
    [SerializeField] Button Btn;

    int _btnIdx;
    Action<int> _callback;
    private void Awake() 
    {
        Btn.onClick.AddListener(OnClickBtn);
    }

    public void SetButton(int idx, Action<int> callback)
    {
        _btnIdx = idx;
        _callback = callback;
        SetSelected(false);
    }

    public void SetSelected(bool isSelected)
    {
        Selected.SetActive(isSelected);
        Unselected.SetActive(!isSelected);
    }

    void OnClickBtn()
    {
        _callback?.Invoke(_btnIdx);
    }
}
