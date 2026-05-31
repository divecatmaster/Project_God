using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DiveCat.God.UI.Popups
{
    public class PopupManager : MonoBehaviour
    {
        private static PopupManager _instance;
        public static PopupManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Object.FindFirstObjectByType<PopupManager>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("PopupManager");
                        _instance = go.AddComponent<PopupManager>();
                    }
                }
                return _instance;
            }
        }

        [Header("Settings")]
        [SerializeField] private bool closeTopOnEscape = true;
        
        [Header("Background Dim")]
        [SerializeField] private Image backgroundDimImage;
        [SerializeField] private float dimFadeDuration = 0.2f;
        [SerializeField] private float targetDimAlpha = 0.6f;

        private readonly List<PopupBase> _popupStack = new List<PopupBase>();
        private bool _isEscPressedThisFrame;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            
            if (backgroundDimImage != null)
            {
                backgroundDimImage.gameObject.SetActive(false);
                backgroundDimImage.raycastTarget = true; // Block raycasts when dim is active
            }
        }

        private void Update()
        {
            _isEscPressedThisFrame = false;

            if (closeTopOnEscape && Input.GetKeyDown(KeyCode.Escape))
            {
                CloseTopmostPopup();
                _isEscPressedThisFrame = true;
            }
        }

        public void RegisterOpenedPopup(PopupBase popup)
        {
            if (!_popupStack.Contains(popup))
            {
                _popupStack.Add(popup);
                UpdateBackgroundDim();
            }
        }

        public void UnregisterClosedPopup(PopupBase popup)
        {
            if (_popupStack.Contains(popup))
            {
                _popupStack.Remove(popup);
                UpdateBackgroundDim();
            }
        }

        public void CloseTopmostPopup()
        {
            if (_popupStack.Count == 0) return;

            PopupBase top = _popupStack[_popupStack.Count - 1];

            if (top.State == PopupState.Opened)
            {
                top.CloseByEscape();
            }
        }

        public void CloseAllPopups()
        {
            // Iterate backwards to avoid collection modification issues while closing
            for (int i = _popupStack.Count - 1; i >= 0; i--)
            {
                _popupStack[i].Close();
            }
        }

        public bool IsAnyPopupOpen()
        {
            return _popupStack.Count > 0;
        }

        private async void UpdateBackgroundDim()
        {
            if (backgroundDimImage == null) return;

            bool shouldBeActive = _popupStack.Count > 0;
            
            if (shouldBeActive && !backgroundDimImage.gameObject.activeSelf)
            {
                backgroundDimImage.gameObject.SetActive(true);
                backgroundDimImage.transform.SetAsFirstSibling(); // Ensure it's behind popups
                await AnimateDim(0, targetDimAlpha);
            }
            else if (!shouldBeActive && backgroundDimImage.gameObject.activeSelf)
            {
                await AnimateDim(backgroundDimImage.color.a, 0);
                backgroundDimImage.gameObject.SetActive(false);
            }
        }

        private async Awaitable AnimateDim(float start, float end)
        {
            float elapsed = 0;
            Color color = backgroundDimImage.color;
            
            while (elapsed < dimFadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / dimFadeDuration);
                color.a = Mathf.Lerp(start, end, t);
                backgroundDimImage.color = color;
                await Awaitable.NextFrameAsync();
            }
            
            color.a = end;
            backgroundDimImage.color = color;
        }
    }
}
