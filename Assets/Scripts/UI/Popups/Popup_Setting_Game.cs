using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Popup_Setting_Game : MonoBehaviour
{
    [Header("TextSpeed")]
    [SerializeField] Slider TextSpeed_Slider;
    [SerializeField] Image TextSpeed_Star;
    [SerializeField] GameObject TextSpeed_StarGlow;
    [SerializeField] TextMeshProUGUI TextSpeed_Amount;

    [Header("AutoSpeed")]
    [SerializeField] Slider AutoSpeed_Slider;
    [SerializeField] Image AutoSpeed_Star;
    [SerializeField] GameObject AutoSpeed_StarGlow;
    [SerializeField] TextMeshProUGUI AutoSpeed_Amount;

    [Header("Production_Effect")]
    [SerializeField] Button Production_Btn;
    [SerializeField] GameObject Production_On;
    [SerializeField] GameObject Production_Off;

    int _textSpeedValue;
    int _autoSpeedValue;
    int _productionValue;

    private void Awake() 
    {
        TextSpeed_Slider.onValueChanged.AddListener(TextSpeed_Value_Change);
        AutoSpeed_Slider.onValueChanged.AddListener(AutoSpeed_Value_Change);
        Production_Btn.onClick.AddListener(OnClickProduction);
    }

    private void OnEnable() 
    {
        Set_TextSpeed();
        Set_AutoSpeed();
        Set_Production();
    }

    void OnDisable()
    {
        PlayerPrefs.SetInt("TextSpeed", _textSpeedValue);
        PlayerPrefs.SetInt("AutoSpeed", _autoSpeedValue);
        PlayerPrefs.SetInt("Production_Effect", _productionValue);
    }

    void Set_TextSpeed()
    {
        TextSpeed_Value_Change(PlayerPrefs.GetInt("TextSpeed", 100));
    }

    public void TextSpeed_Value_Change(float value)
    {
        _textSpeedValue = Mathf.RoundToInt(value);
        TextSpeed_Slider.value = _textSpeedValue;
        if (_textSpeedValue > 0)
        {
            TextSpeed_Star.color = UIUtility.Slider_On_Star_Color;
            TextSpeed_StarGlow.SetActive(true);
        }
        else
        {
            TextSpeed_Star.color = UIUtility.Slider_Off_Star_Color;
            TextSpeed_StarGlow.SetActive(false);
        }
        TextSpeed_Amount.text = _textSpeedValue.ToString();
    }

    void Set_AutoSpeed()
    {
        AutoSpeed_Value_Change(PlayerPrefs.GetInt("AutoSpeed", 100));
    }

    public void AutoSpeed_Value_Change(float value)
    {
        _autoSpeedValue = Mathf.RoundToInt(value);
        AutoSpeed_Slider.value = _autoSpeedValue;
        if (_autoSpeedValue > 0)
        {
            AutoSpeed_Star.color = UIUtility.Slider_On_Star_Color;
            AutoSpeed_StarGlow.SetActive(true);
        }
        else
        {
            AutoSpeed_Star.color = UIUtility.Slider_Off_Star_Color;
            AutoSpeed_StarGlow.SetActive(false);
        }
        AutoSpeed_Amount.text = _autoSpeedValue.ToString();
    }

    void Set_Production()
    {
        _productionValue = PlayerPrefs.GetInt("Production_Effect", 1);

        if (_productionValue == 1)
        {
            Production_On.SetActive(true);
            Production_Off.SetActive(false);
        }
        else
        {
            Production_On.SetActive(false);
            Production_Off.SetActive(true);
        }
    }

    void OnClickProduction()
    {
        if (_productionValue == 1)
        {
            _productionValue = 0;
        }
        else
        {
            _productionValue = 1;
        }

        if (_productionValue == 1)
        {
            Production_On.SetActive(true);
            Production_Off.SetActive(false);
        }
        else
        {
            Production_On.SetActive(false);
            Production_Off.SetActive(true);
        }
    }
}
