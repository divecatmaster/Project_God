using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace God.UI
{
    public enum FireflyType
    {
        RandomNoise,
        RightToLeft
    }

    /// <summary>
    /// A high-performance, production-ready firefly effect for Unity uGUI.
    /// Manages multiple fireflies using a single Update loop and object pooling.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    [AddComponentMenu("UI/Effects/Firefly Manager")]
    public class UIFireflyManager : MonoBehaviour
    {
        [System.Serializable]
        public struct FireflySettings
        {
            public FireflyType Type;
            public int Count;
            public Sprite GlowSprite;
            public Color Color;
            public Vector2 SizeRange;
            public Vector2 SpeedRange;
            public Vector2 BlinkSpeedRange;
            [Range(0, 1)] public float MinAlpha;
            [Range(0, 1)] public float MaxAlpha;
            public float Smoothness;
        }

        [Header("Configuration")]
        public FireflySettings m_Settings = new FireflySettings
        {
            Type = FireflyType.RandomNoise,
            Count = 15,
            Color = new Color(0.8f, 1f, 0.4f, 1f),
            SizeRange = new Vector2(10, 25),
            SpeedRange = new Vector2(20, 50),
            BlinkSpeedRange = new Vector2(0.5f, 2.0f),
            MinAlpha = 0.2f,
            MaxAlpha = 1.0f,
            Smoothness = 0.5f
        };

        [Header("Area")]
        [SerializeField] private RectTransform m_BoundArea;

        [Header("State")]
        [SerializeField] private bool m_IsPaused = false;

        private List<FireflyData> m_Fireflies = new List<FireflyData>();
        private Stack<Image> m_Pool = new Stack<Image>();
        private RectTransform m_ManagerRect;

        private class FireflyData
        {
            public Image Image;
            public RectTransform Rect;
            public Vector2 Position;
            public Vector2 TargetVelocity;
            public Vector2 CurrentVelocity;
            public float Size;
            public float BlinkPhase;
            public float BlinkSpeed;
            public float NoiseOffset;
            public float Speed;
        }

        private void Awake()
        {
            m_ManagerRect = GetComponent<RectTransform>();
            if (m_BoundArea == null) m_BoundArea = m_ManagerRect;
        }

        private void OnEnable()
        {
            InitializeFireflies();
        }

        private void OnDisable()
        {
            ClearFireflies();
        }

        private void Update()
        {
            if (m_IsPaused) return;

            float dt = Time.deltaTime;
            Rect bounds = m_BoundArea.rect;

            foreach (var firefly in m_Fireflies)
            {
                if (m_Settings.Type == FireflyType.RandomNoise)
                {
                    // 1. Movement Logic (Smooth random movement)
                    firefly.NoiseOffset += dt * 0.5f;
                    float noiseX = Mathf.PerlinNoise(firefly.NoiseOffset, 0) * 2 - 1;
                    float noiseY = Mathf.PerlinNoise(0, firefly.NoiseOffset) * 2 - 1;
                    
                    Vector2 noiseDir = new Vector2(noiseX, noiseY);
                    firefly.TargetVelocity = Vector2.Lerp(firefly.TargetVelocity, noiseDir * m_Settings.SpeedRange.y, dt * m_Settings.Smoothness);
                    firefly.CurrentVelocity = Vector2.Lerp(firefly.CurrentVelocity, firefly.TargetVelocity, dt * 2.0f);

                    firefly.Position += firefly.CurrentVelocity * dt;

                    // 2. Wrap/Clamp within bounds
                    if (firefly.Position.x < bounds.xMin) firefly.Position.x = bounds.xMax;
                    if (firefly.Position.x > bounds.xMax) firefly.Position.x = bounds.xMin;
                    if (firefly.Position.y < bounds.yMin) firefly.Position.y = bounds.yMax;
                    if (firefly.Position.y > bounds.yMax) firefly.Position.y = bounds.yMin;
                }
                else if (m_Settings.Type == FireflyType.RightToLeft)
                {
                    // 1. Movement Logic (Right to Left with natural floaty vertical drift)
                    firefly.NoiseOffset += dt * 0.5f;
                    float noiseY = (Mathf.PerlinNoise(0, firefly.NoiseOffset) * 2 - 1) * (firefly.Speed * 0.3f);
                    
                    firefly.Position.x -= firefly.Speed * dt;
                    firefly.Position.y += noiseY * dt;

                    // 2. Wrap with buffer so it fully disappears off-screen before wrapping
                    float buffer = firefly.Size;
                    if (firefly.Position.x < bounds.xMin - buffer)
                    {
                        firefly.Position.x = bounds.xMax + buffer;
                        // Randomize Y upon wrapping to avoid repetitive lanes
                        firefly.Position.y = Random.Range(bounds.yMin, bounds.yMax);
                    }
                    if (firefly.Position.y < bounds.yMin - buffer) firefly.Position.y = bounds.yMax + buffer;
                    if (firefly.Position.y > bounds.yMax + buffer) firefly.Position.y = bounds.yMin - buffer;
                }

                // 3. Blinking Logic
                firefly.BlinkPhase += dt * firefly.BlinkSpeed;
                float blink = (Mathf.Sin(firefly.BlinkPhase) + 1f) * 0.5f; // 0 to 1
                float alpha = Mathf.Lerp(m_Settings.MinAlpha, m_Settings.MaxAlpha, blink);

                // 4. Apply to UI
                firefly.Rect.anchoredPosition = firefly.Position;
                Color c = m_Settings.Color;
                c.a *= alpha;
                firefly.Image.color = c;
            }
        }

        public void SetPaused(bool paused) => m_IsPaused = paused;

        public void RefreshSettings()
        {
            ClearFireflies();
            InitializeFireflies();
        }

        private void InitializeFireflies()
        {
            Rect bounds = m_BoundArea.rect;

            for (int i = 0; i < m_Settings.Count; i++)
            {
                Image img = GetOrCreateImage();
                img.sprite = m_Settings.GlowSprite;
                img.raycastTarget = false;
                
                RectTransform rt = img.rectTransform;
                rt.SetParent(m_ManagerRect, false);
                
                float size = Random.Range(m_Settings.SizeRange.x, m_Settings.SizeRange.y);
                rt.sizeDelta = new Vector2(size, size);

                FireflyData data = new FireflyData
                {
                    Image = img,
                    Rect = rt,
                    Position = new Vector2(Random.Range(bounds.xMin, bounds.xMax), Random.Range(bounds.yMin, bounds.yMax)),
                    Size = size,
                    BlinkPhase = Random.Range(0f, Mathf.PI * 2),
                    BlinkSpeed = Random.Range(m_Settings.BlinkSpeedRange.x, m_Settings.BlinkSpeedRange.y),
                    NoiseOffset = Random.Range(0f, 1000f),
                    Speed = Random.Range(m_Settings.SpeedRange.x, m_Settings.SpeedRange.y)
                };

                m_Fireflies.Add(data);
                img.gameObject.SetActive(true);
            }
        }

        private Image GetOrCreateImage()
        {
            if (m_Pool.Count > 0) return m_Pool.Pop();

            GameObject go = new GameObject("Firefly", typeof(RectTransform), typeof(Image));
            return go.GetComponent<Image>();
        }

        private void ClearFireflies()
        {
            foreach (var f in m_Fireflies)
            {
                f.Image.gameObject.SetActive(false);
                m_Pool.Push(f.Image);
            }
            m_Fireflies.Clear();
        }

        private void OnValidate()
        {
            if (Application.isPlaying)
            {
                // Optionally refresh if settings change in editor during play
            }
        }
    }
}
