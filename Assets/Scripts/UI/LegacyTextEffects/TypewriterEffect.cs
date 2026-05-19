using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace LegacyTextEffects
{
    public class TypewriterEffect : TextEffectBase
    {
        [SerializeField] private float typingSpeed = 0.05f;
        [SerializeField] private bool playOnEnable = true;
        [SerializeField] private bool useRichText = true;

        public UnityEvent onComplete;

        private string _fullText;
        private Coroutine _typingCoroutine;
        private bool _isSkipping;

        public bool IsRunning => _typingCoroutine != null;

        protected override void OnEnable()
        {
            base.OnEnable();
            if (playOnEnable && !string.IsNullOrEmpty(TextComponent.text))
            {
                Play(TextComponent.text);
            }
        }

        public void Play(string text)
        {
            Stop();
            _fullText = text;
            _typingCoroutine = StartCoroutine(TypeText());
        }

        public void Stop()
        {
            if (_typingCoroutine != null)
            {
                StopCoroutine(_typingCoroutine);
                _typingCoroutine = null;
            }
            _isSkipping = false;
        }

        public void Skip()
        {
            if (IsRunning) _isSkipping = true;
        }

        private IEnumerator TypeText()
        {
            TextComponent.text = "";
            int currentPos = 0;

            while (currentPos < _fullText.Length)
            {
                if (_isSkipping)
                {
                    TextComponent.text = _fullText;
                    break;
                }

                if (useRichText && _fullText[currentPos] == '<')
                {
                    int endTag = _fullText.IndexOf('>', currentPos);
                    if (endTag != -1)
                    {
                        // Skip tag
                        currentPos = endTag + 1;
                        continue;
                    }
                }

                currentPos++;
                TextComponent.text = _fullText.Substring(0, currentPos);

                if (currentPos < _fullText.Length)
                    yield return new WaitForSeconds(typingSpeed);
            }

            _typingCoroutine = null;
            _isSkipping = false;
            onComplete?.Invoke();
        }
    }
}
