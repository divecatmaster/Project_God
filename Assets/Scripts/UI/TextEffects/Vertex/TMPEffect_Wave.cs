using TMPro;
using UnityEngine;

namespace UI.TextEffects
{
    /// <summary>
    /// Per-character sine wave movement.
    /// Efficiently modifies TMP MeshInfo.
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(TMP_Text))]
    public class TMPEffect_Wave : MonoBehaviour
    {
        [Header("Wave Settings")]
        [SerializeField] private float amplitude = 10f;
        [SerializeField] private float frequency = 2f;
        [SerializeField] private float speed = 5f;

        private TMP_Text _textComponent;
        private TMP_MeshInfo[] _cachedMeshInfo;

        private void Awake()
        {
            _textComponent = GetComponent<TMP_Text>();
        }

        private void OnEnable()
        {
            TMPro_EventManager.TEXT_CHANGED_EVENT.Add(OnTextChanged);
        }

        private void OnDisable()
        {
            TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(OnTextChanged);
            _textComponent.ForceMeshUpdate();
        }

        private void OnTextChanged(Object obj)
        {
            if (obj == _textComponent)
                _cachedMeshInfo = null;
        }

        private void Update()
        {
            if (_textComponent == null) return;

            // Important: ForceMeshUpdate if we don't have mesh info or it's dirty
            _textComponent.ForceMeshUpdate();
            var textInfo = _textComponent.textInfo;

            if (_cachedMeshInfo == null)
                _cachedMeshInfo = textInfo.CopyMeshInfoVertexData();

            int characterCount = textInfo.characterCount;
            if (characterCount == 0) return;

            float time = Time.time * speed;

            for (int i = 0; i < characterCount; i++)
            {
                TMP_CharacterInfo charInfo = textInfo.characterInfo[i];
                if (!charInfo.isVisible) continue;

                int materialIndex = charInfo.materialReferenceIndex;
                int vertexIndex = charInfo.vertexIndex;

                Vector3[] sourceVertices = _cachedMeshInfo[materialIndex].vertices;
                Vector3[] destinationVertices = textInfo.meshInfo[materialIndex].vertices;

                float offset = Mathf.Sin(time + (i * frequency)) * amplitude;
                Vector3 translation = new Vector3(0, offset, 0);

                destinationVertices[vertexIndex + 0] = sourceVertices[vertexIndex + 0] + translation;
                destinationVertices[vertexIndex + 1] = sourceVertices[vertexIndex + 1] + translation;
                destinationVertices[vertexIndex + 2] = sourceVertices[vertexIndex + 2] + translation;
                destinationVertices[vertexIndex + 3] = sourceVertices[vertexIndex + 3] + translation;
            }

            for (int i = 0; i < textInfo.meshInfo.Length; i++)
            {
                textInfo.meshInfo[i].mesh.vertices = textInfo.meshInfo[i].vertices;
                _textComponent.UpdateGeometry(textInfo.meshInfo[i].mesh, i);
            }
        }
    }
}