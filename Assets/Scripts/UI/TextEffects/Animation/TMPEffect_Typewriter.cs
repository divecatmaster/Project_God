using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace UI.TextEffects
{
    /// <summary>
    /// Production-ready Typewriter effect for TextMeshPro.
    /// Supports rich text, skip functionality, and custom speed.
    /// </summary>
    [RequireComponent(typeof(TMP_Text))]
    public class TMPEffect_Typewriter : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float charactersPerSecond = 30f;
        [SerializeField] private bool playOnEnable = true;
        [SerializeField] private bool useSmoothReveal = false; // Could implement alpha-per-char reveal

        [Header("Events")]
        public UnityEvent onComplete;
        public UnityEvent onCharacterTyped;

        private TMP_Text _textComponent;
        private Coroutine _typeRoutine;
        private string _originalText;
        private bool _isBusy;

        public bool IsBusy => _isBusy;

        private void Awake()
        {
            _textComponent = GetComponent<TMP_Text>();
        }

        private void OnEnable()
        {
            if (playOnEnable)
            {
                Play();
            }
        }

        private void OnDisable()
        {
            Stop();
        }

        public void Play()
        {
            Play(_textComponent.text);
        }

        public void Play(string text)
        {
            Stop();
            _originalText = text;
            _textComponent.text = _originalText;
            _textComponent.maxVisibleCharacters = 0;
            _typeRoutine = StartCoroutine(TypeRoutine());
        }

        public void Stop()
        {
            if (_typeRoutine != null)
            {
                StopCoroutine(_typeRoutine);
                _typeRoutine = null;
            }
            _isBusy = false;
        }

        public void Skip()
        {
            if (!_isBusy) return;
            Stop();
            _textComponent.maxVisibleCharacters = _originalText.Length;
            onComplete?.Invoke();
        }

        private IEnumerator TypeRoutine()
        {
            _isBusy = true;
            _textComponent.ForceMeshUpdate();
            
            int totalVisibleCharacters = _textComponent.textInfo.characterCount;
            int counter = 0;

            float waitTime = 1f / Mathf.Max(0.1f, charactersPerSecond);

            while (counter <= totalVisibleCharacters)
            {
                _textComponent.maxVisibleCharacters = counter;
                onCharacterTyped?.Invoke();
                
                counter++;
                yield return new WaitForSeconds(waitTime);
            }

            _isBusy = false;
            onComplete?.Invoke();
        }
    }
}