using UnityEngine;
using UnityEngine.UI;

namespace God.UI
{
    /// <summary>
    /// A production-ready UI fog effect that scrolls a texture horizontally.
    /// Optimized for mobile and easy to configure from the Inspector.
    /// </summary>
    [RequireComponent(typeof(RawImage))]
    [AddComponentMenu("UI/Effects/Fog Effect")]
    public class UIFogEffect : MonoBehaviour
    {
        [Header("Appearance")]
        [SerializeField] private Color fogColor = new Color(1f, 1f, 1f, 0.3f);
        [SerializeField, Tooltip("Vertical height of the fog layer relative to screen/parent.")]
        private float heightScale = 0.2f;

        [Header("Movement")]
        [SerializeField] private float scrollSpeed = 0.05f;
        [SerializeField] private Vector2 scrollDirection = Vector2.right;

        [Header("Optimization")]
        [SerializeField] private bool playOnAwake = true;
        [SerializeField, Tooltip("Set to false to stop UV updates.")]
        private bool isPaused = false;

        private RawImage _rawImage;
        private Rect _uvRect;

        private void Awake()
        {
            _rawImage = GetComponent<RawImage>();
            _rawImage.raycastTarget = false; // Production best practice: non-interactive by default
            _rawImage.color = fogColor;
            
            // Ensure texture is set to Repeat in your assets, but we'll remind the user
            if (_rawImage.texture != null && _rawImage.texture.wrapMode != TextureWrapMode.Repeat)
            {
                Debug.LogWarning($"[UIFogEffect] Texture on {_rawImage.name} is not set to Repeat. Seamless scrolling may fail.", _rawImage.texture);
            }

            _uvRect = _rawImage.uvRect;
            
            if (playOnAwake)
            {
                Resume();
            }
            else
            {
                Pause();
            }

            SetupLayout();
        }

        private void Update()
        {
            if (isPaused) return;

            // Update UV coordinates based on speed and direction
            _uvRect.x += scrollDirection.x * scrollSpeed * Time.deltaTime;
            _uvRect.y += scrollDirection.y * scrollSpeed * Time.deltaTime;

            // Keep UVs in a sane range to avoid precision issues over very long periods
            if (_uvRect.x > 1f) _uvRect.x -= 1f;
            if (_uvRect.x < -1f) _uvRect.x += 1f;
            if (_uvRect.y > 1f) _uvRect.y -= 1f;
            if (_uvRect.y < -1f) _uvRect.y += 1f;

            _rawImage.uvRect = _uvRect;
        }

        private void OnValidate()
        {
            if (_rawImage == null) _rawImage = GetComponent<RawImage>();
            _rawImage.color = fogColor;
            SetupLayout();
        }

        /// <summary>
        /// Aligns the RectTransform to the bottom of the parent and sets the height.
        /// </summary>
        public void SetupLayout()
        {
            RectTransform rect = GetComponent<RectTransform>();
            if (rect == null) return;

            // Anchor to Bottom-Stretch
            rect.anchorMin = new Vector2(0, 0);
            rect.anchorMax = new Vector2(1, heightScale);
            rect.pivot = new Vector2(0.5f, 0); // Bottom center pivot
            
            // Reset offsets
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        public void Pause() => isPaused = true;
        public void Resume() => isPaused = false;
        public void SetSpeed(float speed) => scrollSpeed = speed;
        public void SetAlpha(float alpha)
        {
            fogColor.a = alpha;
            _rawImage.color = fogColor;
        }
    }
}
