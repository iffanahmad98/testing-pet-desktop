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

    [Header("Game Area Controls")]
    public SliderSettingControl widthControl;
    public SliderSettingControl heightControl;
    public SliderSettingControl horizontalPositionControl;
    public SliderSettingControl verticalPositionControl;

    [Header("UI Size")]
    public Button uiSizeIncreaseButton;
    public Button uiSizeDecreaseButton;
    public Button uiSizeResetButton;
    
    [Header("Pet Size")]
    public Button petSizeIncreaseButton;
    public Button petSizeDecreaseButton;
    public Button petSizeResetButton;

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
    private float petScale = 1f; // pet scaling factor

    private const float MIN_SIZE = 270f;
    private const float DEFAULT_GAME_AREA_WIDTH = 1920f;
    private const float DEFAULT_GAME_AREA_HEIGHT = 1080f;
    private const float DEFAULT_UI_SCALE = 1f;
    private const float DEFAULT_PET_SCALE = 1f;
    private const float MIN_PET_SCALE = 0.25f;
    private const float MAX_PET_SCALE = 1.5f;
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
    private float savedPetScale;
    private int savedLanguageIndex;
    private List<ISettingsSavable> savableSettingsModules = new List<ISettingsSavable>();

    private void Awake()
    {
        ServiceLocator.Register(this);
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
        // UI Scale buttons
        uiSizeIncreaseButton.onClick.AddListener(() => AdjustUIScale(0.05f));
        uiSizeDecreaseButton.onClick.AddListener(() => AdjustUIScale(-0.05f));
        uiSizeResetButton.onClick.AddListener(() => ResetUIScale());
        
        // Pet Scale buttons
        if (petSizeIncreaseButton != null)
            petSizeIncreaseButton.onClick.AddListener(() => AdjustPetScale(0.05f));
        if (petSizeDecreaseButton != null)
            petSizeDecreaseButton.onClick.AddListener(() => AdjustPetScale(-0.05f));
        if (petSizeResetButton != null)
            petSizeResetButton.onClick.AddListener(() => ResetPetScale());
            
        // Other buttons
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
        
        if (petSizeIncreaseButton != null)
            petSizeIncreaseButton.onClick.RemoveAllListeners();
        if (petSizeDecreaseButton != null)
            petSizeDecreaseButton.onClick.RemoveAllListeners();
        if (petSizeResetButton != null)
            petSizeResetButton.onClick.RemoveAllListeners();
            
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

    public void AdjustUIScale(float delta)
    {
        uiScale = Mathf.Clamp(uiScale + delta, 0.5f, 2f);
        canvasScaler.scaleFactor = uiScale;
    }

    public void ResetUIScale()
    {
        UpdateUIScale(DEFAULT_UI_SCALE);
    }

    public void UpdateUIScale(float value)
    {
        if (canvasScaler != null)
            canvasScaler.scaleFactor = value;
    }
    
    // Pet scaling methods
    public void AdjustPetScale(float delta)
    {
        petScale = Mathf.Clamp(petScale + delta, MIN_PET_SCALE, MAX_PET_SCALE);
        ApplyPetScaleToAllMonsters();
    }

    public void ResetPetScale()
    {
        UpdatePetScale(DEFAULT_PET_SCALE);
    }

    public void UpdatePetScale(float value)
    {
        petScale = Mathf.Clamp(value, MIN_PET_SCALE, MAX_PET_SCALE);
        ApplyPetScaleToAllMonsters();
    }

    private void ApplyPetScaleToAllMonsters()
    {
        if (gameManager?.activeMonsters == null) return;
        
        foreach (var monster in gameManager.activeMonsters)
        {
            if (monster != null)
            {
                monster.transform.localScale = Vector3.one * petScale;
            }
        }
    }
    
    public void ApplyCurrentPetScaleToMonster(MonsterController monster)
    {
        if (monster != null)
        {
            monster.transform.localScale = Vector3.one * petScale;
        }
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
        // Cache saved values for Cancel
        savedGameAreaWidth = PlayerPrefs.GetFloat("GameAreaWidth", DEFAULT_GAME_AREA_WIDTH);
        savedGameAreaHeight = PlayerPrefs.GetFloat("GameAreaHeight", DEFAULT_GAME_AREA_HEIGHT);
        savedGameAreaX = PlayerPrefs.GetFloat("GameAreaX", 0f);
        savedGameAreaY = PlayerPrefs.GetFloat("GameAreaY", -500f);
        savedUIScale = PlayerPrefs.GetFloat("UIScale", DEFAULT_UI_SCALE);
        savedPetScale = PlayerPrefs.GetFloat("PetScale", DEFAULT_PET_SCALE);
        savedLanguageIndex = PlayerPrefs.GetInt("Language", 0);
        
        Debug.Log($"Loaded settings: Width={savedGameAreaWidth}, Height={savedGameAreaHeight}, X={savedGameAreaX}, Y={savedGameAreaY}, UIScale={savedUIScale}, LanguageIndex={savedLanguageIndex}");

        // Apply values to game area and UI
        UpdateGameAreaWidth(savedGameAreaWidth);
        UpdateGameAreaHeight(savedGameAreaHeight);
        UpdateGameAreaHorizontalPosition(savedGameAreaX);
        UpdateGameAreaVerticalPosition(savedGameAreaY);
        UpdateUIScale(savedUIScale);
        UpdatePetScale(savedPetScale);

        // Update SliderSettingControl UI fields
        widthControl.SetValueWithoutNotify(savedGameAreaWidth);
        heightControl.SetValueWithoutNotify(savedGameAreaHeight);
        horizontalPositionControl.SetValueWithoutNotify(savedGameAreaX);
        verticalPositionControl.SetValueWithoutNotify(savedGameAreaY);

        // Set dropdown value without triggering change callback
        if (languageDropdown != null && savedLanguageIndex < languageDropdown.options.Count)
        {
            languageDropdown.SetValueWithoutNotify(savedLanguageIndex);
        }
    }

    private void OnSaveSettings()
    {
        PlayerPrefs.SetFloat("GameAreaWidth", gameArea.sizeDelta.x);
        PlayerPrefs.SetFloat("GameAreaHeight", gameArea.sizeDelta.y);
        PlayerPrefs.SetFloat("GameAreaX", gameArea.anchoredPosition.x);
        PlayerPrefs.SetFloat("GameAreaY", gameArea.anchoredPosition.y);
        PlayerPrefs.SetFloat("UIScale", uiScale);
        PlayerPrefs.SetFloat("PetScale", petScale);
        PlayerPrefs.SetInt("Language", languageDropdown.value);
        
        foreach (var module in savableSettingsModules)
            module.SaveSettings(); // Save each module's settings
            
        PlayerPrefs.Save();
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
        UpdatePetScale(savedPetScale);

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