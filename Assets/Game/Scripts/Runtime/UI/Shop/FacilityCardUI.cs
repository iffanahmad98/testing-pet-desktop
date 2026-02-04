using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Spine.Unity;
using UnityEngine.Localization.SmartFormat.Core.Parsing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
public class FacilityCardUI : MonoBehaviour, IPointerClickHandler, IPointerExitHandler
{
    [Header("UI References")]
    public GameObject grayscaleObj;
    public TMP_Text nameText;
    public TMP_Text priceText;
    public Button selectButton;
    public Button buyButton;
    public Button useButton;
    public Button cancelButton;
    public Image highlightImage;
    public Image thumbnail;
    public Image cooldownOverlay;
    public TMP_Text cooldownText;

    [Header("Grayscaleable Components")]
    public Material grayscaleMat;
    public Image[] grayscaleImage;
    
    [SerializeField] private SkeletonGraphic _anim;
    [SerializeField] private Animator _animator;

    [Header("Events")]
    public Action<FacilityCardUI> OnSelected;
    public Action<FacilityCardUI> OnUseClicked;
    public Action<FacilityCardUI> OnBuyClicked;
    public Action<FacilityCardUI> OnCancelClicked;

    public FacilityDataSO FacilityData;
    public MonsterDataSO npc;

    private FacilityManager facilityManager;
    private bool isNPC = false;

    private bool _isSelected;
    public bool IsSelected => _isSelected;

    bool isCanBuy;
    private void Start()
    {
        facilityManager = ServiceLocator.Get<FacilityManager>();
    }

    public void SetupNPC(MonsterDataSO data)
    {
        isNPC = true;
        npc = data;

        print("setup NPC");

        if (data.monsterSpine != null)
        {
            print("npc data skeleton not null");
            thumbnail.gameObject.SetActive(false);
            _animator.gameObject.SetActive(false);
            _anim.gameObject.SetActive(true);
            _anim.skeletonDataAsset = data.monsterSpine[0];
            _anim.Initialize(true);
            _anim.timeScale=0f;
            
            // StartCoroutine (nSetActiveAnim ());
        }

        nameText.text = data.monsterName;
        thumbnail.sprite = data.CardIcon[data.isEvolved ? 1 : 0];
        priceText.text = data.monsterPrice.ToString();

        bool isOwned = SaveSystem.IsNPCOwned(data.id);
        bool isActive = SaveSystem.IsNPCActive(data.id);
        bool hasPrerequisite = CheckNPCPrerequisite(data);
        Debug.Log($"Setting up NPC: {data.id}, Owned: {isOwned}, Active: {isActive}, HasPrerequisite: {hasPrerequisite}");

        buyButton.gameObject.SetActive(!isOwned);
        buyButton.interactable = hasPrerequisite; // Disable buy button if prerequisite not met
        useButton.gameObject.SetActive(isOwned && !isActive);   // Show Apply button only if owned but not active
        cancelButton?.gameObject.SetActive(isOwned && isActive); // Show Cancel button only if already active
        cooldownOverlay?.gameObject.SetActive(false);

        SetupButtonListeners(data.id);

    }

    public IEnumerator nSetActiveAnim () { // menghilangkan bug jamur ketarik.
        yield return null;
        if (_anim != null)
        {
            _anim.timeScale=1f;
            AnimUtils.SetIdle(_anim);
        }
    }

    public void SetupFacility(FacilityDataSO data)
    {
        isNPC = false;
        FacilityData = data;

        if (data.animatorController != null)
        {
            thumbnail.gameObject.SetActive(false);
            _anim.gameObject.SetActive(false);
            _animator.gameObject.SetActive(true);

            _animator.runtimeAnimatorController = data.animatorController;
        }

        nameText.text = data.facilityName;
        thumbnail.sprite = data.thumbnail;

        // Set price to "FREE" for toggle facilities, otherwise show price
        priceText.text = data.isFreeToggleFacility ? "FREE" : data.price.ToString();

        UpdateState();
        SetupButtonListeners(data.facilityID);
    }

    private void SetupButtonListeners(string id)
    {
        selectButton.onClick.RemoveAllListeners();
        selectButton.onClick.AddListener(() => OnClickCard());

        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(() => OnBuyClicked?.Invoke(this));

        useButton.onClick.RemoveAllListeners();
        useButton.onClick.AddListener(() => OnUseClicked?.Invoke(this));

        cancelButton?.onClick.RemoveAllListeners();
        cancelButton?.onClick.AddListener(() => OnCancelClicked?.Invoke(this));
    }

    public void UpdateState()
    {
        if (isNPC)
        {
            UpdateStateNPC(npc.id);
            return;
        }

        if (FacilityData == null) return;

        string id = FacilityData.facilityID;

        // Handle free toggle facilities (Pumpkin Facility)
        if (FacilityData.isFreeToggleFacility)
        {
            bool isActive = SaveSystem.IsFacilityOwned(id); // Use ownership flag as active/inactive state

            // Never show buy button for free toggle facilities
            buyButton.gameObject.SetActive(false);

            // Show Apply button when inactive, Cancel button when active
            useButton.gameObject.SetActive(!isActive);
            cancelButton?.gameObject.SetActive(isActive);

            // Always interactable (no cooldown)
            useButton.interactable = true;

            // Hide cooldown overlay
            cooldownOverlay?.gameObject.SetActive(false);
            if (cooldownText != null)
            {
                cooldownText.text = "";
            }

            return;
        }

        // Normal facility logic
        bool isOwned = SaveSystem.IsFacilityOwned(id);
        bool canUse = facilityManager?.CanUseFacility(id) ?? false;
        bool isOnOwnCooldown = facilityManager?.IsOnOwnCooldown(id) ?? false;

        // Show appropriate buttons based on ownership and cooldown state
        useButton.gameObject.SetActive(isOwned && !isOnOwnCooldown);
        buyButton.gameObject.SetActive(!isOwned);
        cancelButton?.gameObject.SetActive(isOwned && isOnOwnCooldown);

        // Set interactable based on whether facility can be used
        useButton.interactable = canUse;

        UpdateCooldownDisplay();
    }

    public void UpdateStateNPC(string npcID)
    {
        bool isOwned = SaveSystem.IsNPCOwned(npcID);
        bool isActive = SaveSystem.IsNPCActive(npcID);
        bool hasPrerequisite = npc != null ? CheckNPCPrerequisite(npc) : true;

        buyButton.gameObject.SetActive(!isOwned);
        buyButton.interactable = hasPrerequisite; // Disable buy button if prerequisite not met
        useButton.gameObject.SetActive(isOwned && !isActive);   // Show Apply button only if owned but not active
        cancelButton?.gameObject.SetActive(isOwned && isActive); // Show Cancel button only if already active
        useButton.interactable = true;
    }


    private void Update()
    {
        // Only update cooldown visuals if it's a facility and owned
        if (!isNPC && FacilityData != null && SaveSystem.IsFacilityOwned(FacilityData.facilityID))
        {
            UpdateCooldownDisplay();
        }
    }

    private void UpdateCooldownDisplay()
    {
        if (facilityManager == null || FacilityData == null) return;

        bool canUse = facilityManager.CanUseFacility(FacilityData.facilityID);
        bool isOnOwnCooldown = facilityManager.IsOnOwnCooldown(FacilityData.facilityID);

        // Only show cooldown overlay and text if THIS facility is on its own cooldown
        cooldownOverlay?.gameObject.SetActive(isOnOwnCooldown);
        useButton.interactable = canUse;

        if (cooldownText != null)
        {
            cooldownText.text = isOnOwnCooldown ? $"{facilityManager.GetCooldownRemaining(FacilityData.facilityID):F1}s" : "";
        }
    }

    public void SetSelected(bool selected)
    {
        _isSelected = selected;
        highlightImage?.gameObject.SetActive(selected);

        if (_anim != null && _anim.skeletonDataAsset != null)
        {
            AnimateNPC();
        }
        else if (_animator.runtimeAnimatorController != null)
        {
            AnimateFacility();
        }
    }
    private void AnimateNPC()
    {
        if (_isSelected)
        {
            int _random = UnityEngine.Random.Range(0, 2);
            string randomAnim = _random == 0 ? "eating" : "jumping";

            AnimUtils.SetAnim(_anim, randomAnim);
            AnimUtils.AddIdle(_anim);
        }
        else
        {
            AnimUtils.SetIdle(_anim);
        }
    }

    private void AnimateFacility()
    {
        if (_isSelected)
        {
            _animator.SetTrigger("selected");
        }
    }

    public void OnClickCard()
    {
        OnSelected?.Invoke(this);
    }

    public void SetCancelActive(bool isActive)
    {
        cancelButton?.gameObject.SetActive(isActive);
    }

    /// <summary>
    /// Check if prerequisite NPC is owned
    /// </summary>
    /// <param name="npcData">NPC data to check</param>
    /// <returns>True if no prerequisite or prerequisite is met, false otherwise</returns>
    private bool CheckNPCPrerequisite(MonsterDataSO npcData)
    {
        // If no prerequisite, return true
        if (string.IsNullOrEmpty(npcData.prerequisiteNPCId))
        {
            return true;
        }

        // Check if player owns the prerequisite NPC
        return SaveSystem.HasNPC(npcData.prerequisiteNPCId);
    }

    public void SetGrayscale(bool grayscale)
    {
        grayscaleObj.SetActive(grayscale);

        if (grayscale)
        {
            foreach(var img in grayscaleImage)
            {
                img.material = grayscaleMat;
            }
            _anim.material = grayscaleMat;
        }
        else
        {
            foreach(var img in grayscaleImage)
            {
                img.material = null;
            }
            _anim.material = null;
        }
    }

    #region Requirement
    public void SetCanBuy (bool value) { // MonsterShopManager.cs
        isCanBuy = value;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (FacilityData != null) {
            RequirementTipManager.Instance.StartClick(FacilityData.requirementTipDataSO.GetInfoData ());
        } else if (npc != null) {
            RequirementTipManager.Instance.StartClick(npc.requirementTipDataSO.GetInfoData ());
        }
        
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        RequirementTipManager.Instance.EndHover();
    }
    #endregion
}
