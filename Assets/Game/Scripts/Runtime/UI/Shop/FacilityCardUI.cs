using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Spine.Unity;

public class FacilityCardUI : MonoBehaviour
{
    [Header("UI References")]
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
    public SkeletonGraphic _anim;

    [Header("Events")]
    public Action<FacilityCardUI> OnSelected;
    public Action<FacilityCardUI> OnUseClicked;
    public Action<FacilityCardUI> OnBuyClicked;
    public Action<FacilityCardUI> OnCancelClicked;

    public FacilityDataSO FacilityData { get; private set; }
    public MonsterDataSO npc { get; private set; }

    private FacilityManager facilityManager;
    private bool isNPC = false;

    private bool _isSelected;
    public bool IsSelected => _isSelected;

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
            _anim.gameObject.SetActive(true);
            _anim.skeletonDataAsset = data.monsterSpine[0];
            _anim.Initialize(true);
            AnimUtils.SetIdle(_anim);            
        }        

            nameText.text = data.monsterName;
        thumbnail.sprite = data.CardIcon[data.isEvolved ? 1 : 0];
        priceText.text = data.monsterPrice.ToString();

        bool isOwned = SaveSystem.IsNPCOwned(data.id);
        bool isActive = SaveSystem.IsNPCActive(data.id);
        Debug.Log($"Setting up NPC: {data.id}, Owned: {isOwned}, Active: {isActive}");

        buyButton.gameObject.SetActive(!isOwned);
        useButton.gameObject.SetActive(isOwned && !isActive);   // Show Apply button only if owned but not active
        cancelButton?.gameObject.SetActive(isOwned && isActive); // Show Cancel button only if already active
        cooldownOverlay?.gameObject.SetActive(false);

        SetupButtonListeners(data.id);
    }

    public void SetupFacility(FacilityDataSO data)
    {
        isNPC = false;
        FacilityData = data;

        nameText.text = data.facilityName;
        thumbnail.sprite = data.thumbnail;
        priceText.text = data.price.ToString();

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
        bool isOwned = SaveSystem.IsFacilityOwned(id);
        bool canUse = facilityManager?.CanUseFacility(id) ?? false;

        useButton.gameObject.SetActive(isOwned);
        buyButton.gameObject.SetActive(!isOwned);
        cancelButton?.gameObject.SetActive(isOwned && !canUse);
        useButton.interactable = canUse;

        UpdateCooldownDisplay();
    }

    public void UpdateStateNPC(string npcID)
    {
        bool isOwned = SaveSystem.IsNPCOwned(npcID);
        useButton.gameObject.SetActive(isOwned);
        buyButton.gameObject.SetActive(!isOwned);
        cancelButton?.gameObject.SetActive(false);
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

        cooldownOverlay?.gameObject.SetActive(!canUse);
        useButton.interactable = canUse;

        if (cooldownText != null)
        {
            cooldownText.text = canUse ? "" : $"{facilityManager.GetCooldownRemaining(FacilityData.facilityID):F1}s";
        }
    }

    public void SetSelected(bool selected)
    {
        _isSelected = selected;
        highlightImage?.gameObject.SetActive(selected);

        if(_anim!=null && _anim.skeletonDataAsset != null)
        {
            if (selected)
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
    }

    public void OnClickCard()
    {
        OnSelected?.Invoke(this);
    }

    public void SetCancelActive(bool isActive)
    {
        cancelButton?.gameObject.SetActive(isActive);
    }
}
