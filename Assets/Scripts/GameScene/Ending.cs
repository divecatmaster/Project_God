using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class Ending : MonoBehaviour
{
    [SerializeField] RectTransform[] Lines;
    [SerializeField] TextMeshProUGUI EndingText;
    [SerializeField] TextMeshProUGUI Title;
    [SerializeField] CanvasGroup _CanvasGroup;

    Coroutine _titleLineCoroutine;

    public void SetEnding(int idx)
    {
        _CanvasGroup.alpha = 0f;
        Title.color = UIUtility.Common_Off_Color;
        EndingText.color = UIUtility.Common_Off_Color;
        EndingText.text = LanguageManager.Instance.GetText($"Ending_Title_{idx}");
        Lines[0].SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 0f);
        Lines[1].SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 0f);

        PlayTitleLine();
    }

    void PlayTitleLine()
    {
        if (_titleLineCoroutine != null)
            StopCoroutine(_titleLineCoroutine);

        _titleLineCoroutine = StartCoroutine(PlayTitleLineRoutine());
    }

    IEnumerator PlayTitleLineRoutine()
    {
        StoryManager.Instance.IsOpening = true;
        // 초기화        
        if (_CanvasGroup != null)
        {
            _CanvasGroup.DOKill();
            _CanvasGroup.alpha = 0f;
        }

        if (Title != null)
        {
            Title.DOKill();

            Color titleColor = Title.color;
            titleColor.a = 0f;
            Title.color = titleColor;
        }

        for (int i = 0; i < Lines.Length; i++)
        {
            if (Lines[i] == null)
                continue;

            Lines[i].SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 0f);
        }

        // CanvasGroup 알파 0 -> 1, 1초
        if (_CanvasGroup != null)
        {
            _CanvasGroup.DOFade(1f, 1f).SetEase(Ease.Linear);
        }

        // Title 알파 0 -> 1, 1초
        if (Title != null)
        {
            yield return Title
                .DOFade(1f, 1f)
                .SetEase(Ease.Linear)
                .WaitForCompletion();
        }
        else
        {
            yield return new WaitForSecondsRealtime(1f);
        }

        // Lines width 0 -> 400, 1.5초
        float elapsed = 0f;
        float duration = 1.5f;
        float targetWidth = 400f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(elapsed / duration);
            float width = Mathf.Lerp(0f, targetWidth, t);

            if (Lines.Length > 0 && Lines[0] != null)
                Lines[0].SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);

            if (Lines.Length > 1 && Lines[1] != null)
                Lines[1].SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);

            yield return null;
        }

        if (Lines.Length > 0 && Lines[0] != null)
            Lines[0].SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, targetWidth);

        if (Lines.Length > 1 && Lines[1] != null)
            Lines[1].SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, targetWidth);

        if (EndingText != null)
        {
            yield return EndingText
                .DOFade(1f, 1f)
                .SetEase(Ease.Linear)
                .WaitForCompletion();
        }

        StoryManager.Instance.SetNextEnding();

        yield return new WaitForSecondsRealtime(3f);

        if (_CanvasGroup != null)
        {
            _CanvasGroup.DOKill();

            yield return _CanvasGroup
                .DOFade(0f, 1f)
                .SetEase(Ease.Linear)
                .WaitForCompletion();
        }
        else
        {
            yield return new WaitForSecondsRealtime(1f);
        }

        _titleLineCoroutine = null;
        gameObject.SetActive(false);
        StoryManager.Instance.IsOpening = false;
    }
}
