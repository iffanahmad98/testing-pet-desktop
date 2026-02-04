using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public partial class TutorialManager
{
    private void CacheUIButtonsFromUIManager()
    {
        var ui = ServiceLocator.Get<UIManager>();
        if (ui == null)
        {
            Debug.LogWarning("TutorialManager: UIManager tidak ditemukan saat cache buttons.");
            return;
        }

        var list = new List<Button>();

        void Add(Button b)
        {
            if (b != null && !list.Contains(b))
                list.Add(b);
        }

        var main = ui.buttons;
        if (main != null)
        {
            Add(main.UIMenuButton);
            Add(main.miniInventoryButton);
            Add(main.groundButton);
            Add(main.doorButton);
            Add(main.windowButton);
            Add(main.miniWindowButton);
            Add(main.shopButton);
            Add(main.closeShopButton);
            Add(main.settingsButton);
            Add(main.closeSettingsButton);
            Add(main.catalogueButton);
            Add(main.closeCatalogueButton);
            Add(main.mainInventoryButton);
            Add(main.tutorialnext);
        }

        var shop = ui.shopButtons;
        if (shop != null)
        {
            Add(shop.catalogueShopButton);
            Add(shop.settingShopButton);
            Add(shop.helpShopButton);
        }

        _uiButtonsCache = list.ToArray();
        _uiButtonsInteractableCache = new bool[_uiButtonsCache.Length];
        for (int i = 0; i < _uiButtonsCache.Length; i++)
        {
            var btn = _uiButtonsCache[i];
            _uiButtonsInteractableCache[i] = btn != null && btn.interactable;
        }
    }

    private void DisableUIManagerButtonsForTutorial()
    {
        if (_uiButtonsCache == null || _uiButtonsCache.Length == 0)
            return;

        for (int i = 0; i < _uiButtonsCache.Length; i++)
        {
            var btn = _uiButtonsCache[i];
            if (btn == null)
                continue;

            if (IsTutorialControlButton(btn))
                continue;

            btn.interactable = false;
        }
    }

    private void RestoreUIManagerButtonsInteractable()
    {
        if (_uiButtonsCache == null || _uiButtonsInteractableCache == null)
            return;

        int len = Mathf.Min(_uiButtonsCache.Length, _uiButtonsInteractableCache.Length);
        for (int i = 0; i < len; i++)
        {
            var btn = _uiButtonsCache[i];
            if (btn != null)
            {
                btn.interactable = _uiButtonsInteractableCache[i];
            }
        }

        _uiButtonsCache = null;
        _uiButtonsInteractableCache = null;
    }

    private bool IsTutorialControlButton(Button btn)
    {
        if (btn == null)
            return false;

        if (btn == skipTutorialButton)
            return true;

        if (tutorialSteps != null)
        {
            for (int i = 0; i < tutorialSteps.Count; i++)
            {
                var step = tutorialSteps[i];
                if (step != null && step.nextButton == btn)
                    return true;
            }
        }

        if (simpleTutorialPanels != null)
        {
            for (int i = 0; i < simpleTutorialPanels.Count; i++)
            {
                var simpleStep = simpleTutorialPanels[i];
                if (simpleStep != null && simpleStep.nextButtonIndex >= 0 &&
                    _uiButtonsCache != null && simpleStep.nextButtonIndex < _uiButtonsCache.Length &&
                    _uiButtonsCache[simpleStep.nextButtonIndex] == btn)
                    return true;
            }
        }

        return false;
    }

    private Button GetSimpleStepNextButton(SimpleTutorialPanelStep step)
    {
        if (step == null)
            return null;

        if (_uiButtonsCache == null || _uiButtonsCache.Length == 0)
            return null;

        if (step.nextButtonIndex < 0 || step.nextButtonIndex >= _uiButtonsCache.Length)
            return null;

        return _uiButtonsCache[step.nextButtonIndex];
    }
}
