using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace God.UI
{
    /// <summary>
    /// Manages falling leaf effects for Unity uGUI with object pooling.
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
            public Vector2 HorizontalSpeedRange; // Constant horizontal movement
            public Vector2 RotationSpeedRange; 
            public Vector2 HorizontalDriftRange; // Sine wave amplitude
            public float SpawnInterval;
        }

        [Header("Configuration")]
        public LeafSettings m_Settings = new LeafSettings
        {
            InitialPoolSize = 20,
            SizeRange = new Vector2(30, 60),
            FallSpeedRange = new Vector2(100, 200),
            RotationSpeedRange = new Vector2(30, 180),
            HorizontalDriftRange = new Vector2(20, 50),
            SpawnInterval = 0.5f
        };

        [Header("Area")]
        [SerializeField] private RectTransform m_BoundArea;

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
        }

        private void Awake()
        {
            m_ManagerRect = GetComponent<RectTransform>();
            if (m_BoundArea == null) m_BoundArea = m_ManagerRect;
            
            InitializePool();
        }

        private void OnEnable()
        {
            m_SpawnTimer = m_Settings.SpawnInterval;
        }

        private void OnDisable()
        {
            ClearAllLeafs();
        }

        private void Update()
        {
            float dt = Time.deltaTime;

            // Spawning
            m_SpawnTimer += dt;
            if (m_SpawnTimer >= m_Settings.SpawnInterval)
            {
                m_SpawnTimer = 0f;
                SpawnLeaf();
            }

            // Update Active Leafs
            Rect bounds = m_BoundArea.rect;
            float buffer = 100f; // Buffer to ensure it clears off-screen

            for (int i = m_ActiveLeafs.Count - 1; i >= 0; i--)
            {
                var leaf = m_ActiveLeafs[i];

                // Movement
                leaf.Position.y -= leaf.FallSpeed * dt;
                leaf.Position.x += leaf.HorizontalSpeed * dt;
                
                // Horizontal Drift (Sine wave)
                float drift = Mathf.Sin(Time.time * leaf.DriftFrequency + leaf.DriftOffset) * leaf.DriftSpeed;
                leaf.Position.x += drift * dt;

                // Rotation
                leaf.Rotation += leaf.RotationSpeed * dt;

                // Apply to UI
                leaf.Rect.anchoredPosition = leaf.Position;
                leaf.Rect.localRotation = Quaternion.Euler(leaf.Rotation);

                // Check Bounds (Bottom or Right)
                if (leaf.Position.y < bounds.yMin - buffer || leaf.Position.x > bounds.xMax + buffer)
                {
                    ReturnToPool(leaf);
                    m_ActiveLeafs.RemoveAt(i);
                }
            }
}

        private void InitializePool()
        {
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
            if (m_Settings.LeafSprites == null || m_Settings.LeafSprites.Length == 0) return;

            Image img;
            if (m_Pool.Count > 0)
            {
                img = m_Pool.Pop();
            }
            else
            {
                img = CreateLeafImage();
            }

            Rect bounds = m_BoundArea.rect;
            
            // To fall from top-left to bottom-right, we spawn on the top or left edges.
            float startX, startY;
            if (Random.value > 0.3f) // 70% spawn at top
            {
                // Spawn along the top, extending to the left outside bounds to cover diagonal path
                startX = Random.Range(bounds.xMin - bounds.width * 0.5f, bounds.xMax);
                startY = bounds.yMax + 50f;
            }
            else // 30% spawn at left
            {
                startX = bounds.xMin - 50f;
                startY = Random.Range(bounds.yMin, bounds.yMax);
            }

            float size = Random.Range(m_Settings.SizeRange.x, m_Settings.SizeRange.y);
            img.sprite = m_Settings.LeafSprites[Random.Range(0, m_Settings.LeafSprites.Length)];
            
            // Random Color from list
            if (m_Settings.Colors != null && m_Settings.Colors.Length > 0)
            {
                img.color = m_Settings.Colors[Random.Range(0, m_Settings.Colors.Length)];
            }
            else
            {
                img.color = Color.white;
            }

            RectTransform rt = img.rectTransform;
            rt.sizeDelta = new Vector2(size, size);
            rt.anchoredPosition = new Vector2(startX, startY);
            
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
                ) * (Random.value > 0.5f ? 1 : -1),
                Rotation = new Vector3(Random.Range(0f, 360f), Random.Range(0f, 360f), Random.Range(0f, 360f)),
                DriftSpeed = Random.Range(m_Settings.HorizontalDriftRange.x, m_Settings.HorizontalDriftRange.y),
                DriftFrequency = Random.Range(1f, 3f),
                DriftOffset = Random.Range(0f, Mathf.PI * 2),
                Size = size
            };

            m_ActiveLeafs.Add(data);
            img.gameObject.SetActive(true);
        }

        private void ReturnToPool(LeafData leaf)
        {
            leaf.Image.gameObject.SetActive(false);
            m_Pool.Push(leaf.Image);
        }

        private void ClearAllLeafs()
        {
            foreach (var leaf in m_ActiveLeafs)
            {
                ReturnToPool(leaf);
            }
            m_ActiveLeafs.Clear();
        }
    }
}
