using MagicalGarden.Shop;
using UnityEngine;
using UnityEngine.UI;
using MagicalGarden.Inventory;
using System.Linq;
using System.Collections.Generic;
using TMPro;

namespace MagicalGarden.Manager
{
    public class ShopManager : MonoBehaviour
    {
        public Transform contentSeed;
        public Transform contentMonster;
        public GameObject itemShop;
        public ShopUI shopPlantUI;
        public ShopUI shopMonsterUI;
        public List<ItemData> allItems;
        public static ShopManager Instance;
        [Header("Information Shop")]
        public Image iconSeed;
        public GameObject descSeed;
        public TextMeshProUGUI priceText;
        public TextMeshProUGUI timeWateringText;
        public TextMeshProUGUI timeGrowText;
        public TextMeshProUGUI longDescText;
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
                if (item.farmingType.ToLower() == "food")
                {
                    // Debug.Log($"{item.seedName} - Harga: {item.seedPrice}");
                    var prefabItem = Instantiate(itemShop);
                    prefabItem.GetComponent<ShopItemCell>().Setup(item, null);
                    prefabItem.transform.SetParent(contentSeed, false);
                }
                if (item.farmingType.ToLower() == "monster")
                {
                    var prefabItem = Instantiate(itemShop);
                    prefabItem.GetComponent<ShopItemCell>().Setup(item, null);
                    prefabItem.transform.SetParent(contentMonster, false);
                }
            }
            RefreshAllInventoryUI();
        }

        public void SetInformation(SheetData currentItemData, Sprite iconData)
        {
            descSeed.SetActive(true);
            iconSeed.sprite = iconData;
            timeGrowText.text = currentItemData.totalGrowTime;
            priceText.text = currentItemData.seedPrice;
            timeWateringText.text = currentItemData.wateringInterval;
            longDescText.text = currentItemData.description;
        }
    }
}
