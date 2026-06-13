using UnityEngine;
using TMPro;

namespace UI.TextEffects
{
    /// <summary>
    /// Controller for the Marquee demo scene that allows users to type in custom text
    /// and see how the marquee dynamically decides whether to scroll or remain static.
    /// </summary>
    public class MarqueeDemoController : MonoBehaviour
    {
        [Header("References")]
        public TMPEffect_Marquee marqueeText;
        public TMP_InputField inputField;
        public UnityEngine.UI.Button applyButton;

        private void Start()
        {
            if (applyButton != null && inputField != null && marqueeText != null)
            {
                applyButton.onClick.AddListener(OnApplyClicked);
                // Trigger initial marquee setup with input field's text
                OnApplyClicked();
            }
        }

        private void OnApplyClicked()
        {
            if (marqueeText != null && inputField != null)
            {
                marqueeText.SetText(inputField.text);
            }
        }
    }
}

