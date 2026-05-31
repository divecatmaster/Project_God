using DG.Tweening;
using DiveCat.God.UI.Popups;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameSceneManager : MonoBehaviour
{
    public static GameSceneManager Instance;
    [SerializeField] Image LoadingDim;
    [SerializeField] Opening OpeningObj;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        LoadingDim.raycastTarget = true;
        LoadingDim.color = Color.black;
    }

    void OnEnable()
    {
        if (Data_Manager.Instance.IsNewGame)
        {
            OpeningObj.gameObject.SetActive(true);
        }
        EndLoading();
    }

    void EndLoading()
    {
        LoadingDim.DOFade(0f, 2f).OnComplete(()=>
        {
            LoadingDim.raycastTarget = false;
        });
    }

    public void StartLoading()
    {
        LoadingDim.raycastTarget = true;
        LoadingDim.DOFade(1f, 2f).OnComplete(()=>
        {
            PopupManager.Instance.CloseAllPopupsFast();
            StoryManager.Instance.LoadGame(()=>
            {
                EndLoading();
            });
        });
    }

    public void GoToMainScene()
    {
        LoadingDim.raycastTarget = true;
        LoadingDim.DOFade(1f, 2f).OnComplete(()=>
        {
            PopupManager.Instance.CloseAllPopupsFast();
            Data_Manager.nextScene = "MainScene";
            SceneManager.LoadScene("LoadingScene");
        });
    }
}
