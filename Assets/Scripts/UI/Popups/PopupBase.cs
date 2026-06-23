using System;
using UnityEngine;
using System.Threading.Tasks;
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

            canvasGroup.alpha = 0;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            gameObject.SetActive(false);
        }

        public virtual async void Open(Action onComplete = null)
        {
            if (State != PopupState.Closed) return;
            transform.SetAsLastSibling();
            State = PopupState.Opening;
            gameObject.SetActive(true);
            
            PopupManager.Instance.RegisterOpenedPopup(this);

            await AnimateOpen();

            State = PopupState.Opened;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
            
            onComplete?.Invoke();
        }

        public virtual async void Close(Action onComplete = null)
        {
            if (State != PopupState.Opened) return;

            State = PopupState.Closing;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;

            await AnimateClose();

            State = PopupState.Closed;
            gameObject.SetActive(false);
            
            PopupManager.Instance.UnregisterClosedPopup(this);
            
            onComplete?.Invoke();
        }

        public virtual async void CloseFast(Action onComplete = null)
        {
            if (State == PopupState.Closed)
                return;

            State = PopupState.Closed;

            if (canvasGroup != null)
            {
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
                canvasGroup.alpha = 0f;
            }

            if (useScaleAnimation)
            {
                if (contentRoot != null)
                {
                    contentRoot.localScale = startScale;
                }
            }

            if (AnimationType == Popup_Animation_Type.Slide && SlideMask != null && revealMaskRoot != null)
            {
                float fullWidth = _revealOriginWidth;

                if (fullWidth <= 0f)
                    fullWidth = revealMaskRoot.rect.width;

                Vector4 padding = _revealOriginPadding;
                padding.z = fullWidth;
                SlideMask.padding = padding;
            }

            gameObject.SetActive(false);

            PopupManager.Instance.UnregisterClosedPopup(this);

            onComplete?.Invoke();
        }

        public virtual void CloseByEscape()
        {
            Close();
        }

        protected virtual async Awaitable AnimateOpen()
        {
            if (AnimationType == Popup_Animation_Type.Default)
            {
                float elapsed = 0;
                Vector3 targetScale = Vector3.one;

                while (elapsed < fadeDuration || (useScaleAnimation && elapsed < scaleDuration))
                {
                    elapsed += Time.unscaledDeltaTime;
                    float tFade = Mathf.Clamp01(elapsed / fadeDuration);
                    float tScale = Mathf.Clamp01(elapsed / scaleDuration);

                    canvasGroup.alpha = animationCurve.Evaluate(tFade);

                    if (useScaleAnimation)
                    {
                        contentRoot.localScale = Vector3.LerpUnclamped(startScale, targetScale, animationCurve.Evaluate(tScale));
                    }

                    await Awaitable.NextFrameAsync();
                }

                canvasGroup.alpha = 1;
                if (useScaleAnimation) contentRoot.localScale = targetScale;
            }
            else if (AnimationType == Popup_Animation_Type.Slide)
            {
                if (revealMaskRoot == null || SlideMask == null)
                {
                    float elapsed = 0;
                    Vector3 targetScale = Vector3.one;

                    while (elapsed < fadeDuration || (useScaleAnimation && elapsed < scaleDuration))
                    {
                        elapsed += Time.unscaledDeltaTime;
                        float tFade = Mathf.Clamp01(elapsed / fadeDuration);
                        float tScale = Mathf.Clamp01(elapsed / scaleDuration);

                        canvasGroup.alpha = animationCurve.Evaluate(tFade);

                        if (useScaleAnimation)
                        {
                            contentRoot.localScale = Vector3.LerpUnclamped(startScale, targetScale, animationCurve.Evaluate(tScale));
                        }

                        await Awaitable.NextFrameAsync();
                    }

                    canvasGroup.alpha = 1;
                    if (useScaleAnimation) contentRoot.localScale = targetScale;
                }
                else
                {
                    Canvas.ForceUpdateCanvases();

                    float elapsed = 0f;
                    float fullWidth = _revealOriginWidth;

                    if (fullWidth <= 0f)
                        fullWidth = revealMaskRoot.rect.width;

                    canvasGroup.alpha = 1f;

                    Vector4 padding = _revealOriginPadding;

                    // 처음에는 오른쪽 패딩을 전체 너비만큼 줘서 거의 안 보이게
                    padding.z = fullWidth;
                    SlideMask.padding = padding;

                    while (elapsed < revealDuration)
                    {
                        elapsed += Time.unscaledDeltaTime;

                        float t = Mathf.Clamp01(elapsed / revealDuration);
                        float evaluatedT = animationCurve.Evaluate(t);

                        padding = _revealOriginPadding;

                        // right padding: fullWidth -> 원래 padding.z
                        padding.z = Mathf.LerpUnclamped(fullWidth, _revealOriginPadding.z, evaluatedT);

                        SlideMask.padding = padding;

                        await Awaitable.NextFrameAsync();
                    }

                    SlideMask.padding = _revealOriginPadding;
                    canvasGroup.alpha = 1f;
                }
            }
        }

        protected virtual async Awaitable AnimateClose()
        {
            if (AnimationType == Popup_Animation_Type.Default)
            {
                float elapsed = 0;
                float duration = Mathf.Max(fadeDuration, useScaleAnimation ? scaleDuration : 0);
                Vector3 targetScale = startScale;
                Vector3 currentScale = contentRoot.localScale;

                while (elapsed < duration)
                {
                    elapsed += Time.unscaledDeltaTime;
                    float t = Mathf.Clamp01(elapsed / duration);
                    float evaluatedT = animationCurve.Evaluate(1 - t);

                    canvasGroup.alpha = evaluatedT;

                    if (useScaleAnimation)
                    {
                        contentRoot.localScale = Vector3.LerpUnclamped(targetScale, currentScale, evaluatedT);
                    }

                    await Awaitable.NextFrameAsync();
                }

                canvasGroup.alpha = 0;
                if (useScaleAnimation) contentRoot.localScale = targetScale;
            }
            else if (AnimationType == Popup_Animation_Type.Slide)
            {
                if (revealMaskRoot == null || SlideMask == null)
                {
                    float elapsed = 0;
                    float duration = Mathf.Max(fadeDuration, useScaleAnimation ? scaleDuration : 0);
                    Vector3 targetScale = startScale;
                    Vector3 currentScale = contentRoot.localScale;

                    while (elapsed < duration)
                    {
                        elapsed += Time.unscaledDeltaTime;
                        float t = Mathf.Clamp01(elapsed / duration);
                        float evaluatedT = animationCurve.Evaluate(1 - t);

                        canvasGroup.alpha = evaluatedT;

                        if (useScaleAnimation)
                        {
                            contentRoot.localScale = Vector3.LerpUnclamped(targetScale, currentScale, evaluatedT);
                        }

                        await Awaitable.NextFrameAsync();
                    }

                    canvasGroup.alpha = 0;
                    if (useScaleAnimation) contentRoot.localScale = targetScale;
                }
                else
                {
                    Canvas.ForceUpdateCanvases();

                    float elapsed = 0f;
                    float fullWidth = _revealOriginWidth;

                    if (fullWidth <= 0f)
                        fullWidth = revealMaskRoot.rect.width;

                    canvasGroup.alpha = 1f;

                    Vector4 padding = _revealOriginPadding;
                    SlideMask.padding = padding;

                    while (elapsed < revealDuration)
                    {
                        elapsed += Time.unscaledDeltaTime;

                        float t = Mathf.Clamp01(elapsed / revealDuration);
                        float evaluatedT = animationCurve.Evaluate(t);

                        padding = _revealOriginPadding;

                        // right padding: 원래 padding.z -> fullWidth
                        padding.z = Mathf.LerpUnclamped(_revealOriginPadding.z, fullWidth, evaluatedT);

                        SlideMask.padding = padding;

                        await Awaitable.NextFrameAsync();
                    }

                    padding = _revealOriginPadding;
                    padding.z = fullWidth;
                    SlideMask.padding = padding;

                    canvasGroup.alpha = 0f;
                }
            }
        }
    }

    public enum Popup_Animation_Type
    {
        Default,
        Slide,
    }
}
