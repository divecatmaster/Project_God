using UnityEngine;
using TMPro;

namespace UI.TextEffects
{
    /// <summary>
    /// Creates an electronic display/marquee scrolling text effect from right to left.
    /// If the text is short enough to fit inside the viewport, it does not scroll.
    /// </summary>
    [RequireComponent(typeof(TMP_Text))]
    public class TMPEffect_Marquee : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("The viewport container (usually a mask/RectMask2D) that defines the visible area. Defaults to parent if null.")]
        [SerializeField] private RectTransform viewport;

        [Header("Scroll Settings")]
        [Tooltip("Scrolling speed in pixels per second.")]
        [SerializeField] private float scrollSpeed = 100f;
        
        [Tooltip("Delay in seconds before starting to scroll from the right.")]
        [SerializeField] private float startDelay = 1f;
        
        [Tooltip("Delay in seconds after scrolling completely out of view before resetting.")]
        [SerializeField] private float endDelay = 1f;

        private TMP_Text _textComponent;
        private RectTransform _textRectTransform;
        private float _viewportWidth;
        private float _textWidth;
        private bool _isScrolling;
        private float _currentPositionX;
        private float _delayTimer;
        private string _lastText;

        private enum ScrollState
        {
            WaitingToStart,
            Scrolling,
            WaitingToEnd
        }
        private ScrollState _state;

        private void Awake()
        {
            _textComponent = GetComponent<TMP_Text>();
            _textRectTransform = GetComponent<RectTransform>();
            
            if (viewport == null)
            {
                viewport = transform.parent as RectTransform;
            }
        }

        private void Start()
        {
            InitializeMarquee();
        }

        private void OnEnable()
        {
            InitializeMarquee();
        }

        private void Update()
        {
            // If the text string changed, reinitialize measurements and state
            if (_textComponent.text != _lastText)
            {
                InitializeMarquee();
            }

            if (!_isScrolling) return;

            switch (_state)
            {
                case ScrollState.WaitingToStart:
                    _delayTimer -= Time.deltaTime;
                    if (_delayTimer <= 0)
                    {
                        _state = ScrollState.Scrolling;
                    }
                    break;

                case ScrollState.Scrolling:
                    _currentPositionX -= scrollSpeed * Time.deltaTime;
                    
                    // If the entire text has scrolled past the left edge
                    if (_currentPositionX < -_textWidth)
                    {
                        _delayTimer = endDelay;
                        _state = ScrollState.WaitingToEnd;
                    }
                    else
                    {
                        SetPosition(_currentPositionX);
                    }
                    break;

                case ScrollState.WaitingToEnd:
                    _delayTimer -= Time.deltaTime;
                    if (_delayTimer <= 0)
                    {
                        ResetPosition();
                        _delayTimer = startDelay;
                        _state = ScrollState.WaitingToStart;
                    }
                    break;
            }
        }

        public void InitializeMarquee()
        {
            if (_textComponent == null || _textRectTransform == null || viewport == null) return;

            _lastText = _textComponent.text;

            // Enforce single line wrapping settings for accurate scroll behavior
            _textComponent.textWrappingMode = TextWrappingModes.NoWrap;

            // Force TMPro to update mesh geometry so preferredWidth is correctly populated
            _textComponent.ForceMeshUpdate();
            _textWidth = _textComponent.preferredWidth;
            _viewportWidth = viewport.rect.width;

            // Anchors must be anchored to Left-Middle with a Left-Middle pivot (0, 0.5)
            // This simplifies position tracking (X = 0 is left aligned, X = viewportWidth is right aligned)
            _textRectTransform.anchorMin = new Vector2(0, 0.5f);
            _textRectTransform.anchorMax = new Vector2(0, 0.5f);
            _textRectTransform.pivot = new Vector2(0, 0.5f);

            if (_textWidth <= _viewportWidth)
            {
                // Short text: No marquee scroll needed
                _isScrolling = false;
                SetPosition(0); // Left align in the viewport
            }
            else
            {
                // Long text: Play scrolling marquee effect
                _isScrolling = true;
                ResetPosition();
                _delayTimer = startDelay;
                _state = ScrollState.WaitingToStart;
            }
        }

        private void ResetPosition()
        {
            // Start scrolling from the right edge of the viewport
            _currentPositionX = _viewportWidth;
            SetPosition(_currentPositionX);
        }

        private void SetPosition(float x)
        {
            Vector2 anchoredPos = _textRectTransform.anchoredPosition;
            anchoredPos.x = x;
            anchoredPos.y = 0; // Vertically center aligned relative to (0, 0.5) anchors
            _textRectTransform.anchoredPosition = anchoredPos;
        }

        /// <summary>
        /// Public method to set text and immediately initialize marquee
        /// </summary>
        public void SetText(string text)
        {
            if (_textComponent != null)
            {
                _textComponent.text = text;
                InitializeMarquee();
            }
        }
    }
}
