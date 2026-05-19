using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace God.UI
{
    /// <summary>
    /// A production-ready component that fits a UI Image within its parent RectTransform
    /// while preserving aspect ratio. Supports multiple fit modes including Cover and Fit Inside.
    /// Optimized for mobile and performance (no Update usage).
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(RectTransform))]
    [AddComponentMenu("UI/Image Aspect Fitter (Enhanced)")]
    public class UIImageAspectFitter : UIBehaviour, ILayoutSelfController
    {
        public enum FitMode
        {
            None,
            FitWidth,
            FitHeight,
            FitInside,
            Cover
        }

        [Header("Settings")]
        [SerializeField] private FitMode m_FitMode = FitMode.FitInside;
        [SerializeField] private UnityEngine.UI.Image m_Image;
        
        [Header("Padding")]
        [SerializeField] private bool m_UsePadding = false;
        [SerializeField] private Vector4 m_Padding = Vector4.zero; // x:Left, y:Bottom, z:Right, w:Top

        [Header("Advanced")]
        [SerializeField] private bool m_ApplyOnStart = true;
        [SerializeField] private bool m_AutoCenter = true;

        private RectTransform m_Rect;
        private RectTransform m_Parent;

        protected override void Awake()
        {
            base.Awake();
            m_Rect = GetComponent<RectTransform>();
            if (m_Image == null) m_Image = GetComponent<UnityEngine.UI.Image>();
        }

        protected override void Start()
        {
            base.Start();
            if (m_ApplyOnStart) Refresh();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            Refresh();
        }

        protected override void OnTransformParentChanged()
        {
            base.OnTransformParentChanged();
            m_Parent = transform.parent as RectTransform;
            Refresh();
        }

        protected override void OnRectTransformDimensionsChange()
        {
            base.OnRectTransformDimensionsChange();
            Refresh();
        }

        /// <summary>
        /// Manually triggers the resizing logic. Call this if the sprite changes dynamically.
        /// </summary>
        public void Refresh()
        {
            if (m_Rect == null || m_Image == null) return;
            
            Sprite sprite = m_Image.sprite;
            if (sprite == null) return;

            m_Parent = transform.parent as RectTransform;
            if (m_Parent == null) return;

            // 1. Calculate Sprite Aspect Ratio
            float spriteWidth = sprite.rect.width;
            float spriteHeight = sprite.rect.height;
            if (spriteWidth <= 0 || spriteHeight <= 0) return;
            float spriteAspect = spriteWidth / spriteHeight;

            // 2. Get Parent Dimensions
            Rect parentRect = m_Parent.rect;
            if (m_UsePadding)
            {
                parentRect.xMin += m_Padding.x; // Left
                parentRect.yMin += m_Padding.y; // Bottom
                parentRect.xMax -= m_Padding.z; // Right
                parentRect.yMax -= m_Padding.w; // Top
            }

            float parentWidth = parentRect.width;
            float parentHeight = parentRect.height;

            if (parentWidth <= 0 || parentHeight <= 0) return;

            float parentAspect = parentWidth / parentHeight;

            // 3. Calculate Target Size
            float targetWidth = parentWidth;
            float targetHeight = parentHeight;

            switch (m_FitMode)
            {
                case FitMode.FitWidth:
                    targetHeight = parentWidth / spriteAspect;
                    break;
                case FitMode.FitHeight:
                    targetWidth = parentHeight * spriteAspect;
                    break;
                case FitMode.FitInside:
                    if (spriteAspect > parentAspect)
                        targetHeight = parentWidth / spriteAspect;
                    else
                        targetWidth = parentHeight * spriteAspect;
                    break;
                case FitMode.Cover:
                    if (spriteAspect > parentAspect)
                        targetWidth = parentHeight * spriteAspect;
                    else
                        targetHeight = parentWidth / spriteAspect;
                    break;
                case FitMode.None:
                    return;
            }

            // 4. Apply to RectTransform
            // We use anchors (0.5, 0.5) to make sizing and centering stable.
            m_Rect.anchorMin = new Vector2(0.5f, 0.5f);
            m_Rect.anchorMax = new Vector2(0.5f, 0.5f);
            m_Rect.sizeDelta = new Vector2(targetWidth, targetHeight);

            if (m_AutoCenter)
            {
                // Position relative to parent center, considering padding
                m_Rect.anchoredPosition = parentRect.center;
            }
        }

        // ILayoutSelfController implementation
        public void SetLayoutHorizontal() => Refresh();
        public void SetLayoutVertical() => Refresh();

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            // Ensure references are valid for editor preview
            if (m_Rect == null) m_Rect = GetComponent<RectTransform>();
            if (m_Image == null) m_Image = GetComponent<UnityEngine.UI.Image>();
            Refresh();
        }
#endif
    }
}
