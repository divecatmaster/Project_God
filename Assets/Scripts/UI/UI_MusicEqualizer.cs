using UnityEngine;
using DG.Tweening;

public class UI_MusicEqualizer : MonoBehaviour
{
    [SerializeField] RectTransform[] Bars;

    [SerializeField] float minHeight = 18f;
    [SerializeField] float maxHeight = 60f;
    [SerializeField] float minDuration = 0.15f;
    [SerializeField] float maxDuration = 0.35f;

    private bool _isPlaying;

    private void OnEnable()
    {
        //Play();
    }

    private void OnDisable()
    {
        Stop();
    }

    public void Play()
    {
        _isPlaying = true;

        for (int i = 0; i < Bars.Length; i++)
        {
            int index = i;

            DOVirtual.DelayedCall(index * 0.05f, () =>
            {
                AnimateBar(Bars[index]);
            });
        }
    }

    public void Stop()
    {
        _isPlaying = false;

        for (int i = 0; i < Bars.Length; i++)
        {
            if (Bars[i] == null)
                continue;

            Bars[i].DOKill();

            Vector2 size = Bars[i].sizeDelta;
            size.y = 8f;
            Bars[i].sizeDelta = size;
        }
    }

    void AnimateBar(RectTransform bar)
    {
        if (!_isPlaying || bar == null)
            return;

        float targetHeight = Random.Range(minHeight, maxHeight);
        float duration = Random.Range(minDuration, maxDuration);

        bar.DOKill();

        bar.DOSizeDelta(
                new Vector2(bar.sizeDelta.x, targetHeight),
                duration)
            .SetEase(Ease.InOutSine)
            .OnComplete(() => AnimateBar(bar));
    }
}