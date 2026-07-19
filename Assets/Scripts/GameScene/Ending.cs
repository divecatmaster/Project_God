using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using God.Audio;

public class Ending : MonoBehaviour
{
    [SerializeField] RectTransform[] Lines;
    [SerializeField] TextMeshProUGUI EndingText;
    [SerializeField] TextMeshProUGUI Title;
    [SerializeField] CanvasGroup _CanvasGroup;
    [SerializeField] TextMeshProUGUI NextStory;

    Coroutine _titleLineCoroutine;

    public void SetEnding(int idx)
    {
        _CanvasGroup.DOKill();
        _CanvasGroup.alpha = 0f;

        Title.DOKill();
        EndingText.DOKill();
        NextStory.DOKill();

        Title.color = UIUtility.Common_Off_Color;
        EndingText.color = UIUtility.Common_Off_Color;
        NextStory.color = UIUtility.Common_Off_Color;

        for (int i = 0; i < Lines.Length; i++)
        {
            if (Lines[i] == null)
                continue;

            Lines[i].SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 0f);

            Image lineImage = Lines[i].GetComponent<Image>();
            if (lineImage != null)
            {
                Color lineColor = lineImage.color;
                lineColor.a = 1f;
                lineImage.color = lineColor;
            }

            CanvasGroup lineCanvasGroup = Lines[i].GetComponent<CanvasGroup>();
            if (lineCanvasGroup != null)
            {
                lineCanvasGroup.DOKill();
                lineCanvasGroup.alpha = 1f;
            }
        }

        if (idx > 0)
        {
            SetFakeEnding(idx);
        }
        else
        {
            SetRealEnding(idx);
        }
    }

    void SetRealEnding(int idx)
    {
        int realIdx = Mathf.Abs(idx);

        EndingText.text = LanguageManager.Instance.GetText($"Ending_Expl_{realIdx}");

        if (_titleLineCoroutine != null)
            StopCoroutine(_titleLineCoroutine);

        _titleLineCoroutine = StartCoroutine(PlayRealEnding());

        Data_Manager.Instance.AddEndingCount(realIdx);
    }

    IEnumerator PlayRealEnding()
    {
        StoryManager.Instance.IsOpening = true;

        if (_CanvasGroup != null)
        {
            _CanvasGroup.DOKill();
            _CanvasGroup.alpha = 0f;
        }

        if (_CanvasGroup != null)
        {
            _CanvasGroup.DOFade(1f, 3f).SetEase(Ease.Linear);
        }

        yield return new WaitForSecondsRealtime(3f);
        

        if (EndingText != null)
        {
            yield return EndingText
                .DOFade(1f, 1f)
                .SetEase(Ease.Linear)
                .WaitForCompletion();
        }

        yield return new WaitForSecondsRealtime(2f);

        // EndingText.DOFade(0, 1f);
        // yield return new WaitForSecondsRealtime(1f);

        _titleLineCoroutine = null;
        StoryManager.Instance.IsOpening = false;

        GameSceneManager.Instance.GoToMainScene();
    }

    void SetFakeEnding(int idx)
    {
        EndingText.text = LanguageManager.Instance.GetText($"Ending_Title_{idx}");

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

        yield return new WaitForSecondsRealtime(3f);

        SoundManager.Instance.StopBGM();

        // Lines, EndingText, Title 전부 알파 1 -> 0, 1초
        Sequence hideSequence = DOTween.Sequence();

        if (Title != null)
        {
            Title.DOKill();
            hideSequence.Join(
                Title.DOFade(0f, 1f).SetEase(Ease.Linear)
            );
        }

        if (EndingText != null)
        {
            EndingText.DOKill();
            hideSequence.Join(
                EndingText.DOFade(0f, 1f).SetEase(Ease.Linear)
            );
        }

        for (int i = 0; i < Lines.Length; i++)
        {
            if (Lines[i] == null)
                continue;

            CanvasGroup lineCanvasGroup = GetOrAddCanvasGroup(Lines[i]);

            if (lineCanvasGroup == null)
                continue;

            lineCanvasGroup.DOKill();

            hideSequence.Join(
                lineCanvasGroup.DOFade(0f, 1f).SetEase(Ease.Linear)
            );
        }

        yield return hideSequence.WaitForCompletion();

        // 1초 대기
        yield return new WaitForSecondsRealtime(1f);

        // NextStory 알파 0 -> 1, 1초
        if (NextStory != null)
        {
            NextStory.DOKill();

            Color nextColor = NextStory.color;
            nextColor.a = 0f;
            NextStory.color = nextColor;

            yield return NextStory
                .DOFade(1f, 1f)
                .SetEase(Ease.Linear)
                .WaitForCompletion();
        }

        // 2초 대기
        StoryManager.Instance.SetNextEnding();

        yield return new WaitForSecondsRealtime(2f);
        

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

    CanvasGroup GetOrAddCanvasGroup(RectTransform rect)
    {
        if (rect == null)
            return null;

        CanvasGroup canvasGroup = rect.GetComponent<CanvasGroup>();

        if (canvasGroup == null)
            canvasGroup = rect.gameObject.AddComponent<CanvasGroup>();

        return canvasGroup;
    }
}
