using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;

public class MainSceneManager : MonoBehaviour
{
    [SerializeField] Image LoadingDim;
    [SerializeField] bool Skip_Opening;
    public static string nextScene;
    public void OnClickNewGame()
    {
        if (!Skip_Opening)
        {
            Data_Manager.Instance.SetNewGame();    
        }
        
        LoadingDim.raycastTarget = true;
        LoadingDim.DOFade(1f, 2f).SetEase(Ease.Linear).OnComplete(() =>
        {
            nextScene = "GameScene";
            SceneManager.LoadScene("LoadingScene");
        });
    }
}
