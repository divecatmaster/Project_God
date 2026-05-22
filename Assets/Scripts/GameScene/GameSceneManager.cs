using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class GameSceneManager : MonoBehaviour
{
    [SerializeField] Image LoadingDim;
    void OnEnable()
    {
        EndLoading();
    }

    void EndLoading()
    {
        LoadingDim.DOFade(0f, 1f).OnComplete(()=>
        {
            LoadingDim.raycastTarget = false;
        });
    }
}
