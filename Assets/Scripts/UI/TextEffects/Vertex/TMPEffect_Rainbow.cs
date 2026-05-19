using TMPro;
using UnityEngine;

namespace UI.TextEffects
{
    /// <summary>
    /// Animated rainbow colors across text characters.
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(TMP_Text))]
    public class TMPEffect_Rainbow : MonoBehaviour
    {
        [Header("Rainbow Settings")]
        [SerializeField] private Gradient gradient;
        [SerializeField] private float speed = 1f;
        [SerializeField] private float spread = 0.1f;

        private TMP_Text _textComponent;

        private void Awake()
        {
            _textComponent = GetComponent<TMP_Text>();
            if (gradient == null)
            {
                gradient = new Gradient();
                gradient.SetKeys(
                    new GradientColorKey[] { new GradientColorKey(Color.red, 0), new GradientColorKey(Color.blue, 1) },
                    new GradientAlphaKey[] { new GradientAlphaKey(1, 0), new GradientAlphaKey(1, 1) }
                );
            }
        }

        private void Update()
        {
            if (_textComponent == null) return;

            _textComponent.ForceMeshUpdate();
            var textInfo = _textComponent.textInfo;

            int characterCount = textInfo.characterCount;
            if (characterCount == 0) return;

            float time = Time.time * speed;

            for (int i = 0; i < characterCount; i++)
            {
                TMP_CharacterInfo charInfo = textInfo.characterInfo[i];
                if (!charInfo.isVisible) continue;

                int materialIndex = charInfo.materialReferenceIndex;
                int vertexIndex = charInfo.vertexIndex;

                Color32[] vertexColors = textInfo.meshInfo[materialIndex].colors32;

                float colorPos = (time + (i * spread)) % 1f;
                Color32 color = gradient.Evaluate(colorPos);

                vertexColors[vertexIndex + 0] = color;
                vertexColors[vertexIndex + 1] = color;
                vertexColors[vertexIndex + 2] = color;
                vertexColors[vertexIndex + 3] = color;
            }

            _textComponent.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
        }
    }
}