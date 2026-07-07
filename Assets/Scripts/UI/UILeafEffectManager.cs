using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace God.UI
{
    /// <summary>
    /// Manages falling leaf effects for Unity uGUI with object pooling.
    /// Fade Area 안에 들어온 나뭇잎은 서서히 투명해지고,
    /// 밖으로 나오면 서서히 원래 알파로 돌아옵니다.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    [AddComponentMenu("UI/Effects/Leaf Effect Manager")]
    public class UILeafEffectManager : MonoBehaviour
    {
        [System.Serializable]
        public struct LeafSettings
        {
            public Sprite[] LeafSprites;
            public Color[] Colors;

            public int InitialPoolSize;

            public Vector2 SizeRange;
            public Vector2 FallSpeedRange;
            public Vector2 HorizontalSpeedRange;
            public Vector2 RotationSpeedRange;
            public Vector2 HorizontalDriftRange;

            public float SpawnInterval;
        }

        [Header("Configuration")]
        public LeafSettings m_Settings = new LeafSettings
        {
            InitialPoolSize = 20,
            SizeRange = new Vector2(30, 60),
            FallSpeedRange = new Vector2(100, 200),
            HorizontalSpeedRange = new Vector2(30, 80),
            RotationSpeedRange = new Vector2(30, 180),
            HorizontalDriftRange = new Vector2(20, 50),
            SpawnInterval = 0.5f
        };

        [Header("Area")]
        [SerializeField] private RectTransform m_BoundArea;

        [Header("Fade Area")]
        [SerializeField] private RectTransform[] m_FadeAreas;
        [SerializeField, Range(0f, 1f)] private float m_FadeAreaAlpha = 0.15f;
        [SerializeField] private float m_FadeAreaFadeSpeed = 2.5f;

        private List<LeafData> m_ActiveLeafs = new List<LeafData>();
        private Stack<Image> m_Pool = new Stack<Image>();

        private RectTransform m_ManagerRect;
        private float m_SpawnTimer = 0f;

        private class LeafData
        {
            public Image Image;
            public RectTransform Rect;

            public Vector2 Position;

            public float FallSpeed;
            public float HorizontalSpeed;

            public Vector3 RotationSpeed;
            public Vector3 Rotation;

            public float DriftSpeed;
            public float DriftFrequency;
            public float DriftOffset;

            public float Size;

            public Color BaseColor;

            // Fade Area 안팎에서 부드럽게 알파 전환하기 위한 값
            public float AreaAlpha;
        }

        private void Awake()
        {
            m_ManagerRect = GetComponent<RectTransform>();

            if (m_BoundArea == null)
                m_BoundArea = m_ManagerRect;

            InitializePool();
        }

        private void OnEnable()
        {
            // 켜지자마자 바로 하나 생성되게 하려면 SpawnInterval로 둠
            m_SpawnTimer = m_Settings.SpawnInterval;
        }

        private void OnDisable()
        {
            ClearAllLeafs();
        }

        private void Update()
        {
            float dt = Time.unscaledDeltaTime;

            UpdateSpawn(dt);
            UpdateLeafs(dt);
        }

        void UpdateSpawn(float dt)
        {
            if (m_Settings.SpawnInterval <= 0f)
                return;

            m_SpawnTimer += dt;

            if (m_SpawnTimer >= m_Settings.SpawnInterval)
            {
                m_SpawnTimer = 0f;
                SpawnLeaf();
            }
        }

        void UpdateLeafs(float dt)
        {
            if (m_BoundArea == null)
                return;

            Rect bounds = m_BoundArea.rect;
            float buffer = 100f;

            for (int i = m_ActiveLeafs.Count - 1; i >= 0; i--)
            {
                LeafData leaf = m_ActiveLeafs[i];

                if (leaf == null || leaf.Image == null || leaf.Rect == null)
                {
                    m_ActiveLeafs.RemoveAt(i);
                    continue;
                }

                // Movement
                leaf.Position.y -= leaf.FallSpeed * dt;
                leaf.Position.x += leaf.HorizontalSpeed * dt;

                // Horizontal Drift
                float drift = Mathf.Sin(Time.unscaledTime * leaf.DriftFrequency + leaf.DriftOffset) * leaf.DriftSpeed;
                leaf.Position.x += drift * dt;

                // Rotation
                leaf.Rotation += leaf.RotationSpeed * dt;

                // Apply position / rotation
                leaf.Rect.anchoredPosition = leaf.Position;
                leaf.Rect.localRotation = Quaternion.Euler(leaf.Rotation);

                // Fade Area Alpha
                float targetAreaAlpha = IsInFadeArea(leaf.Position) ? m_FadeAreaAlpha : 1f;

                leaf.AreaAlpha = Mathf.MoveTowards(
                    leaf.AreaAlpha,
                    targetAreaAlpha,
                    m_FadeAreaFadeSpeed * dt
                );

                Color c = leaf.BaseColor;
                c.a *= leaf.AreaAlpha;
                leaf.Image.color = c;

                // Check Bounds
                if (leaf.Position.y < bounds.yMin - buffer ||
                    leaf.Position.x > bounds.xMax + buffer ||
                    leaf.Position.x < bounds.xMin - buffer)
                {
                    ReturnToPool(leaf);
                    m_ActiveLeafs.RemoveAt(i);
                }
            }
        }

        private void InitializePool()
        {
            m_Pool.Clear();

            for (int i = 0; i < m_Settings.InitialPoolSize; i++)
            {
                Image img = CreateLeafImage();
                img.gameObject.SetActive(false);
                m_Pool.Push(img);
            }
        }

        private Image CreateLeafImage()
        {
            GameObject go = new GameObject("Leaf", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(m_ManagerRect, false);

            Image img = go.GetComponent<Image>();
            img.raycastTarget = false;

            return img;
        }

        private void SpawnLeaf()
        {
            if (m_Settings.LeafSprites == null || m_Settings.LeafSprites.Length == 0)
                return;

            Image img;

            if (m_Pool.Count > 0)
            {
                img = m_Pool.Pop();
            }
            else
            {
                img = CreateLeafImage();
            }

            if (m_BoundArea == null)
                m_BoundArea = m_ManagerRect;

            Rect bounds = m_BoundArea.rect;

            float startX;
            float startY;

            // 위쪽에서 주로 생성
            if (Random.value > 0.3f)
            {
                startX = Random.Range(bounds.xMin - bounds.width * 0.5f, bounds.xMax);
                startY = bounds.yMax + 50f;
            }
            // 왼쪽에서는 너무 아래에서 나오지 않게 상단 위주
            else
            {
                startX = bounds.xMin - 50f;
                startY = Random.Range(bounds.yMin + bounds.height * 0.3f, bounds.yMax);
            }

            float size = Random.Range(m_Settings.SizeRange.x, m_Settings.SizeRange.y);

            img.sprite = m_Settings.LeafSprites[Random.Range(0, m_Settings.LeafSprites.Length)];

            Color baseColor = Color.white;

            if (m_Settings.Colors != null && m_Settings.Colors.Length > 0)
            {
                baseColor = m_Settings.Colors[Random.Range(0, m_Settings.Colors.Length)];
            }

            img.color = baseColor;

            RectTransform rt = img.rectTransform;
            rt.SetParent(m_ManagerRect, false);
            rt.sizeDelta = new Vector2(size, size);
            rt.anchoredPosition = new Vector2(startX, startY);
            rt.localRotation = Quaternion.identity;

            LeafData data = new LeafData
            {
                Image = img,
                Rect = rt,

                Position = new Vector2(startX, startY),

                FallSpeed = Random.Range(m_Settings.FallSpeedRange.x, m_Settings.FallSpeedRange.y),
                HorizontalSpeed = Random.Range(m_Settings.HorizontalSpeedRange.x, m_Settings.HorizontalSpeedRange.y),

                RotationSpeed = new Vector3(
                    Random.Range(m_Settings.RotationSpeedRange.x, m_Settings.RotationSpeedRange.y),
                    Random.Range(m_Settings.RotationSpeedRange.x, m_Settings.RotationSpeedRange.y),
                    Random.Range(m_Settings.RotationSpeedRange.x, m_Settings.RotationSpeedRange.y)
                ) * (Random.value > 0.5f ? 1f : -1f),

                Rotation = new Vector3(
                    Random.Range(0f, 360f),
                    Random.Range(0f, 360f),
                    Random.Range(0f, 360f)
                ),

                DriftSpeed = Random.Range(m_Settings.HorizontalDriftRange.x, m_Settings.HorizontalDriftRange.y),
                DriftFrequency = Random.Range(1f, 3f),
                DriftOffset = Random.Range(0f, Mathf.PI * 2f),

                Size = size,

                BaseColor = baseColor,

                // 처음 생성 시에는 원래 알파로 시작
                AreaAlpha = 1f
            };

            m_ActiveLeafs.Add(data);
            img.gameObject.SetActive(true);
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

        private void ReturnToPool(LeafData leaf)
        {
            if (leaf == null || leaf.Image == null)
                return;

            leaf.Image.gameObject.SetActive(false);
            m_Pool.Push(leaf.Image);
        }

        private void ClearAllLeafs()
        {
            for (int i = 0; i < m_ActiveLeafs.Count; i++)
            {
                ReturnToPool(m_ActiveLeafs[i]);
            }

            m_ActiveLeafs.Clear();
        }
    }
}