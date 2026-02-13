using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public partial class TutorialManager
{
    private Button _tutorialNextButton;
    private bool _hasLoggedUIButtonCache;

    [Header("Tutorial UI Button Cache (Manual Extras)")]
    [SerializeField] private Button[] manualUIButtonExtras;

    public bool UIButtonsCacheEmpty =>
    _uiButtonsCache == null || _uiButtonsCache.Length == 0;

    public void RebuildUIButtonCache()
    {

        if (_currentMode != TutorialMode.Plain)
            return;

        var previousInteractables = _uiButtonsInteractableCache;
        var previousActives = _uiButtonsActiveCache;

        _uiButtonsCache = null;
        _uiButtonsInteractableCache = null;
        _uiButtonsActiveCache = null;
        _hasLoggedUIButtonCache = false;

        CacheUIButtonsFromUIManager();
        if (previousInteractables != null && _uiButtonsCache != null)
        {
            var merged = new bool[_uiButtonsCache.Length];
            for (int i = 0; i < _uiButtonsCache.Length; i++)
            {
                if (i < previousInteractables.Length)
                {
                    merged[i] = previousInteractables[i];
                }
                else
                {
                    var btn = _uiButtonsCache[i];
                    merged[i] = btn != null && btn.interactable;
                }
            }

            _uiButtonsInteractableCache = merged;
        }

        if (previousActives != null && _uiButtonsCache != null)
        {
            var mergedActive = new bool[_uiButtonsCache.Length];
            for (int i = 0; i < _uiButtonsCache.Length; i++)
            {
                if (i < previousActives.Length)
                {
                    mergedActive[i] = previousActives[i];
                }
                else
                {
                    var btn = _uiButtonsCache[i];
                    mergedActive[i] = btn != null && btn.gameObject.activeSelf;
                }
            }

            _uiButtonsActiveCache = mergedActive;
        }

        Debug.Log("[TutorialManager] RebuildUIButtonCache dipanggil.");
    }
    private void CacheAllButtonsForHotelMode()
    {
        if (_currentMode != TutorialMode.Hotel)
            return;

        var allButtons = Object.FindObjectsOfType<Button>(true);
        var list = new List<Button>();
        foreach (var btn in allButtons)
        {
            if (btn != null && !list.Contains(btn))
                list.Add(btn);
        }
        _uiButtonsCache = list.ToArray();
        _uiButtonsInteractableCache = new bool[_uiButtonsCache.Length];
        _uiButtonsActiveCache = new bool[_uiButtonsCache.Length];
        for (int i = 0; i < _uiButtonsCache.Length; i++)
        {
            var btn = _uiButtonsCache[i];
            _uiButtonsInteractableCache[i] = btn != null && btn.interactable;
            _uiButtonsActiveCache[i] = btn != null && btn.gameObject.activeSelf;
        }
        Debug.Log($"[TutorialManager] CacheAllButtonsForHotelMode: {_uiButtonsCache.Length} button(s) cached for hotel mode.");
    }

    public void CacheUIButtonsFromUIManager()
    {
        Debug.Log("[PlainTutorial] CacheUIButtonsFromUIManager CALLED");
        var list = new List<Button>();
        void Add(Button b)
        {
            if (b != null && !list.Contains(b))
                list.Add(b);
        }

        if (_currentMode == TutorialMode.Plain)
        {
            var ui = ServiceLocator.Get<UIManager>();
            if (ui == null)
            {
                Debug.LogWarning("TutorialManager: UIManager tidak ditemukan saat cache buttons.");
            }
            else
            {
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

                CacheButtonsFromSceneSources(list);
            }
        }

        if (manualUIButtonExtras != null)
        {
            foreach (var extra in manualUIButtonExtras)
            {
                Add(extra);
            }
        }

        _uiButtonsCache = list.ToArray();
        Debug.Log($"[PlainTutorial] UIButtonCache BUILT: {_uiButtonsCache.Length} button(s)");
        _uiButtonsInteractableCache = new bool[_uiButtonsCache.Length];
        _uiButtonsActiveCache = new bool[_uiButtonsCache.Length];
        for (int i = 0; i < _uiButtonsCache.Length; i++)
        {
            var btn = _uiButtonsCache[i];
            _uiButtonsInteractableCache[i] = btn != null && btn.interactable;
            _uiButtonsActiveCache[i] = btn != null && btn.gameObject.activeSelf;
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
        if (_currentMode == TutorialMode.Plain)
        {
            if (_uiButtonsCache == null || _uiButtonsCache.Length == 0)
            {
                CacheUIButtonsFromUIManager();
            }
        }
        else if (_currentMode == TutorialMode.Hotel)
        {
            if (_uiButtonsCache == null || _uiButtonsCache.Length == 0)
            {
                CacheAllButtonsForHotelMode();
            }
        }
        else
        {
            return;
        }

        if (_uiButtonsCache == null || _uiButtonsCache.Length == 0)
            return;

        for (int i = 0; i < _uiButtonsCache.Length; i++)
        {
            var btn = _uiButtonsCache[i];
            if (btn == null)
                continue;

            if (_currentMode == TutorialMode.Plain && IsTutorialControlButton(btn))
                continue;

            if (_currentMode == TutorialMode.Hotel && IsHotelTutorialControlButton(btn))
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

                // Restore active state juga
                if (_uiButtonsActiveCache != null && i < _uiButtonsActiveCache.Length)
                {
                    btn.gameObject.SetActive(_uiButtonsActiveCache[i]);
                }
            }
        }

        if (_tutorialNextButton != null)
        {
            _tutorialNextButton.interactable = false;
        }

        _uiButtonsCache = null;
        _uiButtonsInteractableCache = null;
        _uiButtonsActiveCache = null;
    }

    private bool IsTutorialControlButton(Button btn)
    {
        if (btn == null)
            return false;

        if (btn == skipTutorialButton)
            return true;

        if (plainTutorials != null)
        {
            for (int i = 0; i < plainTutorials.Count; i++)
            {
                var plainStep = plainTutorials[i];
                var config = plainStep != null ? plainStep.config : null;
                if (config != null && config.nextButtonIndex >= 0 &&
                    _uiButtonsCache != null && config.nextButtonIndex < _uiButtonsCache.Length &&
                    _uiButtonsCache[config.nextButtonIndex] == btn)
                    return true;
            }
        }

        return false;
    }

    private bool IsHotelTutorialControlButton(Button btn)
    {
        if (btn == null)
            return false;

        if (btn == skipTutorialButton)
            return true;

        if (hotelTutorials != null)
        {
            for (int i = 0; i < hotelTutorials.Count; i++)
            {
                var hotelStep = hotelTutorials[i];
                var config = hotelStep != null ? hotelStep.config : null;
                if (config == null)
                    continue;

                if (!string.IsNullOrEmpty(config.nextButtonName) &&
                    btn.gameObject.name == config.nextButtonName)
                    return true;
            }
        }

        return false;
    }

    private Button GetPlainStepNextButton(PlainTutorialPanelStep step)
    {
        if (step == null)
        {
            Debug.LogError("[PlainTutorial] GetNextButton: step NULL");
            return null;
        }

        var config = step.config;
        if (config == null)
        {
            Debug.LogError("[PlainTutorial] GetNextButton: config NULL");
            return null;
        }

        if (_uiButtonsCache == null || _uiButtonsCache.Length == 0)
        {
            Debug.Log("[PlainTutorial] UIButtonCache empty → rebuilding");
            CacheUIButtonsFromUIManager();
        }

        if (_uiButtonsCache == null || _uiButtonsCache.Length == 0)
        {
            Debug.LogError("[PlainTutorial] UIButtonCache STILL EMPTY");
            return null;
        }

        if (config.nextButtonIndex < 0 || config.nextButtonIndex >= _uiButtonsCache.Length)
        {
            Debug.LogError(
                $"[PlainTutorial] INVALID nextButtonIndex={config.nextButtonIndex}, cacheSize={_uiButtonsCache.Length}"
            );
            return null;
        }

        var btn = _uiButtonsCache[config.nextButtonIndex];
        Debug.Log($"[PlainTutorial] NextButton RESOLVED: {btn.name}");
        return btn;
    }


    public void DebugLogAssignNextButton(PlainTutorialPanelStep step, Button btn)
    {
        if (btn == null)
        {
            Debug.LogWarning($"[TutorialManager] Next button untuk step {step?.panelRoot?.name ?? "NULL"} tidak ditemukan!");
            return;
        }
        Debug.Log($"[TutorialManager] Assign onClick untuk next button '{btn.gameObject.name}' pada step '{step?.panelRoot?.name ?? "NULL"}'");
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() =>
        {
            Debug.Log($"[TutorialManager] Next button '{btn.gameObject.name}' diklik pada step '{step?.panelRoot?.name ?? "NULL"}'");
            ShowNextPlainPanel();
        });
    }

    public Button TryGetUIButtonByIndex(int index)
    {
        if (_uiButtonsCache == null || index < 0 || index >= _uiButtonsCache.Length)
            return null;
        return _uiButtonsCache[index];
    }

    public Button ResolveHotelButtonByName(string buttonName)
    {
        Debug.Log($"[HandPointerTutorial] ResolveHotelButtonByName: Searching for button with name '{buttonName}'");

        if (string.IsNullOrEmpty(buttonName))
        {
            Debug.LogWarning("[HandPointerTutorial] ResolveHotelButtonByName: buttonName is null or empty");
            return null;
        }

        if (_currentMode == TutorialMode.Hotel &&
            hotelTutorials != null &&
            _hotelPanelIndex >= 0 &&
            _hotelPanelIndex < hotelTutorials.Count)
        {
            Debug.Log($"[HandPointerTutorial] ResolveHotelButtonByName: Searching in current hotel panel (index={_hotelPanelIndex})");
            var step = hotelTutorials[_hotelPanelIndex];
            if (step != null && step.panelRoot != null)
            {
                var buttons = step.panelRoot.GetComponentsInChildren<Button>(true);
                Debug.Log($"[HandPointerTutorial] ResolveHotelButtonByName: Found {buttons.Length} buttons in panel '{step.panelRoot.name}'");
                for (int i = 0; i < buttons.Length; i++)
                {
                    var btn = buttons[i];
                    if (btn != null && btn.gameObject.name == buttonName)
                    {
                        Debug.Log($"[HandPointerTutorial] ResolveHotelButtonByName: Button FOUND in hotel panel - name='{btn.name}', index={i}, active={btn.gameObject.activeSelf}, interactable={btn.interactable}");
                        return btn;
                    }
                }
                Debug.Log($"[HandPointerTutorial] ResolveHotelButtonByName: Button '{buttonName}' NOT FOUND in hotel panel, trying global cache");
            }
            else
            {
                Debug.LogWarning("[HandPointerTutorial] ResolveHotelButtonByName: Hotel step or panelRoot is null");
            }
        }
        else
        {
            Debug.Log($"[HandPointerTutorial] ResolveHotelButtonByName: Not in hotel mode or invalid panel index, trying global cache. Mode={_currentMode}, PanelIndex={_hotelPanelIndex}");
        }

        var result = GetButtonByName(buttonName);
        if (result != null)
        {
            Debug.Log($"[HandPointerTutorial] ResolveHotelButtonByName: Button FOUND in global cache - name='{result.name}'");
        }
        else
        {
            Debug.LogWarning($"[HandPointerTutorial] ResolveHotelButtonByName: Button '{buttonName}' NOT FOUND anywhere");
        }
        return result;
    }
}
