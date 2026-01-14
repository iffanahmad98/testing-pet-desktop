using UnityEngine;

public abstract class FarmShopPanelBase : MonoBehaviour{
    public FarmShop FarmShop { get; set; }

    public abstract void ShowPanel();
    public abstract void HidePanel();
}
