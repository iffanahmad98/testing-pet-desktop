using MagicalGarden.Shop;
using UnityEngine;
using MagicalGarden.Inventory;
using System.Linq;
using System.Collections.Generic;

namespace MagicalGarden.Manager
{    
    public class ShopManager : MonoBehaviour {
        public Transform content;
        public GameObject itemShop;
        public ShopUI shopPlantUI;
        public ShopUI shopMonsterUI;
        public List<ItemData> allItems;
        public static ShopManager Instance;
        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }
        public ItemData GetItemById(string id)
        {
            return allItems.Find(item => item.itemId == id);
        }
        public void LoadAllItems()
        {
            allItems = Resources.LoadAll<ItemData>("Items/Seeds").ToList();
        }
        private void Start()
        {
            GSheetManager.Instance.OnDataLoaded += OnDataReady;
            LoadAllItems();
        }

        public void RefreshAllInventoryUI()
        {
            shopPlantUI.RefreshUI();
            shopMonsterUI.RefreshUI();
        }

        void OnDataReady()
        {
            var items = GSheetManager.Instance.itemList;
            foreach (var item in items)
            {
                // Debug.Log($"{item.seedName} - Harga: {item.seedPrice}");
                var prefabItem = Instantiate(itemShop);
                prefabItem.GetComponent<ItemShop>().Setup(item, null);
                prefabItem.transform.SetParent(content, false);
            }
            RefreshAllInventoryUI();
        }
    }
}
