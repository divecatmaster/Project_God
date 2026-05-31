using DG.Tweening;
using DiveCat.God.UI.Popups;
using UnityEngine;
using UnityEngine.UI;

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
            PopupManager.Instance.CloseAllPopups();
            StoryManager.Instance.LoadGame(()=>
            {
                EndLoading();
            });
        });
    }
}
