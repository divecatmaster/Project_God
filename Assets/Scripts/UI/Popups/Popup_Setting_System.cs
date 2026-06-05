using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Popup_Setting_System : MonoBehaviour
{
    [Header("ScreenMode")]
    [SerializeField] TMP_Dropdown ScreenMode_Dropdown;
    [SerializeField] RectTransform ScreenMode_Arrow;

    private void Awake() 
    {
        ScreenMode_Dropdown.onValueChanged.AddListener(OnValueChange_ScreenMode);
    }

    void OnEnable()
    {
        SetScreenMode();
    }

    void Update()
    {
        if (ScreenMode_Dropdown.template.gameObject.activeSelf)
        {
            ScreenMode_Arrow.localRotation = Quaternion.Euler(0, 0, 180);
        }
        else
        {
            ScreenMode_Arrow.localRotation = Quaternion.identity;
        }
    }

    void SetScreenMode()
    {
        ScreenMode_Dropdown.ClearOptions();

        ScreenMode_Dropdown.AddOptions(new List<string>()
        {
            LanguageManager.Instance.GetText("ScreenMode_1"),
            LanguageManager.Instance.GetText("ScreenMode_2"),
            LanguageManager.Instance.GetText("ScreenMode_3")
        });

        ScreenMode_Dropdown.SetValueWithoutNotify(Data_Manager.Instance.ScreenMode);
        ScreenMode_Dropdown.RefreshShownValue();
    }

    void OnValueChange_ScreenMode(int index)
    {
        if (Data_Manager.Instance.ScreenMode == index)
            return;

        Data_Manager.Instance.SetScreenMode(index);
    }
}