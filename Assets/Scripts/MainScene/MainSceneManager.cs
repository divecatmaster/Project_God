using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;

public class MainSceneManager : MonoBehaviour
{
    [SerializeField] Transform Popup_Trans;
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

    Popup_Save _popup_Save;
    public void OnClickLoad()
    {
        if (_popup_Save == null)
        {
            var target = Resources.Load<GameObject>("Popup/Popup_Save");
            if (target != null)
            {
                var item = Instantiate(target, Popup_Trans);
                _popup_Save = item.GetComponent<Popup_Save>();
            }
        }
        _popup_Save.SetPopup(0);
        _popup_Save.Open();
    }
}
