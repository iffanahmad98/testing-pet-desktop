using UnityEngine;

public class RefreshShopItems : MonoBehaviour
{
    public enum ShopType
    {
        MonsterShop, ItemShop, BiomeShop, FacilityShop, DecorationShop
    }
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

        manager.RefreshItem();
    }

    public void RefreshItemShop()
    {
        ItemShopManager manager = ServiceLocator.Get<ItemShopManager>();

        manager.RefreshItem();
    }

    public void RefreshBiomeShop()
    {
        BiomeShopManager manager = ServiceLocator.Get<BiomeShopManager>();

        manager.RefreshItem();
    }

    public void RefreshFacilityShop()
    {
        FacilityShopManager manager = ServiceLocator.Get<FacilityShopManager>();

        manager.RefreshItem();
    }

    public void RefreshDecorationShop()
    {
        DecorationShopManager manager = ServiceLocator.Get<DecorationShopManager>();

        manager.RefreshItem();
    }
}
