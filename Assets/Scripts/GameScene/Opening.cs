using System.Collections;
using TMPro;
using UnityEngine;
using DG.Tweening;
using System;

public class Opening : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI Main_Text;
    [SerializeField] TextMeshProUGUI Sub_Text_1;
    [SerializeField] TextMeshProUGUI Sub_Text_2;

    float typing_speed_1 = 0.08f;
    Color _offColor = new Color(1f, 1f, 1f, 0f);
    void OnEnable()
    {
        StartCoroutine(StartProduction());
    }

    IEnumerator StartProduction()
    {
        typing_speed_1 = 0.05f;
        Main_Text.text = "";
        Sub_Text_1.text = "";
        Sub_Text_2.text = "";

        yield return new WaitForSeconds(3f);

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        //20살 생일.
        yield return StartCoroutine(TypeRoutine(LanguageManager.Instance.GetText("opening_1"), Main_Text));
        
        yield return new WaitForSeconds(2f);
        Main_Text.text = "";

        //그날, 갑작스러운 사고로
        yield return StartCoroutine(TypeRoutine(LanguageManager.Instance.GetText("opening_2"), Sub_Text_1));
        yield return new WaitForSeconds(2f);

        //부모님이 돌아가셨다
        yield return StartCoroutine(TypeRoutine(LanguageManager.Instance.GetText("opening_3"), Sub_Text_2));
        yield return new WaitForSeconds(2f);
        Sub_Text_1.text = "";
        Sub_Text_2.text = "";

        // 마치 준비해 둔 것처럼—
        yield return StartCoroutine(TypeRoutine(LanguageManager.Instance.GetText("opening_4"), Main_Text));
        yield return new WaitForSeconds(1.2f);
        Main_Text.text = "";

        // 한 장의 유서가 남겨져 있었다.
        yield return StartCoroutine(TypeRoutine(LanguageManager.Instance.GetText("opening_5"), Main_Text));

        // 종이 넘기는 소리
        // SoundManager.Instance.PlaySFX("paper_flip");

        yield return new WaitForSeconds(2f);
        Main_Text.text = "";
        typing_speed_1 = 0.1f;

        // 어릴 적 불치병에 걸려 죽어가던 나를
        yield return StartCoroutine(TypeRoutine(LanguageManager.Instance.GetText("opening_6"), Main_Text));
        yield return new WaitForSeconds(1.2f);
        Main_Text.text = "";

        // 성물의 힘으로 살려냈다는 이야기와,
        yield return StartCoroutine(TypeRoutine(LanguageManager.Instance.GetText("opening_7"), Main_Text));
        yield return new WaitForSeconds(1.5f);
        Main_Text.text = "";

        // 나를 살리기 위해
        yield return StartCoroutine(TypeRoutine(LanguageManager.Instance.GetText("opening_8"), Sub_Text_1));
        yield return new WaitForSeconds(0.8f);

        // 계속해서 성물을 훔쳐왔다는 사실이 적혀 있었다.
        yield return StartCoroutine(TypeRoutine(LanguageManager.Instance.GetText("opening_9"), Sub_Text_2));
        yield return new WaitForSeconds(2f);

        Sub_Text_1.text = "";
        Sub_Text_2.text = "";

        //그리고 마지막으로,
        //내가 계속 살아가기 위해서는—
        //신들의 성물을 훔쳐
        //그 안에 담긴 신력을 흡수해야만 한다는 말.
        //편지를 읽는 순간,
        //죄책감이 밀려왔다.
        //하지만—
        //죽고 싶지 않았다.
        //그날 이후
        //나는 떠돌며 살아간다.
        //신들의 물건을 훔치면서.
        //훔치지 않으면—
        //나는, 죽는다.
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
