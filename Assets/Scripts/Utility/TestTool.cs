using System;
using DiveCat.God.UI.Popups;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TestTool : PopupBase
{
    [SerializeField] Button ConfirmBtn;
    [SerializeField] TMP_InputField InputField;

    protected override void Awake()
    {
        ConfirmBtn.onClick.AddListener(OnClickConfirm);
        base.Awake();
    }

    public override void Open(Action onComplete = null)
    {
        InputField.text = "";
        base.Open(onComplete);
    }

    void OnClickConfirm()
    {
        var value = InputField.text;
        if (value == "") return;

        var result = UIUtility.StringToInt(value);
        
        var data = Data_Manager.Instance.GetStoryData(result);
        if (data != null)
        {
            StoryManager.Instance.SetTest(data);
            Close();
        }
    }
}
