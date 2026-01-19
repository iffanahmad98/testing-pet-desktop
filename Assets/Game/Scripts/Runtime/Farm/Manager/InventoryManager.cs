using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using MagicalGarden.Farm;
using MagicalGarden.Manager;
using TMPro;

namespace MagicalGarden.Inventory
{

    public class InventoryManager : MonoBehaviour
    {
        public static InventoryManager Instance;
        public GameObject dropFlyIcon;
        public List<InventoryItem> items = new List<InventoryItem>();
        public GameObject uiInventory;
        public InventoryUI inventoryFertilizerUI;
        public InventoryUI inventorySeedUI;
        public InventoryUI inventoryHarvestUI;

        [Header("Information Panel Item")]
        public TextMeshProUGUI descFull;
        public Image imageItem;
        public GameObject descAddFertilizer;
        public GameObject descAddCrop;
        public GameObject descAddSeed;

        [Header("Information Panel Item Fertilizer")]
        public TextMeshProUGUI descTime;
        public TextMeshProUGUI descPoopNormal;
        public TextMeshProUGUI descPoopMytic;
        [Header("Information Panel Item Seed")]
        public TextMeshProUGUI descCount;
        public TextMeshProUGUI descWateringHour;
        public TextMeshProUGUI descLiveHour;

        [Header ("Data")]
        public FarmItemDatabaseSO allFarmItemDatabase;
        PlayerConfig playerConfig;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        void Start () 
        {
           // LoadAllItemFarmDatas ();
        }

        public void AddItem(ItemData itemData, int amount)
        {
            Debug.Log ("Add Item");
            if (itemData.itemType == ItemType.Crop)
            {
                PlantManager.Instance.AddAmountHarvest();
            }
            if (itemData.isStackable)
            {
                var existingItem = items.FirstOrDefault(i => i.itemData.itemId == itemData.itemId);
                if (existingItem != null)
                {
                    existingItem.quantity += amount;
                    return;
                }
            }


            items.Add(new InventoryItem(itemData, amount));
            RefreshAllInventoryUI();
            FertilizerManager.Instance.RefreshAllFertilizerUI();
        }

        public bool RemoveItem(ItemData itemData, int amount)
        {
            Debug.Log ("RemoveItem");
            var item = items.FirstOrDefault(i => i.itemData.itemId == itemData.itemId);
            if (item == null || item.quantity < amount) return false;

            item.quantity -= amount;
            if (item.quantity <= 0)
            {
                items.Remove(item);
            }
            RefreshAllInventoryUI();
            return true;
        }

        public void RefreshAllInventoryUI()
        {
            inventoryFertilizerUI.RefreshUI();
            inventoryHarvestUI.RefreshUI();
            inventorySeedUI.RefreshUI();
        }

        public void SetInformationItem(string desc, Sprite image, ItemType _type)
        {
            descFull.text = desc;
            imageItem.sprite = image;
            if (_type == ItemType.Fertilizer)
            {

            }
            else if (_type == ItemType.Crop)
            {

            }
            else if (_type == ItemType.Seed || _type == ItemType.MonsterSeed)
            {

            }
        }

        public bool HasItem(ItemData itemData, int amount = 1)
        {
            var item = items.FirstOrDefault(i => i.itemData.itemId == itemData.itemId);
            return item != null && item.quantity >= amount;
        }

        public bool HasItems(List<ItemStack> requiredItems)
        {
            foreach (var stack in requiredItems)
            {
                if (!HasItem(stack.item, stack.quantity))
                    return false;
            }
            return true;
        }

        public bool RemoveItems(List<ItemStack> requiredItems)
        {
            if (!HasItems(requiredItems))
                return false;

            foreach (var stack in requiredItems)
            {
                RemoveItem(stack.item, stack.quantity);
            }
            return true;
        }

        public InventoryItem GetItem(string itemId)
        {
            return items.FirstOrDefault(i => i.itemData.itemId == itemId);
        }

        public List<InventoryItem> GetItemsByType(ItemType type)
        {
            return items.Where(i => i.itemData.itemType == type).ToList();
        }
        public void SetDescAdditionalFertilizer(string time, string countPoopNormal, string countPoopRare)
        {
            descTime.text = time + " Hours";
            descPoopNormal.text = countPoopNormal + " pcs";
            descPoopMytic.text = countPoopRare + " pcs";
        }
        public void SetDescAdditionalSeed(string hourWatering, string hoursGrow)
        {
            descWateringHour.text = hourWatering + " hour";
            descLiveHour.text = hoursGrow + " hours";
        }
        public void InventoryToogle()
        {
            uiInventory.SetActive(!uiInventory.activeSelf);
        }
        
        public void DisableAllDescriptions()
        {
            descAddFertilizer.SetActive(false);
            descAddCrop.SetActive(false);
            descAddSeed.SetActive(false);
        }

        public void ShowOnlyFertilizer()
        {
            DisableAllDescriptions();
            descAddFertilizer.SetActive(true);
        }

        public void ShowOnlyCrop()
        {
            DisableAllDescriptions();
            descAddCrop.SetActive(true);
        }

        public void ShowOnlySeed()
        {
            DisableAllDescriptions();
            descAddSeed.SetActive(true);
        }

        #region Data
        void LoadAllItemFarmDatas () {
            if (playerConfig == null) {
                playerConfig = SaveSystem.PlayerConfig;
            }

            List <OwnedItemFarmData> listOwnedItemFarmData = playerConfig.GetOwnedItemFarmDatas ();
            foreach (OwnedItemFarmData owned in listOwnedItemFarmData) {
                AddItem (allFarmItemDatabase.GetItemData (owned.itemID), owned.amount);
            }
            
        }
        #endregion
    }
    public enum ItemType
    {
        Seed,
        Crop,
        Tool,
        Fertilizer,
        MonsterSeed
    }
    public enum ItemRarity
    {
        Normal,
        Rare,
        Epic,
        Legendary
    }
}