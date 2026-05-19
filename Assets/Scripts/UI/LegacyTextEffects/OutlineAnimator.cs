using UnityEngine;
using UnityEngine.UI;

namespace LegacyTextEffects
{
    [RequireComponent(typeof(Outline))]
    public class OutlineAnimator : MonoBehaviour
    {
        [SerializeField] private Color pulseColor = Color.red;
        [SerializeField] private float speed = 2.0f;
        [SerializeField] private Vector2 maxDistance = new Vector2(2, -2);
        [SerializeField] private bool animateDistance = false;

        private Outline _outline;
        private Color _originalColor;
        private Vector2 _originalDistance;

        private void Awake()
        {
            _outline = GetComponent<Outline>();
            _originalColor = _outline.effectColor;
            _originalDistance = _outline.effectDistance;
        }

        private void Update()
        {
            float t = (Mathf.Sin(Time.time * speed) + 1.0f) / 2.0f;
            
            // Animate Color
            _outline.effectColor = Color.Lerp(_originalColor, pulseColor, t);

            // Animate Distance
            if (animateDistance)
            {
                _outline.effectDistance = Vector2.Lerp(_originalDistance, maxDistance, t);
            }
        }
    }
}
