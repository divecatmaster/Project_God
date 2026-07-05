using God.Audio;
using UnityEngine;
using UnityEngine.UI;

public class ClickSound : MonoBehaviour
{
    [SerializeField] SoundCategory Category;
    [SerializeField] string SoundName;
    Button Btn;

    private void Awake()
    {
        Btn = GetComponent<Button>();
        if (Btn != null)
        {
            Btn.onClick.AddListener(OnClickBtn);
        }
    }

    void OnClickBtn()
    {
        if (string.IsNullOrEmpty(SoundName))
        {
            return;
        }

        switch (Category)
        {
            case SoundCategory.SFX:
                {
                    SoundManager.Instance.PlaySFX(SoundName);
                }
                break;
            case SoundCategory.UI:
                {
                    SoundManager.Instance.PlayUI(SoundName);
                }
                break;
        }
    }
}
