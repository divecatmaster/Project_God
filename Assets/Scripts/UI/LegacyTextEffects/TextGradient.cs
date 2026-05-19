using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LegacyTextEffects
{
    [AddComponentMenu("UI/Effects/Text Gradient")]
    public class TextGradient : BaseMeshEffect
    {
        [SerializeField] private Color colorTop = Color.white;
        [SerializeField] private Color colorBottom = Color.black;
        [SerializeField] private bool useAlpha = true;

        public override void ModifyMesh(VertexHelper vh)
        {
            if (!IsActive()) return;

            List<UIVertex> vertices = new List<UIVertex>();
            vh.GetUIVertexStream(vertices);

            if (vertices.Count == 0) return;

            float bottomY = vertices[0].position.y;
            float topY = vertices[0].position.y;

            for (int i = 1; i < vertices.Count; i++)
            {
                float y = vertices[i].position.y;
                if (y > topY) topY = y;
                else if (y < bottomY) bottomY = y;
            }

            float height = topY - bottomY;

            for (int i = 0; i < vertices.Count; i++)
            {
                UIVertex v = vertices[i];
                float t = Mathf.InverseLerp(bottomY, topY, v.position.y);
                Color c = Color.Lerp(colorBottom, colorTop, t);
                
                if (!useAlpha) c.a = v.color.a;
                else c.a *= (v.color.a / 255f);

                v.color = c;
                vertices[i] = v;
            }

            vh.Clear();
            vh.AddUIVertexTriangleStream(vertices);
        }
    }
}
