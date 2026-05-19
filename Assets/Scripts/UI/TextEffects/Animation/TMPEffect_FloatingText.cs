using System.Collections;
using TMPro;
using UnityEngine;

namespace UI.TextEffects
{
    /// <summary>
    /// Animates a floating text popup (e.g., damage numbers).
    /// Best used with a pooling system.
    /// </summary>
    [RequireComponent(typeof(TMP_Text))]
    public class TMPEffect_FloatingText : MonoBehaviour
    {
        [Header("Animation Settings")]
        [SerializeField] private float lifetime = 1f;
        [SerializeField] private float floatSpeed = 50f;
        [SerializeField] private AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 0.5f, 1, 1.2f);
        [SerializeField] private AnimationCurve alphaCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
        
        [Header("Random Spread")]
        [SerializeField] private Vector2 randomSpreadRange = new Vector2(20, 0);

        private TMP_Text _textComponent;
        private float _elapsedTime;
        private Vector3 _startPosition;
        private Vector3 _offsetDirection;

        private void Awake()
        {
            _textComponent = GetComponent<TMP_Text>();
        }

        public void Play(string text, Color color, bool isCritical = false)
        {
            _textComponent.text = text;
            _textComponent.color = color;
            
            _elapsedTime = 0;
            _startPosition = transform.position;
            
            // Random horizontal spread
            float randomX = Random.Range(-randomSpreadRange.x, randomSpreadRange.x);
            _offsetDirection = new Vector3(randomX, 0, 0);

            if (isCritical)
            {
                transform.localScale = Vector3.one * 1.5f;
            }
            else
            {
                transform.localScale = Vector3.one;
            }

            gameObject.SetActive(true);
        }

        private void Update()
        {
            _elapsedTime += Time.deltaTime;
            float t = _elapsedTime / lifetime;

            if (t >= 1.0f)
            {
                gameObject.SetActive(false);
                return;
            }

            // Movement
            transform.position = _startPosition + _offsetDirection + (Vector3.up * (floatSpeed * _elapsedTime));

            // Scale
            float scale = scaleCurve.Evaluate(t);
            transform.localScale = Vector3.one * scale;

            // Alpha
            Color c = _textComponent.color;
            c.a = alphaCurve.Evaluate(t);
            _textComponent.color = c;
        }
    }
}