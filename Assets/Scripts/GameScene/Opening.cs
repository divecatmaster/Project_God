using System.Collections;
using TMPro;
using UnityEngine;
using DG.Tweening;
using System;
using UnityEngine.UI;
using God.Audio;
using System.Collections.Generic;

public class Opening : MonoBehaviour
{
        [SerializeField] TextMeshProUGUI Main_Text;
        [SerializeField] TextMeshProUGUI Main_Text_Sub;
        [SerializeField] CanvasGroup CanvasGroup;
        [SerializeField] Image MainBG;
        [SerializeField] TextMeshProUGUI Sub_Text_1;
        [SerializeField] TextMeshProUGUI Sub_Text_2;
        [SerializeField] Button SkipBtn;
        [SerializeField] GameObject Warning_Obj;
        [SerializeField] CanvasGroup Warning_Obj_Canvas;
        [SerializeField] CanvasGroup Warning;
        [SerializeField] private God.UI.UIOldFilmEffect oldFilmEffect;

        public God.UI.UIOldFilmEffect OldFilmEffect => oldFilmEffect;

        /// <summary>
        /// Adjusts the old film effect intensity (0 to 1).
        /// </summary>
        public void SetFilmEffectIntensity(float intensity)
        {
                if (oldFilmEffect != null)
                {
                        oldFilmEffect.SetMasterIntensity(intensity);
                }
        }

        float typing_speed_1 = 0.08f;
        Color _offColor = new Color(1f, 1f, 1f, 0f);
        private static readonly int BlurSoftnessId = Shader.PropertyToID("_FaceSoftness");

        private const string OpeningTweenId = "OpeningTween";
        private Coroutine productionCoroutine;
        private bool _isPaused;

        private void Awake()
        {
                SkipBtn.onClick.AddListener(OnClickSkip);
        }

        void OnEnable()
        {
                _isPaused = false;

                if (productionCoroutine != null)
                {
                        StopCoroutine(productionCoroutine);
                }

                productionCoroutine = StartCoroutine(StartProduction());

                int isView = PlayerPrefs.GetInt("Opening_View", 0);
                if (isView == 0)
                {
                        SkipBtn.gameObject.SetActive(false);
                }
                else
                {
                        SkipBtn.gameObject.SetActive(true);
                }
        }

        private void OnDisable()
        {
                _isPaused = false;

                if (productionCoroutine != null)
                {
                        StopCoroutine(productionCoroutine);
                        productionCoroutine = null;
                }

                DOTween.Kill(OpeningTweenId);
        }

        void OnClickSkip()
        {
                PauseOpening();

                var popup = Resource_Manager.Instance.Get_Yes_Or_No();
                popup.Open();
                popup.SetPopup(LanguageManager.Instance.GetText("Opening_Skip_Title"), ()=>
                {
                        StoryManager.Instance.IsOpening = false;
                        StoryManager.Instance.EndOpening();
                        DOTween.Kill(OpeningTweenId);
                        this.gameObject.SetActive(false);
                },
                ()=>
                {
                        ResumeOpening();
                });
        }

        void PauseOpening()
        {
                _isPaused = true;
                DOTween.Pause(OpeningTweenId);
        }

        void ResumeOpening()
        {
                _isPaused = false;
                DOTween.Play(OpeningTweenId);
        }

        IEnumerator WaitPauseable(float duration)
        {
                float timer = 0f;

                while (timer < duration)
                {
                        if (!_isPaused)
                        {
                                timer += Time.deltaTime;
                        }

                        yield return null;
                }
        }

        void SetFont()
        {
                Main_Text.font = Font_Manager.Instance.GetFont(true);
                Main_Text_Sub.font = Font_Manager.Instance.GetFont(true);
                if (LanguageManager.Instance.GetCurrentLanguage() == LanguageType.KR || LanguageManager.Instance.GetCurrentLanguage() == LanguageType.EN)
                {
                        Main_Text.fontSharedMaterial = Font_Manager.Instance.GetFontMaterial(1);
                        Main_Text_Sub.fontSharedMaterial = Font_Manager.Instance.GetFontMaterial(1);
                }
                else
                {
                        Main_Text.fontSharedMaterial = Font_Manager.Instance.GetFontMaterial(4);
                        Main_Text_Sub.fontSharedMaterial = Font_Manager.Instance.GetFontMaterial(4);
                }
        }

        IEnumerator StartProduction()
        {
                StoryManager.Instance.IsOpening = true;
                SetFont();
                //정환 키보드로 마우스 휠업 등등 막기
                typing_speed_1 = 0.05f;
                Main_Text.text = "";
                Main_Text_Sub.text = "";
                Sub_Text_1.text = "";
                Sub_Text_2.text = "";

                //Main_Text.fontMaterial = new Material(Main_Text.fontMaterial);
                int warning = PlayerPrefs.GetInt("First_Warning", 0);
                if (warning == 0)
                {
                        Warning_Obj.SetActive(true);
                        Warning.DOFade(1,2f).SetId(OpeningTweenId);
                        yield return WaitPauseable(3f);

                        Warning.DOFade(0,2f).SetId(OpeningTweenId);
                        Warning_Obj_Canvas.DOFade(0,2f).SetId(OpeningTweenId);
                        yield return WaitPauseable(2f);
                        Warning_Obj.SetActive(false);
                        PlayerPrefs.SetInt("First_Warning", 1);
                }

                yield return WaitPauseable(1f);
                

                //20살 생일.
                Main_Text.color = _offColor;
                Main_Text.text = LanguageManager.Instance.GetText("opening_1");
                //Main_Text.fontMaterial.SetFloat(BlurSoftnessId, 0);
                Main_Text.DOFade(1, 0.5f).SetId(OpeningTweenId);

                yield return WaitPauseable(1.2f);
                Main_Text.DOFade(0, 0.5f).SetId(OpeningTweenId);
                yield return WaitPauseable(0.8f);
                Main_Text.text = "";
                //----------------------------------------------------------------------------------------------------
                //그날, 갑작스러운 사고로
                Main_Text.text = LanguageManager.Instance.GetText("opening_2") + "\n" + LanguageManager.Instance.GetText("opening_3");
                //Main_Text.fontMaterial.SetFloat(BlurSoftnessId, 1);
                Main_Text.DOFade(1, 0.5f).SetId(OpeningTweenId);
                // DOTween.To(() => Main_Text.fontMaterial.GetFloat(BlurSoftnessId),
                //     x => Main_Text.fontMaterial.SetFloat(BlurSoftnessId, x), 0f, 0.45f).SetId(OpeningTweenId);
                yield return WaitPauseable(2f);
                Main_Text.DOFade(0, 0.5f).SetId(OpeningTweenId);
                yield return WaitPauseable(0.8f);
                Main_Text.text = "";
                //----------------------------------------------------------------------------------------------------
                //부모님이 돌아가셨다
                // Main_Text.text = LanguageManager.Instance.GetText("opening_3");
                // Main_Text.fontMaterial.SetFloat(BlurSoftnessId, 1);
                // Main_Text.DOFade(1, 2f).SetId(OpeningTweenId);
                // DOTween.To(() => Main_Text.fontMaterial.GetFloat(BlurSoftnessId),
                //     x => Main_Text.fontMaterial.SetFloat(BlurSoftnessId, x), 0f, 2f).SetId(OpeningTweenId);
                // yield return WaitPauseable(3.8f);
                // Main_Text.DOFade(0, 1.5f).SetId(OpeningTweenId);
                // yield return WaitPauseable(1.5f);
                // Main_Text.text = "";
                //----------------------------------------------------------------------------------------------------
                // 마치 준비해 둔 것처럼—
                //yield return WaitPauseable(0.5f);
                //Main_Text.fontMaterial.SetFloat(BlurSoftnessId, 0);
                Main_Text.text = LanguageManager.Instance.GetText("opening_4") + "\n" + LanguageManager.Instance.GetText("opening_5");
                Main_Text.DOFade(1, 0.5f).SetId(OpeningTweenId);
                yield return WaitPauseable(2f);
                Main_Text.DOFade(0, 0.5f).SetId(OpeningTweenId);
                yield return WaitPauseable(0.8f);
                Main_Text.text = "";
                //----------------------------------------------------------------------------------------------------
                // 한 장의 유서가 남겨져 있었다.
                // Main_Text.text = LanguageManager.Instance.GetText("opening_5");
                // //SoundManager.Instance.PlaySFX("Sfx_Opening_2");
                // Main_Text.DOFade(1, 2f).SetId(OpeningTweenId);
                // // 종이 넘기는 소리 fade complete 후에
                // // SoundManager.Instance.PlaySFX("paper_flip");
                // yield return WaitPauseable(3.5f);
                // Main_Text.DOFade(0, 1.5f).SetId(OpeningTweenId);
                // yield return WaitPauseable(1.5f);
                // Main_Text.text = "";
                //----------------------------------------------------------------------------------------------------
                // 어릴 적 불치병에 걸려 죽어가던 나를
                Main_Text.text = LanguageManager.Instance.GetText("opening_6") + "\n" + LanguageManager.Instance.GetText("opening_7");
                Main_Text.DOFade(1, 0.5f).SetId(OpeningTweenId);
                yield return WaitPauseable(2.2f);
                Main_Text.DOFade(0, 0.5f).SetId(OpeningTweenId);
                yield return WaitPauseable(0.8f);
                Main_Text.text = "";
                //----------------------------------------------------------------------------------------------------
                // 성물의 힘으로 살려냈다는 이야기와,
                // Main_Text.text = LanguageManager.Instance.GetText("opening_7");
                // Main_Text.DOFade(1, 2f).SetId(OpeningTweenId);
                // yield return WaitPauseable(3.5f);
                // Main_Text.DOFade(0, 1.5f).SetId(OpeningTweenId);
                // yield return WaitPauseable(1.5f);
                // Main_Text.text = "";
                //----------------------------------------------------------------------------------------------------
                // 나를 살리기 위해
                Main_Text.text = LanguageManager.Instance.GetText("opening_8") + "\n" + LanguageManager.Instance.GetText("opening_9"); ;
                // var lang = LanguageManager.Instance.GetCurrentLanguage();
                // if (lang == LanguageType.KR || lang == LanguageType.EN || lang == LanguageType.JA)
                // {
                //         Main_Text_Sub.text = LanguageManager.Instance.GetText("opening_8");
                //         Main_Text_Sub.fontSize = 82f;
                //         DOTween.To(() => Main_Text_Sub.fontSize, x => Main_Text_Sub.fontSize = x, 50f, 2f).SetId(OpeningTweenId);
                // }

                Main_Text.DOFade(1, 0.5f).SetId(OpeningTweenId);
                yield return WaitPauseable(2.2f);
                //Main_Text_Sub.text = "";
                Main_Text.DOFade(0, 0.5f).SetId(OpeningTweenId);
                yield return WaitPauseable(0.8f);
                Main_Text.text = "";
                //----------------------------------------------------------------------------------------------------
                // 계속해서 성물을 훔쳐왔다는 사실이 적혀 있었다.
                // Main_Text.text = LanguageManager.Instance.GetText("opening_9");
                // Main_Text.DOFade(1, 2f).SetId(OpeningTweenId);
                // yield return WaitPauseable(3.5f);
                // Main_Text.DOFade(0, 1.5f).SetId(OpeningTweenId);
                // yield return WaitPauseable(1.5f);
                // Main_Text.text = "";
                //----------------------------------------------------------------------------------------------------
                //그리고 마지막으로,
                Main_Text.text = LanguageManager.Instance.GetText("opening_10") + "\n" + LanguageManager.Instance.GetText("opening_11");
                Main_Text.DOFade(1, 0.5f).SetId(OpeningTweenId);
                yield return WaitPauseable(1.9f);
                Main_Text.DOFade(0, 0.5f).SetId(OpeningTweenId);
                yield return WaitPauseable(0.8f);
                Main_Text.text = "";
                //----------------------------------------------------------------------------------------------------
                //내가 계속 살아가기 위해서는—
                // Main_Text.text = LanguageManager.Instance.GetText("opening_11");
                // Main_Text.DOFade(1, 2.5f).SetId(OpeningTweenId);
                // yield return WaitPauseable(4.5f);
                // Main_Text.DOFade(0, 1.5f).SetId(OpeningTweenId);
                // yield return WaitPauseable(1.5f);
                // Main_Text.text = "";
                //----------------------------------------------------------------------------------------------------
                //신들의 성물을 훔쳐
                Main_Text.text = LanguageManager.Instance.GetText("opening_12") + "\n" + LanguageManager.Instance.GetText("opening_13");
                Main_Text.DOFade(1, 0.5f).SetId(OpeningTweenId);
                yield return WaitPauseable(2.5f);
                Main_Text.DOFade(0, 0.5f).SetId(OpeningTweenId);
                yield return WaitPauseable(0.8f);
                Main_Text.text = "";
                //----------------------------------------------------------------------------------------------------
                //그 안에 담긴 신력을 흡수해야만 한다는 말.
                // Main_Text.text = LanguageManager.Instance.GetText("opening_13");
                // Main_Text.DOFade(1, 2f).SetId(OpeningTweenId);
                // yield return WaitPauseable(4f);
                // Main_Text.DOFade(0, 1.5f).SetId(OpeningTweenId);
                // yield return WaitPauseable(2.5f);
                // Main_Text.text = "";
                //----------------------------------------------------------------------------------------------------
                //편지를 읽는 순간,
                Main_Text.text = LanguageManager.Instance.GetText("opening_14") + "\n" + LanguageManager.Instance.GetText("opening_15");
                // DOTween.To(() => Main_Text.fontMaterial.GetFloat(BlurSoftnessId),
                //     x => Main_Text.fontMaterial.SetFloat(BlurSoftnessId, x), 0f, 0.4f).SetId(OpeningTweenId);
                Main_Text.DOFade(1, 0.5f).SetId(OpeningTweenId);
                yield return WaitPauseable(2f);
                Main_Text.DOFade(0, 0.5f).SetId(OpeningTweenId);
                yield return WaitPauseable(0.8f);
                Main_Text.text = "";
                //----------------------------------------------------------------------------------------------------
                //죄책감이 밀려왔다.
                // Main_Text.text = LanguageManager.Instance.GetText("opening_15");
                // Main_Text.fontMaterial.SetFloat(BlurSoftnessId, 1);
                // //SoundManager.Instance.PlaySFX("Sfx_Opening_3");
                // Main_Text.DOFade(1, 2f).SetId(OpeningTweenId);
                // DOTween.To(() => Main_Text.fontMaterial.GetFloat(BlurSoftnessId),
                //     x => Main_Text.fontMaterial.SetFloat(BlurSoftnessId, x), 0f, 2f).SetId(OpeningTweenId);
                // //complete 이후 피아노 소리 단음 하나 어떤음? 띵?
                // yield return WaitPauseable(4f);
                // Main_Text.DOFade(0, 2f).SetId(OpeningTweenId);
                // yield return WaitPauseable(3f);
                // Main_Text.text = "";
                //----------------------------------------------------------------------------------------------------
                //하지만—
                Main_Text.text = LanguageManager.Instance.GetText("opening_16");
                Main_Text.DOFade(1, 0.5f).SetId(OpeningTweenId);
                yield return WaitPauseable(1.2f);
                Main_Text.DOFade(0, 0.5f).SetId(OpeningTweenId);
                yield return WaitPauseable(0.8f);
                Main_Text.text = "";
                //----------------------------------------------------------------------------------------------------
                //죽고 싶지 않았다.
                Main_Text.text = LanguageManager.Instance.GetText("opening_17");
                //Main_Text.fontMaterial.SetFloat(BlurSoftnessId, 1);
                Main_Text.DOFade(1, 0.5f).SetId(OpeningTweenId);
                // DOTween.To(() => Main_Text.fontMaterial.GetFloat(BlurSoftnessId),
                //     x => Main_Text.fontMaterial.SetFloat(BlurSoftnessId, x), 0f, 0.5f).SetId(OpeningTweenId);
                yield return WaitPauseable(2.2f);
                Main_Text.DOFade(0, 0.5f).SetId(OpeningTweenId);
                yield return WaitPauseable(0.8f);
                Main_Text.text = "";
                Main_Text.ForceMeshUpdate();
                //----------------------------------------------------------------------------------------------------
                //그날 이후
                Main_Text.text = LanguageManager.Instance.GetText("opening_18") + "\n" + LanguageManager.Instance.GetText("opening_19");
                Main_Text.DOFade(1, 0.5f).SetId(OpeningTweenId).OnComplete(() => SoundManager.Instance.PlayStorySFX("Forest_5", -1, 5f));
                MainBG.DOFade(1f, 8f).SetEase(Ease.InCubic).SetId(OpeningTweenId);
                
                //Main_Text.DOFade(1, 0.4f).SetId(OpeningTweenId);
                yield return WaitPauseable(1.5f);
                Main_Text.DOFade(0, 0.5f).SetId(OpeningTweenId);
                yield return WaitPauseable(0.8f);
                Main_Text.text = "";
                //----------------------------------------------------------------------------------------------------
                //나는 떠돌며 살아간다.
                // Main_Text.text = LanguageManager.Instance.GetText("opening_19");
                // //(() => SoundManager.Instance.PlayOpening("Forest_5", 5f, true));
                // //complete 숲소리, 벌레소리,바람소리
                // yield return WaitPauseable(4f);
                // Main_Text.DOFade(0, 1.5f).SetId(OpeningTweenId);
                // yield return WaitPauseable(1.5f);
                // Main_Text.text = "";
                //----------------------------------------------------------------------------------------------------
                //신들의 물건을 훔치면서.
                Main_Text.text = LanguageManager.Instance.GetText("opening_20");
                Main_Text.DOFade(1, 0.5f).SetId(OpeningTweenId);
                
                yield return WaitPauseable(2f);
                Main_Text.DOFade(0, 0.5f).SetId(OpeningTweenId);
                yield return WaitPauseable(0.8f);
                Main_Text.text = "";
                //----------------------------------------------------------------------------------------------------
                //훔치지 않으면—
                Main_Text.text = LanguageManager.Instance.GetText("opening_21");
                Main_Text.DOFade(1, 0.5f).SetId(OpeningTweenId);
                yield return WaitPauseable(1.3f);
                Main_Text.DOFade(0, 0.5f).SetId(OpeningTweenId);
                yield return WaitPauseable(0.8f);
                Main_Text.text = "";
                //----------------------------------------------------------------------------------------------------
                //나는, 죽는다.
                Main_Text.text = LanguageManager.Instance.GetText("opening_22");
                Main_Text.DOFade(1, 0.5f).SetId(OpeningTweenId);
                yield return WaitPauseable(1.5f);
                //SoundManager.Instance.StopOpening(1.5f);
                Main_Text.DOFade(0, 0.5f).SetId(OpeningTweenId);
                yield return WaitPauseable(0.7f);
                Main_Text.text = "";
                StoryManager.Instance.IsOpening = false;
                PlayerPrefs.SetInt("Opening_View", 1);
                CanvasGroup.DOFade(0, 0.7f).SetId(OpeningTweenId).OnComplete(() =>
                {
                        this.gameObject.SetActive(false);
                        StoryManager.Instance.EndOpening();
                });
        }

        #region Utility
        readonly string[] chosung =
        {
        "ㄱ","ㄲ","ㄴ","ㄷ","ㄸ","ㄹ","ㅁ","ㅂ","ㅃ","ㅅ",
        "ㅆ","ㅇ","ㅈ","ㅉ","ㅊ","ㅋ","ㅌ","ㅍ","ㅎ"
    };

        readonly string[] jungsung =
        {
        "ㅏ","ㅐ","ㅑ","ㅒ","ㅓ","ㅔ","ㅕ","ㅖ","ㅗ","ㅘ",
        "ㅙ","ㅚ","ㅛ","ㅜ","ㅝ","ㅞ","ㅟ","ㅠ","ㅡ","ㅢ","ㅣ"
    };

        readonly string[] jongsung =
        {
        "","ㄱ","ㄲ","ㄳ","ㄴ","ㄵ","ㄶ","ㄷ","ㄹ","ㄺ",
        "ㄻ","ㄼ","ㄽ","ㄾ","ㄿ","ㅀ","ㅁ","ㅂ","ㅄ","ㅅ",
        "ㅆ","ㅇ","ㅈ","ㅊ","ㅋ","ㅌ","ㅍ","ㅎ"
    };

        Coroutine typingCoroutine;

        IEnumerator TypeRoutine(string text, TextMeshProUGUI targetText)
        {
                targetText.text = "";

                string completeText = "";

                foreach (char c in text)
                {
                        while (_isPaused)
                        {
                                yield return null;
                        }

                        if (IsKorean(c))
                        {
                                yield return StartCoroutine(TypeKoreanChar(completeText, c, targetText));
                        }

                        completeText += c;
                        targetText.text = completeText;

                        yield return WaitPauseable(typing_speed_1);
                }
        }

        IEnumerator TypeKoreanChar(string prefix, char c, TextMeshProUGUI targetText)
        {
                int unicode = c - 0xAC00;

                int cho = unicode / (21 * 28);
                int jung = (unicode % (21 * 28)) / 28;
                int jong = unicode % 28;

                // 초성
                targetText.text = prefix + chosung[cho];
                yield return WaitPauseable(typing_speed_1);

                // 초성 + 중성
                char midChar = (char)(0xAC00 + (cho * 21 + jung) * 28);
                targetText.text = prefix + midChar;
                yield return WaitPauseable(typing_speed_1);

                // 초성 + 중성 + 종성
                if (jong != 0)
                {
                        targetText.text = prefix + c;
                        yield return WaitPauseable(typing_speed_1);
                }
        }

        bool IsKorean(char c)
        {
                return c >= 0xAC00 && c <= 0xD7A3;
        }
        #endregion
}