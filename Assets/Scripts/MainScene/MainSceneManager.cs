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
        EndLoading();
    }

    void EndLoading()
    {
        LoadingDim.DOFade(0f, 2f).OnComplete(() =>
        {
            LoadingDim.raycastTarget = false;
        });
    }

    public void OnClickNewGame()
    {
        Data_Manager.Instance.SetNewGame(true);
        LoadingDim.raycastTarget = true;
        LoadingDim.DOFade(1f, 2f).SetEase(Ease.Linear).OnComplete(() =>
        {
            PopupManager.Instance.CloseAllPopupsFast();
            Data_Manager.nextScene = "GameScene";
            SceneManager.LoadScene("LoadingScene");
        });
    }

    public void OnClickGame()
    {
        Data_Manager.Instance.SetNewGame(false);
        LoadingDim.raycastTarget = true;
        LoadingDim.DOFade(1f, 2f).SetEase(Ease.Linear).OnComplete(() =>
        {
            PopupManager.Instance.CloseAllPopupsFast();
            Data_Manager.nextScene = "GameScene";
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

    Popup_Setting _popup_Setting;
    public void OnClickSetting()
    {
        if (_popup_Setting == null)
        {
            var target = Resources.Load<GameObject>("Popup/Popup_Setting");
            if (target != null)
            {
                var item = Instantiate(target, Popup_Trans);
                _popup_Setting = item.GetComponent<Popup_Setting>();
            }
        }
        _popup_Setting.Open();
    }
}
