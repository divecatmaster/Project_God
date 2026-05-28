using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;

public class MainSceneManager : MonoBehaviour
{
    [SerializeField] Image LoadingDim;
    public static string nextScene;
    public void OnClickNewGame()
    {
        Data_Manager.Instance.SetNewGame();
        LoadingDim.raycastTarget = true;
        LoadingDim.DOFade(1f, 2f).SetEase(Ease.Linear).OnComplete(() =>
        {
            nextScene = "GameScene";
            SceneManager.LoadScene("LoadingScene");
        });
    }
}
