using UnityEngine;
public enum ShopType
{
    MonsterShop, ItemShop, BiomeShop, FacilityShop, DecorationShop
}

public class RefreshShopItems : MonoBehaviour
{
    public ShopType shopType;

    void Awake()
    {
        ServiceLocator.Register(this);
    }

    void OnEnable()
    {
        switch(shopType)
        {
            case ShopType.MonsterShop:
                RefreshMonsterShop();
                break;
            case ShopType.ItemShop:
                RefreshItemShop();
                break;
            case ShopType.BiomeShop:
                RefreshBiomeShop();
                break;
            case ShopType.FacilityShop:
                RefreshFacilityShop();
                break;
            case ShopType.DecorationShop:
                RefreshDecorationShop();
                break;
        }
    }

    public void RefreshMonsterShop()
    {
        MonsterShopManager manager = ServiceLocator.Get<MonsterShopManager>();

        if(manager != null) 
            manager.RefreshItem();
    }

    public void RefreshItemShop()
    {
        ItemShopManager manager = ServiceLocator.Get<ItemShopManager>();

        if (manager != null)
            manager.RefreshItem();
    }

    public void RefreshBiomeShop()
    {
        BiomeShopManager manager = ServiceLocator.Get<BiomeShopManager>();

        if (manager != null)
            manager.RefreshItem();
    }

    public void RefreshFacilityShop()
    {
        FacilityShopManager manager = ServiceLocator.Get<FacilityShopManager>();

        if (manager != null)
            manager.RefreshItem();
    }

    public void RefreshDecorationShop()
    {
        DecorationShopManager manager = ServiceLocator.Get<DecorationShopManager>();

        if (manager != null)
            manager.RefreshItem();
    }
}
