using UnityEngine;
using UnityEngine.UI;
using MagicalGarden.Farm;

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
        OffDisplay ();
    }

    // UIManager.cs :
    public void OnDisplay() {
        LoadListener ();
        shopPanel.gameObject.SetActive(true);
      //  MagicalGarden.Farm.UIManager.Instance.HideUIFarmBar ();
        ShowSpecificPanel (PanelType.Monster);
    }

    // UIManager.cs :
    public void OffDisplay() {
        shopPanel.gameObject.SetActive(false);
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

            closeButton.onClick.AddListener (() => OffDisplay());
        }
    }
    #endregion
}
