using TMPro;
using UnityEngine;

namespace UI.TextEffects
{
    /// <summary>
    /// Animates a "shine" highlight passing through the text.
    /// Uses vertex color modification.
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(TMP_Text))]
    public class TMPEffect_GradientScroll : MonoBehaviour
    {
        [Header("Shine Settings")]
        [SerializeField] private Color shineColor = Color.white;
        [SerializeField] private float shineWidth = 0.2f;
        [SerializeField] private float speed = 1.0f;
        [SerializeField] private float interval = 2.0f;

        private TMP_Text _textComponent;
        private Color32 _baseColor;

        private void Awake()
        {
            _textComponent = GetComponent<TMP_Text>();
            _baseColor = _textComponent.color;
        }

        private void Update()
        {
            if (_textComponent == null) return;

            _textComponent.ForceMeshUpdate();
            var textInfo = _textComponent.textInfo;
            int characterCount = textInfo.characterCount;

            if (characterCount == 0) return;

            // Loop t from 0 to 1 + shineWidth to allow the shine to fully pass through
            float totalPath = 1.0f + shineWidth;
            float t = (Time.time * speed) % (interval * speed);
            t /= (interval * speed); // 0 to 1 over interval
            
            // Map 0-1 to path
            float pos = t * totalPath - (shineWidth * 0.5f);

            for (int i = 0; i < characterCount; i++)
            {
                TMP_CharacterInfo charInfo = textInfo.characterInfo[i];
                if (!charInfo.isVisible) continue;

                int materialIndex = charInfo.materialReferenceIndex;
                int vertexIndex = charInfo.vertexIndex;
                Color32[] colors = textInfo.meshInfo[materialIndex].colors32;

                // Relative character position 0 to 1
                float charPos = (float)i / characterCount;
                
                // Calculate distance to highlight center
                float dist = Mathf.Abs(charPos - pos);
                float influence = Mathf.Clamp01(1.0f - (dist / shineWidth));
                
                Color32 finalColor = Color32.Lerp(_textComponent.color, shineColor, influence);

                colors[vertexIndex + 0] = finalColor;
                colors[vertexIndex + 1] = finalColor;
                colors[vertexIndex + 2] = finalColor;
                colors[vertexIndex + 3] = finalColor;
            }

            _textComponent.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
        }
    }
}