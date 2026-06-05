using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Popup_Setting_Sound : MonoBehaviour
{
    [Header("BackGround")]
    [SerializeField] Slider Background_Slider;
    [SerializeField] Image Background_Star;
    [SerializeField] GameObject Background_StarGlow;
    [SerializeField] TextMeshProUGUI Background_Amount;

    [Header("Effect")]
    [SerializeField] Slider Effect_Slider;
    [SerializeField] Image Effect_Star;
    [SerializeField] GameObject Effect_StarGlow;
    [SerializeField] TextMeshProUGUI Effect_Amount;

    [Header("UI")]
    [SerializeField] Slider UI_Slider;
    [SerializeField] Image UI_Star;
    [SerializeField] GameObject UI_StarGlow;
    [SerializeField] TextMeshProUGUI UI_Amount;

    int _backgroundValue;
    int _effectValue;
    int _uiValue;

    private void Awake() 
    {
        Background_Slider.onValueChanged.AddListener(Background_Value_Change);
        Effect_Slider.onValueChanged.AddListener(Effect_Value_Change);
        UI_Slider.onValueChanged.AddListener(UI_Value_Change);
    }

    private void OnEnable() 
    {
        Set_Background();
        Set_Effect();
        Set_UI();
    }

    void OnDisable()
    {
        Data_Manager.Instance.SetSound_BG(_backgroundValue);
        Data_Manager.Instance.SetSound_Effect(_effectValue);
        Data_Manager.Instance.SetSound_UI(_uiValue);
    }

    void Set_Background()
    {
        Background_Value_Change(Data_Manager.Instance.Sound_BG);
    }

    public void Background_Value_Change(float value)
    {
        _backgroundValue = Mathf.RoundToInt(value);
        Background_Slider.value = _backgroundValue;
        if (_backgroundValue > 0)
        {
            Background_Star.color = UIUtility.Slider_On_Star_Color;
            Background_StarGlow.SetActive(true);
        }
        else
        {
            Background_Star.color = UIUtility.Slider_Off_Star_Color;
            Background_StarGlow.SetActive(false);
        }
        Background_Amount.text = _backgroundValue.ToString();
    }

    void Set_Effect()
    {
        Effect_Value_Change(Data_Manager.Instance.Sound_Effect);
    }

    public void Effect_Value_Change(float value)
    {
        _effectValue = Mathf.RoundToInt(value);
        Effect_Slider.value = _effectValue;
        if (_effectValue > 0)
        {
            Effect_Star.color = UIUtility.Slider_On_Star_Color;
            Effect_StarGlow.SetActive(true);
        }
        else
        {
            Effect_Star.color = UIUtility.Slider_Off_Star_Color;
            Effect_StarGlow.SetActive(false);
        }
        Effect_Amount.text = _effectValue.ToString();
    }

    void Set_UI()
    {
        UI_Value_Change(Data_Manager.Instance.Sound_UI);
    }

    public void UI_Value_Change(float value)
    {
        _uiValue = Mathf.RoundToInt(value);
        UI_Slider.value = _uiValue;
        if (_uiValue > 0)
        {
            UI_Star.color = UIUtility.Slider_On_Star_Color;
            UI_StarGlow.SetActive(true);
        }
        else
        {
            UI_Star.color = UIUtility.Slider_Off_Star_Color;
            UI_StarGlow.SetActive(false);
        }
        UI_Amount.text = _uiValue.ToString();
    }
}
