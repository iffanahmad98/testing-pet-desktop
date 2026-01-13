using UnityEngine;
using UnityEngine.UI;

public class FarmShopMonsterPanel : FarmShopPanelBase {
    [SerializeField] Transform parentPanel;
    [SerializeField] Image panel;
    [SerializeField] Sprite onPanel, offPanel;
    public override void ShowPanel() {
        panel.sprite = onPanel;
        panel.transform.SetSiblingIndex(parentPanel.childCount - 1);
    }

    public override void HidePanel() {
        panel.sprite = offPanel;
    }
}
