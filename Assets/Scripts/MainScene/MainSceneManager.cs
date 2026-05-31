using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;
using DiveCat.God.UI.Popups;

public class MainSceneManager : MonoBehaviour
{
    public static MainSceneManager Instance;
    [SerializeField] Transform Popup_Trans;
    [SerializeField] Image LoadingDim;
    public static string nextScene;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void OnClickNewGame()
    {
        Data_Manager.Instance.SetNewGame();    
        LoadingDim.raycastTarget = true;
        LoadingDim.DOFade(1f, 2f).SetEase(Ease.Linear).OnComplete(() =>
        {
            PopupManager.Instance.CloseAllPopupsFast();
            nextScene = "GameScene";
            SceneManager.LoadScene("LoadingScene");
        });
    }

    public void OnClickGame()
    {
        LoadingDim.raycastTarget = true;
        LoadingDim.DOFade(1f, 2f).SetEase(Ease.Linear).OnComplete(() =>
        {
            PopupManager.Instance.CloseAllPopupsFast();
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
