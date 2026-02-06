using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public partial class UIManager
{
    #region Event Subscription

    private void SubscribeEvents()
    {
        var monster = ServiceLocator.Get<MonsterManager>();
        if (monster != null)
        {
            monster.OnPoopChanged += UpdatePoopCounterValue;
            monster.OnPoopChanged?.Invoke(monster.poopCollected);
        }
    }

    private void UnsubscribeEvents()
    {
        var monster = ServiceLocator.Get<MonsterManager>();
        if (monster != null)
        {
            monster.OnPoopChanged -= UpdatePoopCounterValue;
        }
    }

    #endregion

    #region Button Listeners

    private void RegisterButtonListeners()
    {
        buttons.UIMenuButton?.onClick.AddListener(FloatMenu);
        buttons.groundButton?.onClick.AddListener(GroundMenu);
        buttons.doorButton?.onClick.AddListener(MinimizeApplication);
        buttons.windowButton?.onClick.AddListener(ToggleMiniWindowMode);
        buttons.miniWindowButton?.onClick.AddListener(ToggleMiniWindowMode);

        buttons.creditsButton.onClick.AddListener(() => FadePanel(panels.CreditsPanel, panels.CreditsCanvasGroup, true));
        buttons.settingsButton?.onClick.AddListener(() => FadePanel(panels.SettingPanel, panels.SettingCanvasGroup, true));
        shopButtons.settingShopButton?.onClick.AddListener(() => FadePanel(panels.SettingPanel, panels.SettingCanvasGroup, true));
        buttons.shopButton?.onClick.AddListener(() => FadePanel(panels.ShopPanel, panels.ShopCanvasGroup, true));
        buttons.miniInventoryButton?.onClick.AddListener(() =>
        {
            bool isActive = panels.InventoryCanvasGroup.interactable;

            if (isActive)
            {
                // Fade out and disable
                FadePanel(panels.InventoryPanel, panels.InventoryCanvasGroup, false, 0.3f, 1.08f, 0.15f, true);
            }
            else
            {
                // Fade in and enable
                FadePanel(panels.InventoryPanel, panels.InventoryCanvasGroup, true, 0.3f, 1.08f, 0.15f, true);
            }
        });
        buttons.mainInventoryButton?.onClick.AddListener(() =>
        {
            bool isActive = panels.InventoryCanvasGroup.interactable;

            if (isActive)
            {
                // Fade out and disable
                FadePanel(panels.InventoryPanel, panels.InventoryCanvasGroup, false, 0.3f, 1.08f, 0.15f, true);
            }
            else
            {
                // Fade in and enable
                FadePanel(panels.InventoryPanel, panels.InventoryCanvasGroup, true, 0.3f, 1.08f, 0.15f, true);
            }
        });
        buttons.catalogueButton?.onClick.AddListener(() => FadePanel(panels.CataloguePanel, panels.CatalogueCanvasGroup, true));
        shopButtons.catalogueShopButton?.onClick.AddListener(() => FadePanel(panels.CataloguePanel, panels.CatalogueCanvasGroup, true));

        buttons.closeSettingsButton?.onClick.AddListener(() => FadePanel(panels.SettingPanel, panels.SettingCanvasGroup, false));
        buttons.closeShopButton?.onClick.AddListener(() =>
        {
            FadePanel(panels.ShopPanel, panels.ShopCanvasGroup, false);
            GroundMenu();
        });
        buttons.closeCatalogueButton?.onClick.AddListener(() => FadePanel(panels.CataloguePanel, panels.CatalogueCanvasGroup, false));
    }

    private void UnregisterButtonListeners()
    {
        buttons.UIMenuButton?.onClick.RemoveAllListeners();
        buttons.groundButton?.onClick.RemoveAllListeners();
        buttons.doorButton?.onClick.RemoveAllListeners();
        buttons.windowButton?.onClick.RemoveAllListeners();
        buttons.miniWindowButton?.onClick.RemoveAllListeners();
        buttons.settingsButton?.onClick.RemoveAllListeners();
        shopButtons.settingShopButton?.onClick.RemoveAllListeners();
        buttons.shopButton?.onClick.RemoveAllListeners();
        buttons.closeSettingsButton?.onClick.RemoveAllListeners();
        buttons.closeShopButton?.onClick.RemoveAllListeners();
        buttons.catalogueButton?.onClick.RemoveAllListeners();
        shopButtons.catalogueShopButton?.onClick.RemoveAllListeners();
        buttons.closeCatalogueButton?.onClick.RemoveAllListeners();
        buttons.miniInventoryButton?.onClick.RemoveAllListeners();
        buttons.mainInventoryButton?.onClick.RemoveAllListeners();
        buttons.creditsButton.onClick.RemoveAllListeners();
    }

    #endregion
}
