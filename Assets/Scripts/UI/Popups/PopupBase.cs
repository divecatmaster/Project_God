using System;
using God.Audio;
using UnityEngine;
using UnityEngine.UI;

namespace DiveCat.God.UI.Popups
{
    [RequireComponent(typeof(CanvasGroup))]
    public abstract class PopupBase : MonoBehaviour, IPopup
    {
        [Header("Animation Settings")]
        [SerializeField] protected Popup_Animation_Type AnimationType = Popup_Animation_Type.Default;
        [SerializeField] protected float fadeDuration = 0.25f;
        [SerializeField] protected bool useScaleAnimation = true;
        [SerializeField] protected float scaleDuration = 0.25f;
        [SerializeField] protected AnimationCurve animationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        [SerializeField] protected Vector3 startScale = new Vector3(0.8f, 0.8f, 0.8f);

        [Header("SlideMask")]
        [SerializeField] protected RectTransform revealMaskRoot;
        [SerializeField] protected RectMask2D SlideMask;
        [SerializeField] protected float revealDuration = 0.25f;
        private float _revealOriginWidth;
        private Vector4 _revealOriginPadding;

        [Header("Components")]
        [SerializeField] protected CanvasGroup canvasGroup;
        [SerializeField] protected RectTransform contentRoot;

        public PopupState State { get; protected set; } = PopupState.Closed;

        private int _animationVersion;
        private bool _isDestroyed;

        protected virtual void Awake()
        {
            if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
            if (contentRoot == null) contentRoot = transform as RectTransform;

            if (revealMaskRoot != null)
            {
                Canvas.ForceUpdateCanvases();
                _revealOriginWidth = revealMaskRoot.rect.width;
            }

            if (SlideMask != null)
            {
                _revealOriginPadding = SlideMask.padding;
            }

            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            gameObject.SetActive(false);
        }

        protected virtual void OnDestroy()
        {
            _isDestroyed = true;
            _animationVersion++;

            if (PopupManager.Instance != null)
            {
                PopupManager.Instance.UnregisterClosedPopup(this);
            }
        }

        public virtual async void Open(Action onComplete = null)
        {
            if (_isDestroyed || this == null)
                return;

            if (State != PopupState.Closed)
                return;

            int version = ++_animationVersion;

            transform.SetAsLastSibling();
            State = PopupState.Opening;
            gameObject.SetActive(true);

            if (canvasGroup != null)
            {
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }

            if (PopupManager.Instance != null)
            {
                PopupManager.Instance.RegisterOpenedPopup(this);
            }

            await AnimateOpen(version);

            if (!IsValidAnimation(version, PopupState.Opening))
                return;

            State = PopupState.Opened;

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            }
            
            onComplete?.Invoke();
        }

        public virtual async void Close(Action onComplete = null)
        {
            if (_isDestroyed || this == null)
                return;

            if (State != PopupState.Opened && State != PopupState.Opening)
                return;

            int version = ++_animationVersion;

            State = PopupState.Closing;

            if (canvasGroup != null)
            {
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }

            await AnimateClose(version);

            if (!IsValidAnimation(version, PopupState.Closing))
                return;

            State = PopupState.Closed;

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }

            if (PopupManager.Instance != null)
            {
                PopupManager.Instance.UnregisterClosedPopup(this);
            }

            if (!_isDestroyed && this != null)
            {
                gameObject.SetActive(false);
            }

            onComplete?.Invoke();
        }

        public virtual void CloseFast(Action onComplete = null)
        {
            if (_isDestroyed || this == null)
                return;

            if (State == PopupState.Closed)
                return;

            _animationVersion++;
            State = PopupState.Closed;

            if (canvasGroup != null)
            {
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
                canvasGroup.alpha = 0f;
            }

            if (contentRoot != null)
            {
                if (AnimationType == Popup_Animation_Type.Default && useScaleAnimation)
                {
                    contentRoot.localScale = startScale;
                }
            }

            if (AnimationType == Popup_Animation_Type.Slide && SlideMask != null && revealMaskRoot != null)
            {
                float fullWidth = GetRevealWidth();

                Vector4 padding = _revealOriginPadding;
                padding.z = fullWidth;
                SlideMask.padding = padding;
            }

            if (PopupManager.Instance != null)
            {
                PopupManager.Instance.UnregisterClosedPopup(this);
            }

            if (!_isDestroyed && this != null)
            {
                gameObject.SetActive(false);
            }

            onComplete?.Invoke();
        }

        public virtual void CloseByEscape()
        {
            Close();
        }

        protected virtual async Awaitable AnimateOpen(int version)
        {
            if (AnimationType == Popup_Animation_Type.Default)
            {
                await AnimateOpenDefault(version);
            }
            else if (AnimationType == Popup_Animation_Type.Slide)
            {
                if (revealMaskRoot == null || SlideMask == null)
                {
                    await AnimateOpenDefault(version);
                }
                else
                {
                    await AnimateOpenSlide(version);
                }
            }
        }

        protected virtual async Awaitable AnimateClose(int version)
        {
            if (AnimationType == Popup_Animation_Type.Default)
            {
                await AnimateCloseDefault(version);
            }
            else if (AnimationType == Popup_Animation_Type.Slide)
            {
                if (revealMaskRoot == null || SlideMask == null)
                {
                    await AnimateCloseDefault(version);
                }
                else
                {
                    await AnimateCloseSlide(version);
                }
            }
        }

        protected virtual async Awaitable AnimateOpenDefault(int version)
        {
            float elapsed = 0f;
            Vector3 targetScale = Vector3.one;
            float duration = Mathf.Max(fadeDuration, useScaleAnimation ? scaleDuration : 0f);

            if (duration <= 0f)
            {
                if (canvasGroup != null) canvasGroup.alpha = 1f;
                if (useScaleAnimation && contentRoot != null) contentRoot.localScale = targetScale;
                return;
            }

            if (canvasGroup != null) canvasGroup.alpha = 0f;
            if (useScaleAnimation && contentRoot != null) contentRoot.localScale = startScale;

            while (elapsed < duration)
            {
                if (!IsValidAnimation(version, PopupState.Opening))
                    return;

                elapsed += Time.unscaledDeltaTime;

                if (canvasGroup != null && fadeDuration > 0f)
                {
                    float tFade = Mathf.Clamp01(elapsed / fadeDuration);
                    canvasGroup.alpha = animationCurve.Evaluate(tFade);
                }

                if (useScaleAnimation && contentRoot != null && scaleDuration > 0f)
                {
                    float tScale = Mathf.Clamp01(elapsed / scaleDuration);
                    contentRoot.localScale = Vector3.LerpUnclamped(startScale, targetScale, animationCurve.Evaluate(tScale));
                }

                await Awaitable.NextFrameAsync();
            }

            if (!IsValidAnimation(version, PopupState.Opening))
                return;

            if (canvasGroup != null) canvasGroup.alpha = 1f;
            if (useScaleAnimation && contentRoot != null) contentRoot.localScale = targetScale;
        }

        protected virtual async Awaitable AnimateCloseDefault(int version)
        {
            float elapsed = 0f;
            float duration = Mathf.Max(fadeDuration, useScaleAnimation ? scaleDuration : 0f);
            Vector3 targetScale = startScale;
            Vector3 currentScale = contentRoot != null ? contentRoot.localScale : Vector3.one;

            if (duration <= 0f)
            {
                if (canvasGroup != null) canvasGroup.alpha = 0f;
                if (useScaleAnimation && contentRoot != null) contentRoot.localScale = targetScale;
                return;
            }

            while (elapsed < duration)
            {
                if (!IsValidAnimation(version, PopupState.Closing))
                    return;

                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float evaluatedT = animationCurve.Evaluate(1f - t);

                if (canvasGroup != null)
                    canvasGroup.alpha = evaluatedT;

                if (useScaleAnimation && contentRoot != null)
                    contentRoot.localScale = Vector3.LerpUnclamped(targetScale, currentScale, evaluatedT);

                await Awaitable.NextFrameAsync();
            }

            if (!IsValidAnimation(version, PopupState.Closing))
                return;

            if (canvasGroup != null) canvasGroup.alpha = 0f;
            if (useScaleAnimation && contentRoot != null) contentRoot.localScale = targetScale;
        }

        protected virtual async Awaitable AnimateOpenSlide(int version)
        {
            Canvas.ForceUpdateCanvases();

            float elapsed = 0f;
            float fullWidth = GetRevealWidth();
            float duration = Mathf.Max(0.001f, revealDuration);

            if (canvasGroup != null)
                canvasGroup.alpha = 1f;

            Vector4 padding = _revealOriginPadding;
            padding.z = fullWidth;
            SlideMask.padding = padding;

            while (elapsed < duration)
            {
                if (!IsValidAnimation(version, PopupState.Opening))
                    return;

                elapsed += Time.unscaledDeltaTime;

                float t = Mathf.Clamp01(elapsed / duration);
                float evaluatedT = animationCurve.Evaluate(t);

                padding = _revealOriginPadding;
                padding.z = Mathf.LerpUnclamped(fullWidth, _revealOriginPadding.z, evaluatedT);
                SlideMask.padding = padding;

                await Awaitable.NextFrameAsync();
            }

            if (!IsValidAnimation(version, PopupState.Opening))
                return;

            SlideMask.padding = _revealOriginPadding;
            if (canvasGroup != null) canvasGroup.alpha = 1f;
        }

        protected virtual async Awaitable AnimateCloseSlide(int version)
        {
            Canvas.ForceUpdateCanvases();

            float elapsed = 0f;
            float fullWidth = GetRevealWidth();
            float duration = Mathf.Max(0.001f, revealDuration);

            if (canvasGroup != null)
                canvasGroup.alpha = 1f;

            Vector4 padding = _revealOriginPadding;
            SlideMask.padding = padding;

            while (elapsed < duration)
            {
                if (!IsValidAnimation(version, PopupState.Closing))
                    return;

                elapsed += Time.unscaledDeltaTime;

                float t = Mathf.Clamp01(elapsed / duration);
                float evaluatedT = animationCurve.Evaluate(t);

                padding = _revealOriginPadding;
                padding.z = Mathf.LerpUnclamped(_revealOriginPadding.z, fullWidth, evaluatedT);
                SlideMask.padding = padding;

                await Awaitable.NextFrameAsync();
            }

            if (!IsValidAnimation(version, PopupState.Closing))
                return;

            padding = _revealOriginPadding;
            padding.z = fullWidth;
            SlideMask.padding = padding;

            if (canvasGroup != null)
                canvasGroup.alpha = 0f;
        }

        private bool IsValidAnimation(int version, PopupState expectedState)
        {
            return !_isDestroyed && this != null && _animationVersion == version && State == expectedState;
        }

        private float GetRevealWidth()
        {
            float fullWidth = _revealOriginWidth;

            if (fullWidth <= 0f && revealMaskRoot != null)
            {
                fullWidth = revealMaskRoot.rect.width;
            }

            return Mathf.Max(0f, fullWidth);
        }
    }

    public enum Popup_Animation_Type
    {
        Default,
        Slide,
    }
}
