using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class FacilityCardUI : MonoBehaviour
{
    public TMP_Text nameText;
    public TMP_Text priceText;
    public Button selectButton;
    public Button buyButton;
    public Button useButton;
    public Button cancelButton; // Added cancel button
    public Image highlightImage; // Optional highlight for selection
    public Image thumbnail;
    public Image cooldownOverlay; // Visual overlay for cooldown
    public TMP_Text cooldownText; // Countdown text

    public Action<FacilityCardUI> OnSelected;
    public Action<FacilityCardUI> OnUseClicked;
    public Action<FacilityCardUI> OnBuyClicked;
    public Action<FacilityCardUI> OnCancelClicked; // Added cancel action

    public FacilityDataSO FacilityData { get; private set; }
    public MonsterDataSO npc { get; private set; } // For NPCs, if applicable
    private FacilityManager facilityManager;

    private void Start()
    {
        facilityManager = ServiceLocator.Get<FacilityManager>();
    }

    public void SetupNPC(MonsterDataSO data)
    {
        nameText.text = data.monsterName;
        thumbnail.sprite = data.CardIcon[data.isEvolved ? 1 : 0];
        priceText.text = data.monsterPrice.ToString();

        bool isOwned = SaveSystem.IsFacilityOwned(data.id);

        useButton.gameObject.SetActive(isOwned);
        buyButton.gameObject.SetActive(!isOwned);
        if (cancelButton != null)
        {
            cancelButton.gameObject.SetActive(isOwned);
        }
        selectButton.onClick.RemoveAllListeners();
        selectButton.onClick.AddListener(() => OnClickCard());
        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(() => OnBuyClicked?.Invoke(this));
        useButton.onClick.RemoveAllListeners();
        useButton.onClick.AddListener(() => OnUseClicked?.Invoke(this));
        if (cancelButton != null)
        {
            cancelButton.onClick.RemoveAllListeners();
            cancelButton.onClick.AddListener(() => OnCancelClicked?.Invoke(this));
        }
    }
    public void SetupFacility(FacilityDataSO data)
    {
        FacilityData = data;
        nameText.text = data.facilityName;
        thumbnail.sprite = data.thumbnail;
        priceText.text = data.price.ToString();

        bool isOwned = SaveSystem.IsFacilityOwned(data.facilityID);
        bool canUse = facilityManager?.CanUseFacility(data.facilityID) ?? false;

        useButton.gameObject.SetActive(isOwned);
        buyButton.gameObject.SetActive(!isOwned);

        // Cancel button is visible when owned but not usable (on cooldown)
        if (cancelButton != null)
        {
            cancelButton.gameObject.SetActive(isOwned && !canUse);
        }

        // Update use button interactability based on cooldown
        if (useButton != null)
        {
            useButton.interactable = canUse;
        }

        // Set up button listeners
        selectButton.onClick.RemoveAllListeners();
        selectButton.onClick.AddListener(() => OnClickCard());
        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(() => OnBuyClicked?.Invoke(this));
        useButton.onClick.RemoveAllListeners();
        useButton.onClick.AddListener(() => OnUseClicked?.Invoke(this));

        if (cancelButton != null)
        {
            cancelButton.onClick.RemoveAllListeners();
            cancelButton.onClick.AddListener(() => OnCancelClicked?.Invoke(this));
        }
    }

    public void UpdateStateNPC(string npcID)
{
    bool isOwned = SaveSystem.IsFacilityOwned(npcID);
    bool canUse = facilityManager?.CanUseFacility(npcID) ?? false;

    useButton.gameObject.SetActive(isOwned && canUse);
    buyButton.gameObject.SetActive(!isOwned);
    
    // Cancel button shows when owned but on cooldown
    if (cancelButton != null)
    {
        cancelButton.gameObject.SetActive(isOwned && !canUse);
    }

    if (useButton != null)
    {
        useButton.interactable = canUse;
    }
}

public void UpdateState()
{
    if (FacilityData == null) return;
    
    bool isOwned = SaveSystem.IsFacilityOwned(FacilityData.facilityID);
    bool canUse = facilityManager?.CanUseFacility(FacilityData.facilityID) ?? false;

    useButton.gameObject.SetActive(isOwned && canUse);
    buyButton.gameObject.SetActive(!isOwned);
    
    // Cancel button shows when owned but on cooldown
    if (cancelButton != null)
    {
        cancelButton.gameObject.SetActive(isOwned && !canUse);
    }

    if (useButton != null)
    {
        useButton.interactable = canUse;
    }

    UpdateCooldownDisplay();
}

    private void Update()
    {
        if (FacilityData != null && SaveSystem.IsFacilityOwned(FacilityData.facilityID))
        {
            UpdateCooldownDisplay();
        }
    }

    private void UpdateCooldownDisplay()
    {
        if (facilityManager == null) return;

        bool canUse = facilityManager.CanUseFacility(FacilityData.facilityID);
        
        if (cooldownOverlay != null)
        {
            cooldownOverlay.gameObject.SetActive(!canUse);
        }

        if (useButton != null)
        {
            useButton.interactable = canUse;
        }

        if (!canUse && cooldownText != null)
        {
            float remainingTime = facilityManager.GetCooldownRemaining(FacilityData.facilityID);
            cooldownText.text = $"{remainingTime:F1}s";
        }
        else if (cooldownText != null)
        {
            cooldownText.text = "";
        }
    }

    public void SetSelected(bool selected)
    {
        if (highlightImage != null)
            highlightImage.gameObject.SetActive(selected);
    }

    public void OnClickCard()
    {
        OnSelected?.Invoke(this);
    }

    public void SetCancelActive(bool isActive)
    {
        if (cancelButton != null)
            cancelButton.gameObject.SetActive(isActive);
    }
}
