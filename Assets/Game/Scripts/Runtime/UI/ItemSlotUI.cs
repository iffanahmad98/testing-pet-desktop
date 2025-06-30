using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class ItemSlotUI : MonoBehaviour, IPointerClickHandler
{
    [Header("UI References")]
    public Image iconImage;
    public TextMeshProUGUI amountText;

    [Header("Prefab Mapping")]
    [SerializeField] private GameObject foodPrefab;
    [SerializeField] private GameObject medicinePrefab;
    [SerializeField] private RectTransform canvasParent;

    private ItemDataSO itemData;
    private int itemAmount;

    public void Initialize(ItemDataSO data, int amount, GameObject food, GameObject medicine, RectTransform canvas)
    {
        itemData = data;
        itemAmount = amount;
        foodPrefab = food;
        medicinePrefab = medicine;
        canvasParent = canvas;

        iconImage.sprite = itemData.itemImgs[0];
        amountText.text = $"{amount} pcs";
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            // Cancel placement if any
            ServiceLocator.Get<PlacementManager>().CancelPlacement();
            return;
        }

        if (itemAmount <= 0) return;

        // Determine which prefab to use
        GameObject prefabToPlace = itemData.category switch
        {
            ItemType.Food => foodPrefab,
            ItemType.Medicine => medicinePrefab,
            _ => null
        };


        if (prefabToPlace == null)
        {
            Debug.LogWarning($"No prefab assigned for category {itemData.category}");
            return;
        }

        // Enter placement mode using PlacementManager

        float healingValue = itemData.nutritionValue;

        ServiceLocator.Get<PlacementManager>().StartPlacement(
            prefabToPlace,
            canvasParent,
            itemData.category == ItemType.Medicine
                ? (Vector2 _) => HandleMedicineDirectApply(healingValue)
                : OnConfirmPlacement,
            OnCancelPlacement,
            allowMultiple: itemData.category == ItemType.Food,
            isMedicine: itemData.category == ItemType.Medicine,
            previewSprite: itemData.itemImgs[0]
        );



    }

    private void OnConfirmPlacement(Vector2 position)
    {
        if (itemAmount <= 0)
        {
            Debug.LogWarning("Tried to place item with 0 amount");
            return;
        }

        ServiceLocator.Get<MonsterManager>().SpawnItem(itemData, position);

        itemAmount--;
        amountText.text = $"{itemAmount} pcs";
        SaveSystem.UpdateItemData(itemData.itemID, itemAmount);

        if (itemAmount <= 0)
        {
            ServiceLocator.Get<PlacementManager>().CancelPlacement();
            Destroy(gameObject); // Remove slot if no items left
        }
    }
    private void HandleMedicineDirectApply(float healingValue)
    {
        MonsterController monster = ServiceLocator.Get<PlacementManager>().TryGetMonsterUnderCursor();

        if (monster != null)
        {
            monster.GiveMedicine(healingValue);
            itemAmount--;
            amountText.text = $"{itemAmount} pcs";
            SaveSystem.UpdateItemData(itemData.itemID, itemAmount);

            if (itemAmount <= 0)
            {
                ServiceLocator.Get<PlacementManager>().CancelPlacement();
                Destroy(gameObject);
            }
        }
        else
        {
            ServiceLocator.Get<UIManager>()?.ShowMessage("Place medicine on a sick monster!", 1f);
        }
    }



    private void OnCancelPlacement()
    {
        Debug.Log("Placement cancelled.");
    }
}
