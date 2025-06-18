using MagicalGarden.Shop;
using UnityEngine;

namespace MagicalGarden.Manager
{    
    public class ShopManager : MonoBehaviour {
        public Transform content;
        public GameObject itemShop;
        private void Start()
        {
            GSheetManager.Instance.OnDataLoaded += OnDataReady;
        }

        void OnDataReady()
        {
            var items = GSheetManager.Instance.itemList;
            foreach (var item in items)
            {
                // Debug.Log($"{item.seedName} - Harga: {item.seedPrice}");
                var prefabItem = Instantiate(itemShop);
                prefabItem.GetComponent<ItemShop>().Setup(item, null);
                prefabItem.transform.parent = content;
            }
        }
    }
}
