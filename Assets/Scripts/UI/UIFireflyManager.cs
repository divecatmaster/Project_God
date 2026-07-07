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

            [Header("Fade")]
            public bool FadeInOnEnable;
            public float FadeInDuration;
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
            Smoothness = 0.5f,

            FadeInOnEnable = false,
            FadeInDuration = 1.0f
        };

        [Header("Area")]
        [SerializeField] private RectTransform m_BoundArea;

        [Header("Fade Area")]
        [SerializeField] private RectTransform[] m_FadeAreas;
        [SerializeField, Range(0f, 1f)] private float m_FadeAreaAlpha = 0.15f;
        [SerializeField] private float m_FadeAreaFadeSpeed = 2.5f;

        [Header("State")]
        [SerializeField] private bool m_IsPaused = false;

        private List<FireflyData> m_Fireflies = new List<FireflyData>();
        private Stack<Image> m_Pool = new Stack<Image>();

        private RectTransform m_ManagerRect;

        private float m_FadeAlpha = 1f;
        private float m_FadeTimer = 0f;

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

            // Fade Area 안팎에서 부드럽게 알파 전환하기 위한 값
            public float AreaAlpha;
        }

        private void Awake()
        {
            m_ManagerRect = GetComponent<RectTransform>();

            if (m_BoundArea == null)
                m_BoundArea = m_ManagerRect;
        }

        private void OnEnable()
        {
            if (m_Settings.FadeInOnEnable)
            {
                m_FadeAlpha = 0f;
                m_FadeTimer = 0f;
            }
            else
            {
                m_FadeAlpha = 1f;
                m_FadeTimer = 0f;
            }

            InitializeFireflies();
        }

        private void OnDisable()
        {
            ClearFireflies();
        }

        private void Update()
        {
            if (m_IsPaused)
                return;

            float dt = Time.deltaTime;

            UpdateFadeIn(dt);
            UpdateFireflies(dt);
        }

        void UpdateFadeIn(float dt)
        {
            if (!m_Settings.FadeInOnEnable)
                return;

            if (m_FadeAlpha >= 1f)
                return;

            if (m_Settings.FadeInDuration <= 0f)
            {
                m_FadeAlpha = 1f;
            }
            else
            {
                m_FadeTimer += dt;
                m_FadeAlpha = Mathf.Clamp01(m_FadeTimer / m_Settings.FadeInDuration);
            }
        }

        void UpdateFireflies(float dt)
        {
            if (m_BoundArea == null)
                return;

            Rect bounds = m_BoundArea.rect;

            for (int i = 0; i < m_Fireflies.Count; i++)
            {
                FireflyData firefly = m_Fireflies[i];

                if (firefly == null || firefly.Image == null || firefly.Rect == null)
                    continue;

                if (m_Settings.Type == FireflyType.RandomNoise)
                {
                    firefly.NoiseOffset += dt * 0.5f;

                    float noiseX = Mathf.PerlinNoise(firefly.NoiseOffset, 0f) * 2f - 1f;
                    float noiseY = Mathf.PerlinNoise(0f, firefly.NoiseOffset) * 2f - 1f;

                    Vector2 noiseDir = new Vector2(noiseX, noiseY);

                    firefly.TargetVelocity = Vector2.Lerp(
                        firefly.TargetVelocity,
                        noiseDir * m_Settings.SpeedRange.y,
                        dt * m_Settings.Smoothness
                    );

                    firefly.CurrentVelocity = Vector2.Lerp(
                        firefly.CurrentVelocity,
                        firefly.TargetVelocity,
                        dt * 2.0f
                    );

                    firefly.Position += firefly.CurrentVelocity * dt;

                    if (firefly.Position.x < bounds.xMin) firefly.Position.x = bounds.xMax;
                    if (firefly.Position.x > bounds.xMax) firefly.Position.x = bounds.xMin;
                    if (firefly.Position.y < bounds.yMin) firefly.Position.y = bounds.yMax;
                    if (firefly.Position.y > bounds.yMax) firefly.Position.y = bounds.yMin;
                }
                else if (m_Settings.Type == FireflyType.RightToLeft)
                {
                    firefly.NoiseOffset += dt * 0.5f;

                    float noiseY = (Mathf.PerlinNoise(0f, firefly.NoiseOffset) * 2f - 1f) * (firefly.Speed * 0.3f);

                    firefly.Position.x -= firefly.Speed * dt;
                    firefly.Position.y += noiseY * dt;

                    float buffer = firefly.Size;

                    if (firefly.Position.x < bounds.xMin - buffer)
                    {
                        firefly.Position.x = bounds.xMax + buffer;
                        firefly.Position.y = Random.Range(bounds.yMin, bounds.yMax);
                    }

                    if (firefly.Position.y < bounds.yMin - buffer)
                        firefly.Position.y = bounds.yMax + buffer;

                    if (firefly.Position.y > bounds.yMax + buffer)
                        firefly.Position.y = bounds.yMin - buffer;
                }

                // Blinking
                firefly.BlinkPhase += dt * firefly.BlinkSpeed;

                float blink = (Mathf.Sin(firefly.BlinkPhase) + 1f) * 0.5f;
                float blinkAlpha = Mathf.Lerp(m_Settings.MinAlpha, m_Settings.MaxAlpha, blink);

                // Fade Area Alpha
                float targetAreaAlpha = IsInFadeArea(firefly.Position) ? m_FadeAreaAlpha : 1f;

                firefly.AreaAlpha = Mathf.MoveTowards(
                    firefly.AreaAlpha,
                    targetAreaAlpha,
                    m_FadeAreaFadeSpeed * dt
                );

                // Apply to UI
                firefly.Rect.anchoredPosition = firefly.Position;

                Color c = m_Settings.Color;
                c.a *= blinkAlpha * m_FadeAlpha * firefly.AreaAlpha;
                firefly.Image.color = c;
            }
        }

        public void SetPaused(bool paused)
        {
            m_IsPaused = paused;
        }

        public void RefreshSettings()
        {
            ClearFireflies();
            InitializeFireflies();
        }

        private void InitializeFireflies()
        {
            if (m_BoundArea == null)
                m_BoundArea = m_ManagerRect;

            Rect bounds = m_BoundArea.rect;

            for (int i = 0; i < m_Settings.Count; i++)
            {
                Image img = GetOrCreateImage();

                img.sprite = m_Settings.GlowSprite;
                img.enabled = m_Settings.GlowSprite != null;
                img.raycastTarget = false;

                RectTransform rt = img.rectTransform;
                rt.SetParent(m_ManagerRect, false);

                float size = Random.Range(m_Settings.SizeRange.x, m_Settings.SizeRange.y);

                rt.sizeDelta = new Vector2(size, size);
                rt.localRotation = Quaternion.identity;
                rt.localScale = Vector3.one;

                FireflyData data = new FireflyData
                {
                    Image = img,
                    Rect = rt,

                    Position = new Vector2(
                        Random.Range(bounds.xMin, bounds.xMax),
                        Random.Range(bounds.yMin, bounds.yMax)
                    ),

                    Size = size,

                    BlinkPhase = Random.Range(0f, Mathf.PI * 2f),
                    BlinkSpeed = Random.Range(m_Settings.BlinkSpeedRange.x, m_Settings.BlinkSpeedRange.y),

                    NoiseOffset = Random.Range(0f, 1000f),
                    Speed = Random.Range(m_Settings.SpeedRange.x, m_Settings.SpeedRange.y),

                    AreaAlpha = 1f
                };

                Color startColor = m_Settings.Color;
                startColor.a = m_Settings.FadeInOnEnable ? 0f : m_Settings.Color.a;
                img.color = startColor;

                rt.anchoredPosition = data.Position;

                m_Fireflies.Add(data);
                img.gameObject.SetActive(true);
            }
        }

        private Image GetOrCreateImage()
        {
            if (m_Pool.Count > 0)
                return m_Pool.Pop();

            GameObject go = new GameObject("Firefly", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(m_ManagerRect, false);

            Image img = go.GetComponent<Image>();
            img.raycastTarget = false;

            return img;
        }

        bool IsInFadeArea(Vector2 localPosition)
        {
            if (m_FadeAreas == null || m_FadeAreas.Length <= 0)
                return false;

            for (int i = 0; i < m_FadeAreas.Length; i++)
            {
                if (m_FadeAreas[i] == null)
                    continue;

                Rect rect = GetLocalRectInManager(m_FadeAreas[i]);

                if (rect.Contains(localPosition))
                    return true;
            }

            return false;
        }

        Rect GetLocalRectInManager(RectTransform target)
        {
            Vector3[] corners = new Vector3[4];
            target.GetWorldCorners(corners);

            Vector2 min = Vector2.positiveInfinity;
            Vector2 max = Vector2.negativeInfinity;

            for (int i = 0; i < corners.Length; i++)
            {
                Vector2 localPoint = m_ManagerRect.InverseTransformPoint(corners[i]);

                min = Vector2.Min(min, localPoint);
                max = Vector2.Max(max, localPoint);
            }

            return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
        }

        private void ClearFireflies()
        {
            for (int i = 0; i < m_Fireflies.Count; i++)
            {
                FireflyData f = m_Fireflies[i];

                if (f == null || f.Image == null)
                    continue;

                Color c = f.Image.color;
                c.a = 0f;
                f.Image.color = c;

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