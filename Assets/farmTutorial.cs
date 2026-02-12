using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum MenuBtn { None,
    Harvest, 
    Watering, 
    Remove, 
    Inventory, 
    Shop, 
    Setting,
    PlantShopButton,
    MonsterShopButton,
    BuySeedButton,
    FertilizerTab,
    SeedTab,
    HarvestTab,
    All }

public class FarmTutorial : MonoBehaviour
{
    [SerializeField] private int tutorialStepIndex = 0;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI bodyText;
    [SerializeField] private Button nextButton;
    [SerializeField] private Image handPointer;

    [SerializeField] private FarmTutorialStepData[] stepData;

    Dictionary<MenuBtn, Button> _btn = new();
    readonly Dictionary<string, Button> _buyButtons = new();
    Button _currentTutorialButton;

    int _seedBuyRequirement = 8;
    int _totalSeedBought = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        var menuBar = GameObject.Find("MenuBar")?.transform;
        _totalSeedBought = 0;
        
        var holder = menuBar.Find("GameObject");
        if (!holder) holder = menuBar; // fallback kalau container "GameObject" tidak ada

        _btn[MenuBtn.None] = null;
        _btn[MenuBtn.Harvest] = holder.Find("Btn_Harvest")?.GetComponent<Button>();
        _btn[MenuBtn.Watering] = holder.Find("Btn_Watering")?.GetComponent<Button>();
        _btn[MenuBtn.Remove] = holder.Find("Btn_Remove")?.GetComponent<Button>();
        _btn[MenuBtn.Inventory] = holder.Find("Btn_inventory")?.GetComponent<Button>();
        _btn[MenuBtn.Shop] = holder.Find("Btn_Shop")?.GetComponent<Button>();
        _btn[MenuBtn.Setting] = holder.Find("Btn_Setting")?.GetComponent<Button>();
        _btn[MenuBtn.PlantShopButton] = FindButtonIncludeInactive("plantShopButton");
        _btn[MenuBtn.MonsterShopButton] = FindButtonIncludeInactive("monsterShopButton");
        _btn[MenuBtn.FertilizerTab] = FindButtonIncludeInactive("ButtonFertilizer");
        _btn[MenuBtn.SeedTab] = FindButtonIncludeInactive("ButtonSeed");
        _btn[MenuBtn.HarvestTab] = FindButtonIncludeInactive("ButtonHarvest");

        LockAll();
        ExecuteTutorialAtStep();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void ExecuteTutorialAtStep(int step = 0)
    {
        // step is an index number
        if (tutorialStepIndex >= stepData.Length)
        {
            Debug.LogError("Step is bigger than the stepData array");
            return;
        }

        FarmTutorialStepData currentStep = stepData[step];
        currentStep.DeletePreviousStep(handPointer);
        _totalSeedBought = 0;

        PrepareButtonsFor(currentStep);

        currentStep.WriteInstruction(titleText, bodyText);

        handPointer.gameObject.SetActive(currentStep.showHandPointer);
        if (currentStep.showHandPointer)
        {
            handPointer.transform.localPosition = currentStep.handPosition;
            var startingX = handPointer.transform.position.x;

            handPointer.transform.DOKill();
            handPointer.transform.DOMoveX(startingX + 20, 0.25f)
                .SetEase(Ease.OutQuad).SetLoops(-1, LoopType.Yoyo);
        }
    }

    private void PrepareButtonsFor(FarmTutorialStepData currentStep)
    {
        if (nextButton != null)
        {
            // tampilkan tombol Next cuma kalau perlu
            nextButton.gameObject.SetActive(currentStep.isNextButtonActive);
            nextButton.onClick.RemoveAllListeners();

            if (currentStep.isNextButtonActive)
                nextButton.onClick.AddListener(OnNextButtonClicked);
        }

        // atur tombol yg boleh diklik
        EnableOnly(currentStep.enabledButton);
        HookTutorialAdvance(_btn[currentStep.enabledButton]);
        if (currentStep.enabledButton == MenuBtn.All) UnlockAll();

        CheckBuySeed(currentStep);

        if (currentStep.isSelectSeed)
        {
            UnhookTutorialAdvance();
        }
    }

    private void CheckBuySeed(FarmTutorialStepData currentStep)
    {
        if (currentStep.isBuySeeds)
        {
            UnhookTutorialAdvance();
            _btn[MenuBtn.BuySeedButton] = GetBuyButton(currentStep.seedName);
            EnableOnly(MenuBtn.BuySeedButton);

            // pastikan punya cukup coin
            if (CoinManager.CheckCoins(currentStep.minimumCost) == false)
            {
                CoinManager.AddCoins(currentStep.minimumCost - CoinManager.Coins);
            }
        }
    }

    private void OnNextButtonClicked()
    {
        Debug.Log("Next step");
        tutorialStepIndex++;
        ExecuteTutorialAtStep(tutorialStepIndex);
    }

    void HookTutorialAdvance(Button btn)
    {
        if (!btn) return;
        Debug.Log("Hooked advance tutorial button");

        // lepas dari tombol sebelumnya
        if (_currentTutorialButton)
            _currentTutorialButton.onClick.RemoveListener(OnNextButtonClicked);

        _currentTutorialButton = btn;

        // cegah dobel
        btn.onClick.RemoveListener(OnNextButtonClicked);
        btn.onClick.AddListener(OnNextButtonClicked);
    }

    void UnhookTutorialAdvance()
    {
        if (_currentTutorialButton)
            _currentTutorialButton.onClick.RemoveListener(OnNextButtonClicked);

        _currentTutorialButton = null;
    }

    void OnDestroy()
    {
        UnhookTutorialAdvance(); // bersih-bersih
    }

    public void LockAll()
    {
        foreach (var b in _btn.Values)
            if (b) b.interactable = false;
    }

    public void EnableOnly(MenuBtn which)
    {
        Debug.Log($"Enable only {which}");
        foreach (var kv in _btn)
            if (kv.Value) kv.Value.interactable = (kv.Key == which);
    }

    public void UnlockAll()
    {
        foreach (var b in _btn.Values)
            if (b) b.interactable = true;
    }

    Button FindButtonIncludeInactive(string name)
    {
        return Object.FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None)
            .FirstOrDefault(b => b.name == name && b.gameObject.scene.IsValid());
    }

    public void CountSeedBought()
    {
        // if (!stepData[tutorialStepIndex].isBuySeeds) return;

        _totalSeedBought += 1;
        if (_totalSeedBought == stepData[tutorialStepIndex].seedBuyRequirement - 1)
        {
            // buat tombol ini jadi HookTutorialAdvance supaya bisa lanjut tutorial kalau klik sekali lagi
            HookTutorialAdvance(_btn[MenuBtn.BuySeedButton]);
        }

        if (stepData[tutorialStepIndex].seedBuyRequirement == 1)
        {
            // pengecekan khusus kalau requirement cuma 1
            // langsung aja next
            OnNextButtonClicked();

            Debug.Log($"{tutorialStepIndex} | Panel name to close: {stepData[tutorialStepIndex].panelNameToClose}");
            if (!string.IsNullOrWhiteSpace(stepData[tutorialStepIndex].panelNameToClose))
            {
                var shopPanel = Object.FindObjectsByType<Image>(FindObjectsInactive.Include, FindObjectsSortMode.None)
                .FirstOrDefault(b => b.name == stepData[tutorialStepIndex].panelNameToClose && b.gameObject.scene.IsValid());
                if (shopPanel != null)
                {
                    Debug.Log($"Find image by name {stepData[tutorialStepIndex].panelNameToClose}");
                    shopPanel.gameObject.SetActive(false);
                }
                else
                {
                    Debug.Log($"Cannot find image by name {stepData[tutorialStepIndex].panelNameToClose}");
                }
            }
        }
    }

    public void SelectSeedToSow()
    {
        // Pemain klik tombol seed, lanjut ke langkah berikutnya
        OnNextButtonClicked();
    }

    public void RegisterBuyButton(string itemId, Button btn)
    {
        Debug.Log($"Register a buy button with ID: {itemId}");
        _buyButtons[itemId] = btn; // overwrite kalau respawn
    }

    public Button GetBuyButton(string itemId)
        => _buyButtons.TryGetValue(itemId, out var b) ? b : null;
}
