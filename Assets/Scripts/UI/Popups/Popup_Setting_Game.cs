using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

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

    [Header("Example")]
    [SerializeField] TextMeshProUGUI ExampleText;

    int _textSpeedValue;
    float _autoSpeedValue;
    int _productionValue;
    private Coroutine _typeRoutine;

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
        if (_typeRoutine != null)
        {
            StopCoroutine(_typeRoutine);
            _typeRoutine = null;
        }
        _typeRoutine = StartCoroutine(SetExample());
    }

    void OnDisable()
    {
        if (_typeRoutine != null)
        {
            StopCoroutine(_typeRoutine);
            _typeRoutine = null;
        }

        Data_Manager.Instance.SetTextSpeed(_textSpeedValue);
        Data_Manager.Instance.SetAutoSpeed(_autoSpeedValue);
        Data_Manager.Instance.SetProduction_Effect(_productionValue);
    }

    void Set_TextSpeed()
    {
        TextSpeed_Value_Change(Data_Manager.Instance.TextSpeed);
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
        AutoSpeed_Value_Change(Data_Manager.Instance.AutoSpeed);
    }

    public void AutoSpeed_Value_Change(float value)
    {
        _autoSpeedValue = Mathf.Round(value * 10f) / 10f;
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
        AutoSpeed_Amount.text = $"{_autoSpeedValue}s";
    }

    void Set_Production()
    {
        _productionValue = Data_Manager.Instance.Production_Effect;

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

    private IEnumerator SetExample()
    {
        int idx = 0;
        while (true)
        {
            ExampleText.text = LanguageManager.Instance.GetText($"Sample_Text_{idx}");
            ExampleText.maxVisibleCharacters = 0;
            ExampleText.ForceMeshUpdate();

            int totalVisibleCharacters = ExampleText.textInfo.characterCount;
            int counter = 0;

            float waitTime = Mathf.Lerp(0.12f, 0.001f, _textSpeedValue / 100f);

            if (_textSpeedValue >= 100)
            {
                ExampleText.maxVisibleCharacters = totalVisibleCharacters;
            }
            else
            {
                while (counter <= totalVisibleCharacters)
                {
                    ExampleText.maxVisibleCharacters = counter;

                    counter++;
                    yield return new WaitForSeconds(waitTime);
                }
            }

            idx++;
            if (idx >= 2)
            {
                idx = 0;
            }
            yield return new WaitForSeconds(_autoSpeedValue);
        }
    }
}
