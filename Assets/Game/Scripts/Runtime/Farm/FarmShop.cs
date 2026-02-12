using UnityEngine;
using UnityEngine.UI;
using MagicalGarden.Farm;
using DG.Tweening;

public class FarmShop : MonoBehaviour {

    public static FarmShop instance;

    [Header("Main Display")]
    [SerializeField] Image shopPanel;

    [Header("Panels")]
    [SerializeField] FarmShopMonsterPanel farmMonsterPanel;
    [SerializeField] FarmShopPlantPanel farmPlantPanel;
    [SerializeField] Button farmMonsterButton, farmPlantButton;
    [SerializeField] Button closeButton;

    public enum PanelType {Monster, Plant};
    bool loadListener;
    void Awake () {
        instance = this;    
    }
    void Start() {
        farmMonsterPanel.FarmShop = this;
        farmPlantPanel.FarmShop = this;
        //OffDisplay ();
        shopPanel.gameObject.SetActive(false);
    }

    // UIManager.cs :
    public void OnDisplay() {
        LoadListener ();
        shopPanel.gameObject.SetActive(true);
        //  MagicalGarden.Farm.UIManager.Instance.HideUIFarmBar ();
        shopPanel.transform.DOPunchScale(new Vector2(0.05f, 0.05f), 0.3f, 5);
        ShowSpecificPanel (PanelType.Monster);
    }

    // UIManager.cs :
    public void OffDisplay(bool instant = true) {

        if(instant)
        {
            shopPanel.gameObject.SetActive(false);
            return;
        }

        shopPanel.transform.DOScale(new Vector2(0.1f, 0.1f), 0.5f).SetEase(Ease.InBack).OnComplete(() =>
        {
            shopPanel.gameObject.SetActive(false);
            shopPanel.transform.localScale = Vector3.one;
        });

        // MagicalGarden.Farm.UIManager.Instance.ShowUIFarmBar ();
    }

    void ShowSpecificPanel (PanelType panelType) {
        farmMonsterPanel.HidePanel ();
        farmPlantPanel.HidePanel ();

        switch (panelType) {
            case PanelType.Monster :
                farmMonsterPanel.ShowPanel ();
                break;
            case PanelType.Plant :
                farmPlantPanel.ShowPanel ();
                break;
        }
    }

    #region Listener
    void LoadListener () {
        if (!loadListener) {
            loadListener = true;

            farmMonsterButton.onClick.AddListener(() => ShowSpecificPanel(PanelType.Monster));
            farmPlantButton.onClick.AddListener(() => ShowSpecificPanel(PanelType.Plant));

            closeButton.onClick.AddListener (() => OffDisplay(false));
        }
    }
    #endregion
}
