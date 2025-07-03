using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using UnityEngine.EventSystems;
using Unity.VisualScripting;

public enum MonsterCatalogueItemType
{
    Monster,
    Add,
    Locked,
    Unlocked
}

public class MonsterCatalogueItemUI : MonoBehaviour, IPointerClickHandler
{
    [Header("UI Elements")]
    public Image monsterImage;
    public GameObject selectedOverlay;
    public GameObject lockedOverlay;
    public GameObject unlockedOverlay;
    public GameObject addOverlay;

    private MonsterController monsterController;
    private MonsterCatalogueItemType itemType;
    private MonsterCatalogueDetailUI monsterCatalogueDetailUI;
    private MonsterCatalogueListUI parentListUI;
    private bool isSelected;

    public void SetupItem(MonsterController _monsterController, MonsterCatalogueItemType _itemType)
    {
        monsterController = _monsterController;
        itemType = _itemType;

        isSelected = false;
        selectedOverlay.SetActive(false);
        lockedOverlay.SetActive(false);
        unlockedOverlay.SetActive(false);
        addOverlay.SetActive(false);

        SetType(_itemType);
        SetMonsImg();

        if (monsterCatalogueDetailUI == null)
            monsterCatalogueDetailUI = FindAnyObjectByType<MonsterCatalogueDetailUI>();
        
        if (parentListUI == null)
            parentListUI = GetComponentInParent<MonsterCatalogueListUI>();
    }

    private void SetType(MonsterCatalogueItemType type)
    {
        switch (type)
        {
            case MonsterCatalogueItemType.Monster:
                monsterImage.color = Color.white;
                selectedOverlay.SetActive(false);
                lockedOverlay.SetActive(false);
                unlockedOverlay.SetActive(false);
                addOverlay.SetActive(false);
                break;
            case MonsterCatalogueItemType.Add:
                monsterImage.color = Color.clear;
                selectedOverlay.SetActive(false);
                lockedOverlay.SetActive(false);
                unlockedOverlay.SetActive(false);
                addOverlay.SetActive(true);
                break;
            case MonsterCatalogueItemType.Locked:
                monsterImage.color = Color.clear;
                selectedOverlay.SetActive(false);
                lockedOverlay.SetActive(true);
                unlockedOverlay.SetActive(false);
                addOverlay.SetActive(false);
                break;
            case MonsterCatalogueItemType.Unlocked:
                monsterImage.color = Color.clear;
                selectedOverlay.SetActive(false);
                lockedOverlay.SetActive(false);
                unlockedOverlay.SetActive(true);
                addOverlay.SetActive(false);
                break;
        }
    }

    private void SetMonsImg()
    {
        if (itemType == MonsterCatalogueItemType.Monster)
            monsterImage.sprite = monsterController.GetEvolutionIcon(MonsterIconType.Catalogue);
        else
            monsterImage.sprite = null; // Clear image for non-monster items
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Only handle click for Monster type items
        if (itemType == MonsterCatalogueItemType.Monster) HandleMonsterItemClick();
        if (itemType == MonsterCatalogueItemType.Add) HandleAddButtonClick();
    }   
    
    private void HandleMonsterItemClick()
    {
        // Set selected state
        SetSelected(true);
        
        // Notify parent list to handle selection
        if (parentListUI != null)
        {
            parentListUI.SelectItem(this);
        }
        
        // Update details panel
        if (monsterCatalogueDetailUI != null && monsterController != null)
        {
            monsterCatalogueDetailUI.SetDetails(monsterController);
        }
        else
        {
            Debug.LogWarning($"MonsterCatalogueDetailUI : {monsterCatalogueDetailUI != null} & MonsterController : {monsterController != null}");
        }
    }
    
    private void HandleAddButtonClick()
    {
        // Get a reference to the MonsterManager
        var monsterManager = ServiceLocator.Get<MonsterManager>();
        if (monsterManager != null)
        {
            // Increment the max monster slots
            monsterManager.maxMonstersSlots++;
            Debug.Log($"Added monster slot. Total slots now: {monsterManager.maxMonstersSlots}");
            
            // Change this item from "Add" to "Unlocked"
            itemType = MonsterCatalogueItemType.Unlocked;
            SetType(MonsterCatalogueItemType.Unlocked);
            
            // Optional: Save the updated slot count
            // SaveSystem.SaveMonsterSlots(monsterManager.maxMonstersSlots);
            
            // Find the next locked item and convert it to "Add"
            if (parentListUI != null)
            {
                parentListUI.SetNextLockedItemToAdd();
            }
        }
        else
        {
            Debug.LogError("MonsterManager not found. Cannot add monster slot.");
        }
    }
    
    // Public method to set selected state externally
    public void SetSelected(bool selected)
    {
        isSelected = selected;
        selectedOverlay.SetActive(isSelected);
    }

    // Add these methods to your MonsterCatalogueItemUI class
    public MonsterCatalogueItemType GetItemType()
    {
        return itemType;
    }

    public void SetItemType(MonsterCatalogueItemType type)
    {
        itemType = type;
        SetType(type);
    }
}
