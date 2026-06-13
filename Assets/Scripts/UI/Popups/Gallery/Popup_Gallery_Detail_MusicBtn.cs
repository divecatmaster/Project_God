using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;
using System;

public class Popup_Gallery_Detail_MusicBtn : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] Button Btn;
    [SerializeField] Image Glow;
    [SerializeField] Image PlayImg;
    [SerializeField] Sprite[] PlaySprites;
    [SerializeField] UI_MusicEqualizer MusicAnimation;
    [SerializeField] TextMeshProUGUI MusicName;


    Color _glowOnColor = new Color(1f, 1f, 1f, 0.3f);
    Action _callback;

    private void Awake() 
    {
        Btn.onClick.AddListener(OnClickBtn);
    }

    public void SetButton(string musicName, Action callback)
    {
        _callback = callback;
        Glow.color = UIUtility.Common_Off_Color;
        MusicName.text = musicName;
        SetPlay(false);
    }

    public void SetPlay(bool isPlay)
    {
        if (isPlay)
        {
            PlayImg.sprite = PlaySprites[1];
            MusicAnimation.Play();
        }
        else
        {
            PlayImg.sprite = PlaySprites[0];
            MusicAnimation.Stop();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        KillTweens();

        Glow.DOColor(_glowOnColor, 0.7f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        KillTweens();

        Glow.DOColor(UIUtility.Common_Off_Color, 0.7f);
    }

    private void KillTweens()
    {
        Glow?.DOKill();
    }

    void OnClickBtn()
    {
        _callback?.Invoke();
    }
}
