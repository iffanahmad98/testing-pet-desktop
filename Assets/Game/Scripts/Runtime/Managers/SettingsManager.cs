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
            RepositionFoodsAfterScaling();
            RepositionMedicinesAfterScaling();
            RepositionCoinsAfterScaling();
            RepositionPoopsAfterScaling();
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
            RepositionFoodsAfterScaling();
            RepositionMedicinesAfterScaling();
            RepositionCoinsAfterScaling();
            RepositionPoopsAfterScaling();
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
        if (canvasScaler == null || canvasScaler.scaleFactor <= 0.7f || canvasScaler.scaleFactor > 1.1f) return;
        uiScale = Mathf.Clamp(uiScale + delta, 0.2f, 2f);
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

            // Calculate ground area bounds (same as MonsterBoundsHandler)
            const float PADDING = 10f;
            const float GROUND_AREA_HEIGHT_RATIO = 0.4f;
            const float MIN_MOVEMENT_DISTANCE = 10f;

            Vector2 boundsMin = new Vector2(
                -gameAreaSize.x / 2 + monsterHalfWidth + PADDING,
                -gameAreaSize.y / 2 + monsterHalfHeight + PADDING
            );

            Vector2 boundsMax = new Vector2(
                gameAreaSize.x / 2 - monsterHalfWidth - PADDING,
                -gameAreaSize.y / 2 + (gameAreaSize.y * GROUND_AREA_HEIGHT_RATIO) - monsterHalfHeight
            );

            // Determine Y position based on game area height
            float newY;
            if (gameArea.sizeDelta.y > initialGameAreaHeight / 2f)
            {
                // Random Y within ground area when height is above half
                newY = Random.Range(boundsMin.y, boundsMax.y);
            }
            else
            {
                // Center Y when height is below or equal to half
                newY = (boundsMin.y + boundsMax.y) / 2f;
            }

            Vector2 newPos = new Vector2(
                Mathf.Clamp(currentPos.x, boundsMin.x, boundsMax.x),
                newY
            );

            rectTransform.anchoredPosition = newPos;

            // Set walking state to make monster move
            if (monster.StateMachine != null)
            {
                monster.StateMachine.ChangeState(MonsterState.Walking);
            }
            else
            {
                Debug.LogWarning($"Failed to change state for monster {monster.name}: StateMachine is null");
            }

            // Generate target with minimum distance from current position
            Vector2 targetPosition;

            if (gameArea.sizeDelta.y > initialGameAreaHeight / 2f)
            {
                // Diagonal movement: generate random X and Y target
                float targetX;
                float targetY;
                int attempts = 0;
                int maxAttempts = 10;

                do
                {
                    targetX = Random.Range(boundsMin.x, boundsMax.x);
                    targetY = Random.Range(boundsMin.y, boundsMax.y);
                    attempts++;
                }
                while (Vector2.Distance(new Vector2(targetX, targetY), newPos) < MIN_MOVEMENT_DISTANCE && attempts < maxAttempts);

                // If still too close after max attempts, force minimum distance
                if (Vector2.Distance(new Vector2(targetX, targetY), newPos) < MIN_MOVEMENT_DISTANCE)
                {
                    // Try moving right and up
                    targetX = newPos.x + MIN_MOVEMENT_DISTANCE;
                    targetY = newPos.y + MIN_MOVEMENT_DISTANCE;

                    // Clamp to bounds
                    targetX = Mathf.Clamp(targetX, boundsMin.x, boundsMax.x);
                    targetY = Mathf.Clamp(targetY, boundsMin.y, boundsMax.y);
                }

                targetPosition = new Vector2(targetX, targetY);
            }
            else
            {
                // Horizontal only movement
                float targetX;
                int attempts = 0;
                int maxAttempts = 10;

                do
                {
                    targetX = Random.Range(boundsMin.x, boundsMax.x);
                    attempts++;
                }
                while (Mathf.Abs(targetX - newPos.x) < MIN_MOVEMENT_DISTANCE && attempts < maxAttempts);

                // If still too close after max attempts, force minimum distance
                if (Mathf.Abs(targetX - newPos.x) < MIN_MOVEMENT_DISTANCE)
                {
                    targetX = newPos.x + MIN_MOVEMENT_DISTANCE;
                    if (targetX > boundsMax.x)
                        targetX = newPos.x - MIN_MOVEMENT_DISTANCE;
                    targetX = Mathf.Clamp(targetX, boundsMin.x, boundsMax.x);
                }

                targetPosition = new Vector2(targetX, newPos.y);
            }

            monster.SetTargetPosition(targetPosition);

            // Debug log for movement target
            float distance = Vector2.Distance(targetPosition, newPos);
            Debug.Log($"Monster {monster.name} repositioned - Current: {newPos}, Target: {targetPosition}, Distance: {distance:F2}");
        }
    }

    private void RepositionFoodsAfterScaling()
    {
        if (gameManager?.activeFoods == null) return;

        foreach (var food in gameManager.activeFoods)
        {
            if (food == null) continue;

            var rectTransform = food.GetComponent<RectTransform>();
            Vector2 currentPos = rectTransform.anchoredPosition;

            // Use proper bounds calculation like MonsterMovementBounds does
            Vector2 gameAreaSize = gameArea.sizeDelta;
            float foodHalfWidth = rectTransform.rect.width / 2;
            float foodHalfHeight = rectTransform.rect.height / 2;

            // Calculate ground area bounds (same as MonsterBoundsHandler)
            const float PADDING = 10f;
            const float GROUND_AREA_HEIGHT_RATIO = 0.4f;

            Vector2 boundsMin = new Vector2(
                -gameAreaSize.x / 2 + foodHalfWidth + PADDING,
                -gameAreaSize.y / 2 + foodHalfHeight + PADDING
            );

            Vector2 boundsMax = new Vector2(
                gameAreaSize.x / 2 - foodHalfWidth - PADDING,
                -gameAreaSize.y / 2 + (gameAreaSize.y * GROUND_AREA_HEIGHT_RATIO) - foodHalfHeight
            );

            // Determine Y position based on game area height
            float newY;
            if (gameArea.sizeDelta.y > initialGameAreaHeight / 2f)
            {
                // Random Y within ground area when height is above half
                newY = Random.Range(boundsMin.y, boundsMax.y);
            }
            else
            {
                // Center Y when height is below or equal to half
                newY = (boundsMin.y + boundsMax.y) / 2f;
            }

            Vector2 newPos = new Vector2(
                Mathf.Clamp(currentPos.x, boundsMin.x, boundsMax.x),
                newY
            );

            rectTransform.anchoredPosition = newPos;

            // Debug log for food repositioning
            Debug.Log($"Food repositioned - Old: {currentPos}, New: {newPos}");
        }
    }

    private void RepositionMedicinesAfterScaling()
    {
        if (gameManager?.activeMedicines == null) return;

        foreach (var medicine in gameManager.activeMedicines)
        {
            if (medicine == null) continue;

            var rectTransform = medicine.GetComponent<RectTransform>();
            Vector2 currentPos = rectTransform.anchoredPosition;

            // Use proper bounds calculation like MonsterMovementBounds does
            Vector2 gameAreaSize = gameArea.sizeDelta;
            float medicineHalfWidth = rectTransform.rect.width / 2;
            float medicineHalfHeight = rectTransform.rect.height / 2;

            // Calculate ground area bounds (same as MonsterBoundsHandler)
            const float PADDING = 10f;
            const float GROUND_AREA_HEIGHT_RATIO = 0.4f;

            Vector2 boundsMin = new Vector2(
                -gameAreaSize.x / 2 + medicineHalfWidth + PADDING,
                -gameAreaSize.y / 2 + medicineHalfHeight + PADDING
            );

            Vector2 boundsMax = new Vector2(
                gameAreaSize.x / 2 - medicineHalfWidth - PADDING,
                -gameAreaSize.y / 2 + (gameAreaSize.y * GROUND_AREA_HEIGHT_RATIO) - medicineHalfHeight
            );

            // Determine Y position based on game area height
            float newY;
            if (gameArea.sizeDelta.y > initialGameAreaHeight / 2f)
            {
                // Random Y within ground area when height is above half
                newY = Random.Range(boundsMin.y, boundsMax.y);
            }
            else
            {
                // Center Y when height is below or equal to half
                newY = (boundsMin.y + boundsMax.y) / 2f;
            }

            Vector2 newPos = new Vector2(
                Mathf.Clamp(currentPos.x, boundsMin.x, boundsMax.x),
                newY
            );

            rectTransform.anchoredPosition = newPos;

            // Debug log for medicine repositioning
            Debug.Log($"Medicine repositioned - Old: {currentPos}, New: {newPos}");
        }
    }

    private void RepositionCoinsAfterScaling()
    {
        if (gameManager?.activeCoins == null) return;

        foreach (var coin in gameManager.activeCoins)
        {
            if (coin == null) continue;

            var rectTransform = coin.GetComponent<RectTransform>();
            Vector2 currentPos = rectTransform.anchoredPosition;

            // Use proper bounds calculation like MonsterMovementBounds does
            Vector2 gameAreaSize = gameArea.sizeDelta;
            float coinHalfWidth = rectTransform.rect.width / 2;
            float coinHalfHeight = rectTransform.rect.height / 2;

            // Calculate ground area bounds (same as MonsterBoundsHandler)
            const float PADDING = 10f;
            const float GROUND_AREA_HEIGHT_RATIO = 0.4f;

            Vector2 boundsMin = new Vector2(
                -gameAreaSize.x / 2 + coinHalfWidth + PADDING,
                -gameAreaSize.y / 2 + coinHalfHeight + PADDING
            );

            Vector2 boundsMax = new Vector2(
                gameAreaSize.x / 2 - coinHalfWidth - PADDING,
                -gameAreaSize.y / 2 + (gameAreaSize.y * GROUND_AREA_HEIGHT_RATIO) - coinHalfHeight
            );

            // Determine Y position based on game area height
            float newY;
            if (gameArea.sizeDelta.y > initialGameAreaHeight / 2f)
            {
                // Random Y within ground area when height is above half
                newY = Random.Range(boundsMin.y, boundsMax.y);
            }
            else
            {
                // Center Y when height is below or equal to half
                newY = (boundsMin.y + boundsMax.y) / 2f;
            }

            Vector2 newPos = new Vector2(
                Mathf.Clamp(currentPos.x, boundsMin.x, boundsMax.x),
                newY
            );

            rectTransform.anchoredPosition = newPos;

            // Debug log for coin repositioning
            Debug.Log($"Coin repositioned - Old: {currentPos}, New: {newPos}");
        }
    }

    private void RepositionPoopsAfterScaling()
    {
        if (gameManager?.activePoops == null) return;

        foreach (var poop in gameManager.activePoops)
        {
            if (poop == null) continue;

            var rectTransform = poop.GetComponent<RectTransform>();
            Vector2 currentPos = rectTransform.anchoredPosition;

            // Use proper bounds calculation like MonsterMovementBounds does
            Vector2 gameAreaSize = gameArea.sizeDelta;
            float poopHalfWidth = rectTransform.rect.width / 2;
            float poopHalfHeight = rectTransform.rect.height / 2;

            // Calculate ground area bounds (same as MonsterBoundsHandler)
            const float PADDING = 10f;
            const float GROUND_AREA_HEIGHT_RATIO = 0.4f;

            Vector2 boundsMin = new Vector2(
                -gameAreaSize.x / 2 + poopHalfWidth + PADDING,
                -gameAreaSize.y / 2 + poopHalfHeight + PADDING
            );

            Vector2 boundsMax = new Vector2(
                gameAreaSize.x / 2 - poopHalfWidth - PADDING,
                -gameAreaSize.y / 2 + (gameAreaSize.y * GROUND_AREA_HEIGHT_RATIO) - poopHalfHeight
            );

            // Determine Y position based on game area height
            float newY;
            if (gameArea.sizeDelta.y > initialGameAreaHeight / 2f)
            {
                // Random Y within ground area when height is above half
                newY = Random.Range(boundsMin.y, boundsMax.y);
            }
            else
            {
                // Center Y when height is below or equal to half
                newY = (boundsMin.y + boundsMax.y) / 2f;
            }

            Vector2 newPos = new Vector2(
                Mathf.Clamp(currentPos.x, boundsMin.x, boundsMax.x),
                newY
            );

            rectTransform.anchoredPosition = newPos;

            // Debug log for poop repositioning
            Debug.Log($"Poop repositioned - Old: {currentPos}, New: {newPos}");
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