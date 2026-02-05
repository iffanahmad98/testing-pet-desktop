using UnityEngine;
using UnityEngine.UI;
namespace MagicalGarden.Inventory
{
    public class InventoryUISendToPlains : MonoBehaviour
    {
        [SerializeField] InventoryUI inventoryUI;
        [SerializeField] Button sendPlainsButton;
        [SerializeField] Image sendPlainsImage;
        [Header("Data")]
        PlayerConfig playerConfig;
        void Start()
        {
            playerConfig = SaveSystem.PlayerConfig;
            sendPlainsButton.onClick.AddListener(SendPlaint);
        }

        #region EventListener
        public void StartPlains(UITabManager uiTabManager)
        { // UITabManager.cs
            uiTabManager.AddEventClick(OnSendPlainsImage);
        }
        #endregion

        void SendPlaint()
        {
            foreach (InventoryItemCell cell in inventoryUI.GetSlotList())
            {
                ItemDataSO itemDataSO = cell.GetItemDataSO();
                int amount = playerConfig.GetItemFarmHarvestAmount(itemDataSO.itemID);
                playerConfig.AddItem(itemDataSO.itemID, itemDataSO.category, amount);
                playerConfig.ClearItemFarmHarvest(itemDataSO.itemID);
            }
            SaveSystem.SaveAll();
            inventoryUI.RefreshUI();
            ServiceLocator.Get<ItemInventoryUI>().StartPopulateAllInventoriesWhenOpen();
        }

        public void OnSendPlainsImage(int value)
        {
            if (value == 1)
            {
                sendPlainsImage.gameObject.SetActive(true);
            }
            else
            {
                sendPlainsImage.gameObject.SetActive(false);
            }
        }

    }
}
