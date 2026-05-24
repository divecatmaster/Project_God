using System.Collections;
using TMPro;
using UnityEngine;
using DG.Tweening;
using System;
using UnityEngine.UI;
using God.Audio;

public class Opening : MonoBehaviour
{
        [SerializeField] TextMeshProUGUI Main_Text;
        [SerializeField] TextMeshProUGUI Main_Text_Sub;
        [SerializeField] Image MainBG;
        [SerializeField] TextMeshProUGUI Sub_Text_1;
        [SerializeField] TextMeshProUGUI Sub_Text_2;

        float typing_speed_1 = 0.08f;
        Color _offColor = new Color(1f, 1f, 1f, 0f);
        private static readonly int BlurSoftnessId = Shader.PropertyToID("_FaceSoftness");
        void OnEnable()
        {
                StartCoroutine(StartProduction());
        }

        IEnumerator StartProduction()
        {
                typing_speed_1 = 0.05f;
                Main_Text.text = "";
                Main_Text_Sub.text = "";
                Sub_Text_1.text = "";
                Sub_Text_2.text = "";

                Main_Text.fontMaterial = new Material(Main_Text.fontMaterial);
                yield return new WaitForSeconds(1f);

                //20살 생일.
                Main_Text.color = _offColor;
                Main_Text.text = LanguageManager.Instance.GetText("opening_1");
                Main_Text.fontMaterial.SetFloat(BlurSoftnessId, 0);
                Main_Text.DOFade(1, 2f);

                yield return new WaitForSeconds(3.5f);
                Main_Text.DOFade(0, 2f);
                yield return new WaitForSeconds(2f);
                Main_Text.text = "";
                //----------------------------------------------------------------------------------------------------
                //그날, 갑작스러운 사고로
                Main_Text.text = LanguageManager.Instance.GetText("opening_2");
                Main_Text.fontMaterial.SetFloat(BlurSoftnessId, 1);
                Main_Text.DOFade(1, 2f);
                DOTween.To(() => Main_Text.fontMaterial.GetFloat(BlurSoftnessId),
                    x => Main_Text.fontMaterial.SetFloat(BlurSoftnessId, x), 0f, 2f);
                yield return new WaitForSeconds(3.8f);
                Main_Text.DOFade(0, 2f);
                yield return new WaitForSeconds(2f);
                Main_Text.text = "";
                //----------------------------------------------------------------------------------------------------
                //부모님이 돌아가셨다
                Main_Text.text = LanguageManager.Instance.GetText("opening_3");
                Main_Text.fontMaterial.SetFloat(BlurSoftnessId, 1);
                Main_Text.DOFade(1, 2f);
                DOTween.To(() => Main_Text.fontMaterial.GetFloat(BlurSoftnessId),
                    x => Main_Text.fontMaterial.SetFloat(BlurSoftnessId, x), 0f, 2f);
                yield return new WaitForSeconds(3.8f);
                Main_Text.DOFade(0, 2f);
                yield return new WaitForSeconds(3f);
                Main_Text.text = "";
                //----------------------------------------------------------------------------------------------------
                // 마치 준비해 둔 것처럼—
                yield return new WaitForSeconds(0.5f);
                Main_Text.fontMaterial.SetFloat(BlurSoftnessId, 0);
                Main_Text.text = LanguageManager.Instance.GetText("opening_4");
                Main_Text.DOFade(1, 2f);
                yield return new WaitForSeconds(3.8f);
                Main_Text.DOFade(0, 2f);
                yield return new WaitForSeconds(2f);
                Main_Text.text = "";
                //----------------------------------------------------------------------------------------------------
                // 한 장의 유서가 남겨져 있었다.
                Main_Text.text = LanguageManager.Instance.GetText("opening_5");
                SoundManager.Instance.PlaySFX("Sfx_Opening_2");
                Main_Text.DOFade(1, 2f);
                // 종이 넘기는 소리 fade complete 후에
                // SoundManager.Instance.PlaySFX("paper_flip");
                yield return new WaitForSeconds(4f);
                Main_Text.DOFade(0, 2f);
                yield return new WaitForSeconds(2f);
                Main_Text.text = "";
                //----------------------------------------------------------------------------------------------------
                // 어릴 적 불치병에 걸려 죽어가던 나를
                Main_Text.text = LanguageManager.Instance.GetText("opening_6");
                Main_Text.DOFade(1, 2f);
                yield return new WaitForSeconds(4f);
                Main_Text.DOFade(0, 2f);
                yield return new WaitForSeconds(2f);
                Main_Text.text = "";
                //----------------------------------------------------------------------------------------------------
                // 성물의 힘으로 살려냈다는 이야기와,
                Main_Text.text = LanguageManager.Instance.GetText("opening_7");
                Main_Text.DOFade(1, 2f);
                yield return new WaitForSeconds(4f);
                Main_Text.DOFade(0, 2f);
                yield return new WaitForSeconds(2f);
                Main_Text.text = "";
                //----------------------------------------------------------------------------------------------------
                // 나를 살리기 위해
                Main_Text.text = LanguageManager.Instance.GetText("opening_8");
                var lang = LanguageManager.Instance.GetCurrentLanguage();
                if (lang == LanguageType.KR || lang == LanguageType.EN || lang == LanguageType.JA)
                {
                        Main_Text_Sub.text = LanguageManager.Instance.GetText("opening_8");
                        Main_Text_Sub.fontSize = 82f;
                        DOTween.To(() => Main_Text_Sub.fontSize, x => Main_Text_Sub.fontSize = x, 67f, 2f);
                }


                Main_Text.DOFade(1, 2f);
                yield return new WaitForSeconds(4f);
                Main_Text_Sub.text = "";
                Main_Text.DOFade(0, 2f);
                yield return new WaitForSeconds(3f);
                Main_Text.text = "";
                //----------------------------------------------------------------------------------------------------
                // 계속해서 성물을 훔쳐왔다는 사실이 적혀 있었다.
                Main_Text.text = LanguageManager.Instance.GetText("opening_9");
                Main_Text.DOFade(1, 2f);
                yield return new WaitForSeconds(4f);
                Main_Text.DOFade(0, 2f);
                yield return new WaitForSeconds(2f);
                Main_Text.text = "";
                //----------------------------------------------------------------------------------------------------
                //그리고 마지막으로,
                Main_Text.text = LanguageManager.Instance.GetText("opening_10");
                Main_Text.DOFade(1, 2f);
                yield return new WaitForSeconds(4f);
                Main_Text.DOFade(0, 2f);
                yield return new WaitForSeconds(3f);
                Main_Text.text = "";
                //----------------------------------------------------------------------------------------------------
                //내가 계속 살아가기 위해서는—
                Main_Text.text = LanguageManager.Instance.GetText("opening_11");
                Main_Text.DOFade(1, 2.5f);
                yield return new WaitForSeconds(4.5f);
                Main_Text.DOFade(0, 2f);
                yield return new WaitForSeconds(2f);
                Main_Text.text = "";
                //----------------------------------------------------------------------------------------------------
                //신들의 성물을 훔쳐
                Main_Text.text = LanguageManager.Instance.GetText("opening_12");
                Main_Text.DOFade(1, 2f);
                yield return new WaitForSeconds(4f);
                Main_Text.DOFade(0, 2f);
                yield return new WaitForSeconds(2f);
                Main_Text.text = "";
                //----------------------------------------------------------------------------------------------------
                //그 안에 담긴 신력을 흡수해야만 한다는 말.
                Main_Text.text = LanguageManager.Instance.GetText("opening_13");
                Main_Text.DOFade(1, 2f);
                yield return new WaitForSeconds(4f);
                Main_Text.DOFade(0, 2f);
                yield return new WaitForSeconds(3f);
                Main_Text.text = "";
                //----------------------------------------------------------------------------------------------------
                //편지를 읽는 순간,
                Main_Text.text = LanguageManager.Instance.GetText("opening_14");
                Main_Text.DOFade(1, 2f);
                yield return new WaitForSeconds(4f);
                Main_Text.DOFade(0, 2f);
                yield return new WaitForSeconds(2f);
                Main_Text.text = "";
                //----------------------------------------------------------------------------------------------------
                //죄책감이 밀려왔다.
                Main_Text.text = LanguageManager.Instance.GetText("opening_15");
                Main_Text.fontMaterial.SetFloat(BlurSoftnessId, 1);
                SoundManager.Instance.PlaySFX("Sfx_Opening_3");
                Main_Text.DOFade(1, 2f);
                DOTween.To(() => Main_Text.fontMaterial.GetFloat(BlurSoftnessId),
                    x => Main_Text.fontMaterial.SetFloat(BlurSoftnessId, x), 0f, 2f);
                //complete 이후 피아노 소리 단음 하나 어떤음? 띵?
                yield return new WaitForSeconds(4f);
                Main_Text.DOFade(0, 2f);
                yield return new WaitForSeconds(3f);
                Main_Text.text = "";
                //----------------------------------------------------------------------------------------------------
                //하지만—
                Main_Text.text = LanguageManager.Instance.GetText("opening_16");
                Main_Text.DOFade(1, 2.5f);
                yield return new WaitForSeconds(4.5f);
                Main_Text.DOFade(0, 2f);
                yield return new WaitForSeconds(3f);
                Main_Text.text = "";
                //----------------------------------------------------------------------------------------------------
                //죽고 싶지 않았다.
                Main_Text.text = LanguageManager.Instance.GetText("opening_17");
                Main_Text.fontMaterial.SetFloat(BlurSoftnessId, 1);
                Main_Text.DOFade(1, 3f);
                DOTween.To(() => Main_Text.fontMaterial.GetFloat(BlurSoftnessId),
                    x => Main_Text.fontMaterial.SetFloat(BlurSoftnessId, x), 0f, 3f);
                yield return new WaitForSeconds(5.5f);
                Main_Text.DOFade(0, 2f);
                yield return new WaitForSeconds(2f);
                Main_Text.text = "";
                Main_Text.ForceMeshUpdate();
                //----------------------------------------------------------------------------------------------------
                //그날 이후
                Main_Text.text = LanguageManager.Instance.GetText("opening_18");
                Main_Text.DOFade(1, 2f);
                yield return new WaitForSeconds(4f);
                Main_Text.DOFade(0, 2f);
                yield return new WaitForSeconds(2f);
                Main_Text.text = "";
                //----------------------------------------------------------------------------------------------------
                //나는 떠돌며 살아간다.
                Main_Text.text = LanguageManager.Instance.GetText("opening_19");
                Main_Text.DOFade(1, 2f).OnComplete(() => SoundManager.Instance.PlayOpening("Sfx_Opening_1", 5f, true));
                //complete 숲소리, 벌레소리,바람소리
                yield return new WaitForSeconds(4f);
                Main_Text.DOFade(0, 2f);
                yield return new WaitForSeconds(2f);
                Main_Text.text = "";
                //----------------------------------------------------------------------------------------------------
                //신들의 물건을 훔치면서.
                Main_Text.text = LanguageManager.Instance.GetText("opening_20");
                Main_Text.DOFade(1, 2f);
                MainBG.DOFade(1f, 20f).SetEase(Ease.InCubic);
                yield return new WaitForSeconds(4f);
                Main_Text.DOFade(0, 2f);
                yield return new WaitForSeconds(2f);
                Main_Text.text = "";
                //----------------------------------------------------------------------------------------------------
                //훔치지 않으면—
                Main_Text.text = LanguageManager.Instance.GetText("opening_21");
                Main_Text.DOFade(1, 2f);
                yield return new WaitForSeconds(4f);
                Main_Text.DOFade(0, 2f);
                yield return new WaitForSeconds(2f);
                Main_Text.text = "";
                //----------------------------------------------------------------------------------------------------
                //나는, 죽는다.
                Main_Text.text = LanguageManager.Instance.GetText("opening_22");
                Main_Text.DOFade(1, 2f);
                yield return new WaitForSeconds(4f);
                SoundManager.Instance.StopOpening(1.5f);
                Main_Text.DOFade(0, 2f);
                yield return new WaitForSeconds(2f);
                Main_Text.text = "";
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
                        if (IsKorean(c))
                        {
                                yield return StartCoroutine(TypeKoreanChar(completeText, c, targetText));
                        }

                        completeText += c;
                        targetText.text = completeText;

                        yield return new WaitForSeconds(typing_speed_1);
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
                yield return new WaitForSeconds(typing_speed_1);

                // 초성 + 중성
                char midChar = (char)(0xAC00 + (cho * 21 + jung) * 28);
                targetText.text = prefix + midChar;
                yield return new WaitForSeconds(typing_speed_1);

                // 초성 + 중성 + 종성
                if (jong != 0)
                {
                        targetText.text = prefix + c;
                        yield return new WaitForSeconds(typing_speed_1);
                }
        }

        bool IsKorean(char c)
        {
                return c >= 0xAC00 && c <= 0xD7A3;
        }
        #endregion
}
