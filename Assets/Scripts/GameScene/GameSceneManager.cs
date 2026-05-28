using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class GameSceneManager : MonoBehaviour
{
    [SerializeField] Image LoadingDim;
    [SerializeField] Opening OpeningObj;
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
}
