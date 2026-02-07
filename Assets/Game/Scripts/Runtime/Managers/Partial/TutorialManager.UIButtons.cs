using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public partial class TutorialManager
{
    private Button _tutorialNextButton;
    private bool _hasLoggedUIButtonCache;

    [Header("Tutorial UI Button Cache (Manual Extras)")]
    [SerializeField] private Button[] manualUIButtonExtras;

    public void RebuildUIButtonCache()
    {
        _uiButtonsCache = null;
        _uiButtonsInteractableCache = null;
        _hasLoggedUIButtonCache = false;
        CacheUIButtonsFromUIManager();
        Debug.Log("[TutorialManager] RebuildUIButtonCache dipanggil.");
    }

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
            _tutorialNextButton = main.tutorialnext;
            Add(main.tutorialnext);
        }

        var shop = ui.shopButtons;
        if (shop != null)
        {
            Add(shop.catalogueShopButton);
            Add(shop.settingShopButton);
            Add(shop.helpShopButton);
        }

        // Also collect buttons from any other UI sources (e.g. BoardSign)
        CacheButtonsFromSceneSources(list);

        // Terakhir, tambahkan button manual dari Inspector (jika ada).
        if (manualUIButtonExtras != null)
        {
            foreach (var extra in manualUIButtonExtras)
            {
                Add(extra);
            }
        }

        _uiButtonsCache = list.ToArray();
        _uiButtonsInteractableCache = new bool[_uiButtonsCache.Length];
        for (int i = 0; i < _uiButtonsCache.Length; i++)
        {
            var btn = _uiButtonsCache[i];
            _uiButtonsInteractableCache[i] = btn != null && btn.interactable;
        }

        LogUIButtonCacheOnce();
    }

    private void CacheButtonsFromSceneSources(List<Button> list)
    {
        if (list == null)
            return;

        // Find all MonoBehaviours so we can check which ones implement IUIButtonSource.
        var behaviours = Object.FindObjectsOfType<MonoBehaviour>(true);
        if (behaviours == null || behaviours.Length == 0)
            return;

        foreach (var behaviour in behaviours)
        {
            if (behaviour is IUIButtonSource source)
            {
                source.CollectButtons(list);
            }
        }
    }

    private void LogUIButtonCacheOnce()
    {
        if (_hasLoggedUIButtonCache)
            return;

        if (_uiButtonsCache == null || _uiButtonsCache.Length == 0)
            return;

        _hasLoggedUIButtonCache = true;

        for (int i = 0; i < _uiButtonsCache.Length; i++)
        {
            var btn = _uiButtonsCache[i];
            if (btn == null)
                continue;

            var go = btn.gameObject;
        }
    }

    private static string GetTransformPath(Transform t)
    {
        if (t == null)
            return "<null>";

        var stack = new System.Collections.Generic.List<string>();
        while (t != null)
        {
            stack.Add(t.name);
            t = t.parent;
        }
        stack.Reverse();
        return string.Join("/", stack);
    }

    private void DisableUIManagerButtonsForTutorial()
    {
        if (_uiButtonsCache == null || _uiButtonsCache.Length == 0)
        {
            CacheUIButtonsFromUIManager();
        }

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

        if (_tutorialNextButton != null)
        {
            _tutorialNextButton.interactable = false;
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

        if (simpleTutorialPanels != null)
        {
            for (int i = 0; i < simpleTutorialPanels.Count; i++)
            {
                var simpleStep = simpleTutorialPanels[i];
                var config = simpleStep != null ? simpleStep.config : null;
                if (config != null && config.nextButtonIndex >= 0 &&
                    _uiButtonsCache != null && config.nextButtonIndex < _uiButtonsCache.Length &&
                    _uiButtonsCache[config.nextButtonIndex] == btn)
                    return true;
            }
        }

        return false;
    }

    private Button GetSimpleStepNextButton(SimpleTutorialPanelStep step)
    {
        if (step == null)
            return null;

        var config = step.config;
        if (config == null)
            return null;

        if (_uiButtonsCache == null || _uiButtonsCache.Length == 0)
        {
            CacheUIButtonsFromUIManager();
        }

        if (_uiButtonsCache == null || _uiButtonsCache.Length == 0)
            return null;

        if (config.nextButtonIndex < 0 || config.nextButtonIndex >= _uiButtonsCache.Length)
            return null;

        return _uiButtonsCache[config.nextButtonIndex];
    }
}
