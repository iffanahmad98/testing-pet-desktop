using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Comprehensive setup manager handling game area, audio, and language settings
/// </summary>
public class SettingsManager : MonoBehaviour
{
    [Header("Game Area References")]
    public RectTransform gameArea;
    public CanvasScaler canvasScaler;
    private MonsterManager gameManager;

    [Header("Game Area Incremental Control")]
    public IncrementSettingControl widthControl;
    public IncrementSettingControl heightControl;
    public IncrementSettingControl horizontalPositionControl;
    public IncrementSettingControl verticalPositionControl;

    [Header("UI Size")]
    public Button uiSizeIncreaseButton;
    public Button uiSizeDecreaseButton;
    public Button uiSizeResetButton;

    [Header("Switch Screen")]
    public Button switchScreenLeftButton;
    public Button switchScreenRightButton;
    public Button switchScreenResetButton;

    [Header("Settings Control Buttons")]
    public Button saveButton;
    public Button cancelButton;
    public Button newGameButton;


    [Header("Language Settings")]
    public TMP_Dropdown languageDropdown;
    public List<SystemLanguage> supportedLanguages = new List<SystemLanguage>
    {
        SystemLanguage.English,
        SystemLanguage.French,
        SystemLanguage.Spanish
    };
    [SerializeField] private GameObject settingPanel;
    [SerializeField] private Button closeButton;

    private float uiScale = 1f; // optionally make this persistent
    private int screenState = 0; // 0 = Center, -1 = Left, 1 = Right

    private const float MIN_SIZE = 270f;
    private const float DEFAULT_GAME_AREA_WIDTH = 1920f;
    private const float DEFAULT_GAME_AREA_HEIGHT = 1080f;
    private const float DEFAULT_UI_SCALE = 1f;
    private const float MONSTER_BOUNDS_PADDING = 50f;
    private float maxScreenWidth;
    private float maxScreenHeight;
    private float initialGameAreaHeight;
    private float _lastRepositionTime = 0f;
    private const float REPOSITION_COOLDOWN = 0.2f;

    [Header("Events")]
    public UnityEvent OnGameAreaChanged;

    [Header("Saved Settings")]
    private float savedGameAreaWidth;
    private float savedGameAreaHeight;
    private float savedGameAreaX;
    private float savedGameAreaY;
    private float savedUIScale;
    private int savedLanguageIndex;
    private int savedScreenState;
    private List<ISettingsSavable> savableSettingsModules = new List<ISettingsSavable>();


    private void Awake()
    {
        ServiceLocator.Register(this);
        SaveSystem.Initialize();
    }

    private void Start()
    {
        gameManager = ServiceLocator.Get<MonsterManager>();
        InitializeGameAreaSettings();
        InitializeLanguageSettings();


        // Discover all savable modules in scene
        savableSettingsModules.AddRange(
            FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).OfType<ISettingsSavable>()
        );
    }

    private void OnDestroy()
    {
        UnregisterAllCallbacks();
        ServiceLocator.Unregister<SettingsManager>();
    }


    private void InitializeGameAreaSettings()
    {
        if (!ValidateGameAreaReferences()) return;

        CacheScreenValues();
        InitializeGameAreaConfig();
        RegisterButtonCallbacks();
    }



    private void InitializeLanguageSettings()
    {
        if (languageDropdown == null) return;

        // Populate dropdown with supported languages
        languageDropdown.ClearOptions();
        List<string> languageOptions = new List<string>();

        foreach (var lang in supportedLanguages)
        {
            languageOptions.Add(lang.ToString());
        }

        languageDropdown.AddOptions(languageOptions);

        // Set current language from preferences
        int currentLangIndex = PlayerPrefs.GetInt("Language", 0);
        currentLangIndex = Mathf.Clamp(currentLangIndex, 0, supportedLanguages.Count - 1);
        languageDropdown.value = currentLangIndex;

        languageDropdown.onValueChanged.AddListener(OnLanguageChanged);
    }

    private bool ValidateGameAreaReferences()
    {
        if (gameArea == null) return false;
        if (canvasScaler == null) return false;
        return true;
    }

    private void CacheScreenValues()
    {
        maxScreenWidth = Screen.currentResolution.width;
        maxScreenHeight = Screen.currentResolution.height;
        initialGameAreaHeight = gameArea.sizeDelta.y;
    }

    private void InitializeGameAreaConfig()
    {
        widthControl.Initialize(
            defaultVal: DEFAULT_GAME_AREA_WIDTH,
            min: MIN_SIZE,
            max: maxScreenWidth
        );
        heightControl.Initialize(
            defaultVal: DEFAULT_GAME_AREA_HEIGHT,
            min: MIN_SIZE,
            max: maxScreenHeight
        );
        horizontalPositionControl.Initialize(
            defaultVal: 0f,
            min: -maxScreenWidth / 2f,
            max: maxScreenWidth / 2f
        );
        verticalPositionControl.Initialize(
            defaultVal: -500f,
            min: -maxScreenHeight / 2f,
            max: maxScreenHeight / 2f
        );
        // Load saved values or defaults to avoid null/zero on cancel
        LoadSavedSettings();


        RegisterGameAreaCallbacks();
    }


    #region Callback Registration
    private void RegisterGameAreaCallbacks()
    {
        widthControl.onValueChanged += UpdateGameAreaWidth;
        heightControl.onValueChanged += UpdateGameAreaHeight;
        horizontalPositionControl.onValueChanged += UpdateGameAreaHorizontalPosition;
        verticalPositionControl.onValueChanged += UpdateGameAreaVerticalPosition;

    }
    private void RegisterButtonCallbacks()
    {
        uiSizeIncreaseButton.onClick.AddListener(() => AdjustUIScale(0.05f));
        uiSizeDecreaseButton.onClick.AddListener(() => AdjustUIScale(-0.05f));
        uiSizeResetButton.onClick.AddListener(() => ResetUIScale());
        switchScreenLeftButton.onClick.AddListener(SwitchScreenLeft);
        switchScreenRightButton.onClick.AddListener(SwitchScreenRight);
        switchScreenResetButton.onClick.AddListener(ResetScreenLayout);
        saveButton.onClick.AddListener(OnSaveSettings);
        cancelButton.onClick.AddListener(OnCancelSettings);
        newGameButton.onClick.AddListener(OnNewGame);
        closeButton.onClick.AddListener(OnClickCloseButton);
    }



    private void UnregisterAllCallbacks()
    {
        uiSizeIncreaseButton.onClick.RemoveAllListeners();
        uiSizeDecreaseButton.onClick.RemoveAllListeners();
        uiSizeResetButton.onClick.RemoveAllListeners();
        switchScreenLeftButton.onClick.RemoveAllListeners();
        switchScreenRightButton.onClick.RemoveAllListeners();
        switchScreenResetButton.onClick.RemoveAllListeners();
        saveButton.onClick.RemoveAllListeners();
        cancelButton.onClick.RemoveAllListeners();
        newGameButton.onClick.RemoveAllListeners();

        // Language
        languageDropdown?.onValueChanged.RemoveListener(OnLanguageChanged);

    }
    #endregion

    #region Update Methods (modified to sync with input fields)
    public void UpdateGameAreaWidth(float value)
    {
        // Clamp already handled in IncrementSettingControl
        Vector2 size = gameArea.sizeDelta;
        size.x = value;
        gameArea.sizeDelta = size;

        // UpdateValueText(widthValueText, value, DECIMAL_FORMAT);
        // if (widthInputField != null) widthInputField.text = value.ToString(DECIMAL_FORMAT);

        if (Time.time - _lastRepositionTime > REPOSITION_COOLDOWN)
        {
            RepositionMonstersAfterScaling();
            _lastRepositionTime = Time.time;
        }

        OnGameAreaChanged?.Invoke();
    }


    public void UpdateGameAreaHeight(float value)
    {
        if (gameArea == null) return;

        value = Mathf.Clamp(value, MIN_SIZE, initialGameAreaHeight);

        // Store current bottom position before scaling
        float currentBottom = gameArea.anchoredPosition.y - (gameArea.sizeDelta.y * gameArea.pivot.y);

        // Update size
        Vector2 size = gameArea.sizeDelta;
        size.y = value;
        gameArea.sizeDelta = size;

        // Maintain bottom position by adjusting anchoredPosition
        Vector2 pos = gameArea.anchoredPosition;
        pos.y = currentBottom + (value * gameArea.pivot.y);
        gameArea.anchoredPosition = pos;

        if (Time.time - _lastRepositionTime > REPOSITION_COOLDOWN)
        {
            RepositionMonstersAfterScaling();
            _lastRepositionTime = Time.time;
        }

        OnGameAreaChanged?.Invoke();
    }

    public void UpdateGameAreaHorizontalPosition(float value)
    {
        if (gameArea == null) return;

        Vector2 pos = gameArea.anchoredPosition;
        pos.x = value;
        gameArea.anchoredPosition = pos;
    }
    public void UpdateGameAreaVerticalPosition(float value)
    {
        if (gameArea == null) return;

        Vector2 pos = gameArea.anchoredPosition;
        pos.y = value;
        gameArea.anchoredPosition = pos;
    }
    public void AdjustGameAreaSize(float delta)
    {
        if (gameArea == null) return;

        float currentHeight = gameArea.sizeDelta.y;
        float newHeight = Mathf.Clamp(currentHeight + delta, MIN_SIZE, maxScreenHeight);

        UpdateGameAreaHeight(newHeight);
    }

    public void AdjustUIScale(float delta)
    {
        uiScale = Mathf.Clamp(uiScale + delta, 0.5f, 2f);
        canvasScaler.scaleFactor = uiScale;
    }
    public void ResetGameAreaSize()
    {

        UpdateGameAreaHeight(DEFAULT_GAME_AREA_HEIGHT);
    }

    public void ResetUIScale()
    {
        UpdateUIScale(DEFAULT_UI_SCALE);
    }
    public void SwitchScreenLeft()
    {
        screenState--;
        ApplyScreenLayout(screenState);
    }

    public void SwitchScreenRight()
    {
        screenState++;
        ApplyScreenLayout(screenState);
    }

    public void ResetScreenLayout()
    {
        screenState = 0;
        ApplyScreenLayout(screenState);
    }

    private void ApplyScreenLayout(int layoutId)
    {
        // Your logic to adjust RectTransform layout or canvas anchoring
        Debug.Log("Applying screen layout: " + layoutId);

        // Example:
        // layoutId = -1 => left, 0 => center, 1 => right
        // Adjust anchoring, pivot, or camera accordingly
    }


    public void UpdateUIScale(float value)
    {
        if (canvasScaler != null)
            canvasScaler.scaleFactor = value;
    }
    #endregion

    #region Language Controls
    private void OnLanguageChanged(int index)
    {
        if (index < 0 || index >= supportedLanguages.Count) return;

        SystemLanguage selectedLanguage = supportedLanguages[index];
        PlayerPrefs.SetInt("Language", index);

        // Implement your language change logic here
        // e.g., LocalizationManager.Instance.SetLanguage(selectedLanguage);

        Debug.Log($"Language changed to: {selectedLanguage}");
    }
    #endregion
    
    private void RepositionMonstersAfterScaling()
    {
        if (gameManager?.activeMonsters == null) return;

        foreach (var monster in gameManager.activeMonsters)
        {
            if (monster == null) continue;

            var rectTransform = monster.GetComponent<RectTransform>();
            Vector2 currentPos = rectTransform.anchoredPosition;

            // Use proper bounds calculation like MonsterMovementBounds does
            Vector2 gameAreaSize = gameArea.sizeDelta;
            float monsterHalfWidth = rectTransform.rect.width / 2;
            float monsterHalfHeight = rectTransform.rect.height / 2;

            // Calculate actual movement bounds with consistent padding
            Vector2 boundsMin = new Vector2(
                -gameAreaSize.x / 2 + monsterHalfWidth + MONSTER_BOUNDS_PADDING,
                -gameAreaSize.y / 2 + monsterHalfHeight + MONSTER_BOUNDS_PADDING
            );

            Vector2 boundsMax = new Vector2(
                gameAreaSize.x / 2 - monsterHalfWidth - MONSTER_BOUNDS_PADDING,
                gameAreaSize.y / 2 - monsterHalfHeight - MONSTER_BOUNDS_PADDING
            );

            // Check if monster is outside new bounds
            bool isOutside = currentPos.x < boundsMin.x ||
                            currentPos.x > boundsMax.x ||
                            currentPos.y < boundsMin.y ||
                            currentPos.y > boundsMax.y;

            if (isOutside)
            {
                // Clamp to new bounds with consistent padding
                Vector2 newPos = new Vector2(
                    Mathf.Clamp(currentPos.x, boundsMin.x, boundsMax.x),
                    Mathf.Clamp(currentPos.y, boundsMin.y, boundsMax.y)
                );

                rectTransform.anchoredPosition = newPos;

                // Give monster new random target within new bounds
                monster.SetRandomTarget();
            }
        }
    }
    private void LoadSavedSettings()
    {
        var settings = SaveSystem.GetPlayerConfig().settings;

        // Cache saved values for Cancel
        savedGameAreaWidth = settings.gameAreaWidth;
        savedGameAreaHeight = settings.gameAreaHeight;
        savedGameAreaX = settings.gameAreaX;
        savedGameAreaY = settings.gameAreaY;
        savedUIScale = settings.uiScale;
        savedLanguageIndex = settings.languageIndex;
        savedScreenState = settings.screenState;

        // Apply values to game area and UI
        UpdateGameAreaWidth(settings.gameAreaWidth);
        UpdateGameAreaHorizontalPosition(settings.gameAreaX);
        UpdateGameAreaVerticalPosition(settings.gameAreaY);
        UpdateUIScale(settings.uiScale);
        ApplyScreenLayout(settings.screenState);

        // Update IncrementSettingControl UI fields
        widthControl.SetValueWithoutNotify(settings.gameAreaWidth);
        heightControl.SetValueWithoutNotify(settings.gameAreaHeight);
        horizontalPositionControl.SetValueWithoutNotify(settings.gameAreaX);
        verticalPositionControl.SetValueWithoutNotify(settings.gameAreaY);

        // Set dropdown value without triggering change callback
        if (languageDropdown != null && settings.languageIndex < languageDropdown.options.Count)
        {
            languageDropdown.SetValueWithoutNotify(settings.languageIndex);
        }

        // Also update screenState field (used in ApplyScreenLayout logic)
        screenState = settings.screenState;
    }


    private void OnSaveSettings()
    {
        var settings = SaveSystem.GetPlayerConfig().settings;
        Debug.Log(settings);

        settings.gameAreaWidth = gameArea.sizeDelta.x;
        settings.gameAreaX = gameArea.anchoredPosition.x;
        settings.gameAreaY = gameArea.anchoredPosition.y;
        settings.uiScale = uiScale;
        // settings.languageIndex = languageDropdown.value;
        settings.screenState = screenState;
        foreach (var module in savableSettingsModules)
            module.SaveSettings(); // Save each module's settings

        SaveSystem.SaveAll(); // This will serialize PlayerConfig to file
    }


    private void OnCancelSettings()
    {
        // Revert UI/Game area settings
        UpdateGameAreaWidth(savedGameAreaWidth);
        UpdateGameAreaHeight(savedGameAreaHeight);
        UpdateGameAreaHorizontalPosition(savedGameAreaX);
        UpdateGameAreaVerticalPosition(savedGameAreaY);
        widthControl.SetValueWithoutNotify(savedGameAreaWidth);
        heightControl.SetValueWithoutNotify(savedGameAreaHeight);
        horizontalPositionControl.SetValueWithoutNotify(savedGameAreaX);
        verticalPositionControl.SetValueWithoutNotify(savedGameAreaY);

        UpdateUIScale(savedUIScale);
        // languageDropdown.value = savedLanguageIndex;
        screenState = savedScreenState;
        ApplyScreenLayout(screenState);

        foreach (var module in savableSettingsModules)
            module.RevertSettings();

        Debug.Log("Settings reverted.");
    }

    private void OnNewGame()
    {
        Debug.Log("Starting new game...");

        foreach (var module in savableSettingsModules)
            module.SaveSettings(); // optionally save before new game
        // gameManager?.StartNewGame(); // Or your scene load logic
    }
    private void OnClickCloseButton()
    {
        settingPanel.SetActive(false); // Hide settings panel
    }

    public float GetMinGameAreaHeight()
    {
        return MIN_SIZE;
    }

    public float GetMaxGameAreaHeight()
    {
        return DEFAULT_GAME_AREA_HEIGHT;
    }
}