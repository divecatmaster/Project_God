using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class LoadingSceneManager : MonoBehaviour
{
    [SerializeField] Image BG;
    void OnEnable()
    {
        StartLoading();
    }

    public void StartLoading()
    {
        StartCoroutine(LoadSceneProcess());
    }

    IEnumerator LoadSceneProcess()
    {
        AsyncOperation op = SceneManager.LoadSceneAsync(MainSceneManager.nextScene);

        op.allowSceneActivation = false;

        while (!op.isDone)
        {
            float progress = Mathf.Clamp01(op.progress / 0.9f);

            // if (progressBar != null)
            // {
            //     progressBar.value = progress;
            // }

            // 로딩 완료
            if (op.progress >= 0.9f)
            {
                yield return new WaitForSeconds(0.5f);

                op.allowSceneActivation = true;
            }
            yield return null;
        }
    }
}
