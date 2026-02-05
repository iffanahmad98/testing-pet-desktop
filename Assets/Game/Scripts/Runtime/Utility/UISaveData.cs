
using System.Collections.Generic;
using System.Linq;

public class UISaveData 
{
    public List<ShopCardData> ItemShopCards = new();
    public List<ShopCardData> BiomeShopCards = new();
    public List<ShopCardData> DecorationShopCards = new();
    public List<ShopCardData> FacilityShopCards = new();

    public ShopCardData GetShopCardData(ShopType type, string id) => type switch
    {
        ShopType.ItemShop => ItemShopCards.FirstOrDefault(x => x.Id == id),
        ShopType.BiomeShop => BiomeShopCards.FirstOrDefault(x => x.Id == id),
        ShopType.DecorationShop => DecorationShopCards.FirstOrDefault(x => x.Id == id),
        ShopType.FacilityShop => FacilityShopCards.FirstOrDefault(x => x.Id == id),
        _ => null
    };

    public void SetShopCardsOpenState(ShopType type, string id, bool state)
    {
        switch (type)
        {
            case ShopType.ItemShop:
                ItemShopCards.Find(x => x.Id == id).IsOpened = state;
                break;
            case ShopType.BiomeShop:
                BiomeShopCards.Find(x => x.Id == id).IsOpened = state;
                break;
            case ShopType.FacilityShop:
                FacilityShopCards.Find(x => x.Id == id).IsOpened = state;
                break;
            case ShopType.DecorationShop:
                DecorationShopCards.Find(x => x.Id == id).IsOpened = state;
                break;
            default:
                break;
        }
    }

    public void SetBiomeShopCardsOpenState(List<ShopCardData> listToSearch, string id, bool state)
    {
        listToSearch.Find(x => x.Id == id).IsOpened = state;
    }
}

public class ShopCardData
{
    public string Id = string.Empty;
    public bool IsOpened = false;
}
