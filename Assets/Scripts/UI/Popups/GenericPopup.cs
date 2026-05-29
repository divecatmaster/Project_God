using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace DiveCat.God.UI.Popups
{
    /// <summary>
    /// A generic popup implementation with a title, message, and buttons.
    /// </summary>
    public class GenericPopup : PopupBase
    {
        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button confirmButton;

        private Action _onConfirm;
        private Action _onCancel;

        protected override void Awake()
        {
            base.Awake();
            
            if (closeButton != null)
                closeButton.onClick.AddListener(OnCloseButtonClicked);
            
            if (confirmButton != null)
                confirmButton.onClick.AddListener(OnConfirmButtonClicked);
        }

        public void Setup(string title, string message, Action onConfirm = null, Action onCancel = null)
        {
            if (titleText != null) titleText.text = title;
            if (messageText != null) messageText.text = message;
            
            _onConfirm = onConfirm;
            _onCancel = onCancel;
        }

        private void OnCloseButtonClicked()
        {
            _onCancel?.Invoke();
            Close();
        }

        private void OnConfirmButtonClicked()
        {
            _onConfirm?.Invoke();
            Close();
        }
    }
}
