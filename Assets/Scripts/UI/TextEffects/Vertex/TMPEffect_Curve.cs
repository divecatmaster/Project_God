using TMPro;
using UnityEngine;

namespace UI.TextEffects
{
    /// <summary>
    /// Bends text along an arc.
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(TMP_Text))]
    public class TMPEffect_Curve : MonoBehaviour
    {
        [Header("Curve Settings")]
        [SerializeField] private float curveStrength = 100f;
        [SerializeField] private bool updateRealtime = true;

        private TMP_Text _textComponent;

        private void Awake()
        {
            _textComponent = GetComponent<TMP_Text>();
        }

        private void Update()
        {
            if (!updateRealtime && Application.isPlaying) return;
            ApplyCurve();
        }

        [ContextMenu("Apply Curve")]
        public void ApplyCurve()
        {
            if (_textComponent == null) return;

            _textComponent.ForceMeshUpdate();
            var textInfo = _textComponent.textInfo;
            int characterCount = textInfo.characterCount;

            if (characterCount == 0) return;

            float boundsMinX = _textComponent.bounds.min.x;
            float boundsMaxX = _textComponent.bounds.max.x;
            float width = boundsMaxX - boundsMinX;

            for (int i = 0; i < characterCount; i++)
            {
                TMP_CharacterInfo charInfo = textInfo.characterInfo[i];
                if (!charInfo.isVisible) continue;

                int vertexIndex = charInfo.vertexIndex;
                int materialIndex = charInfo.materialReferenceIndex;

                Vector3[] vertices = textInfo.meshInfo[materialIndex].vertices;

                for (int j = 0; j < 4; j++)
                {
                    Vector3 v = vertices[vertexIndex + j];
                    
                    // Normalize X position (0 to 1)
                    float px = (v.x - boundsMinX) / width;
                    
                    // Simple parabola for the arc: y = x * (1-x) * strength
                    float yOffset = px * (1.0f - px) * curveStrength;
                    
                    vertices[vertexIndex + j].y += yOffset;
                }
            }

            for (int i = 0; i < textInfo.meshInfo.Length; i++)
            {
                textInfo.meshInfo[i].mesh.vertices = textInfo.meshInfo[i].vertices;
                _textComponent.UpdateGeometry(textInfo.meshInfo[i].mesh, i);
            }
        }
    }
}