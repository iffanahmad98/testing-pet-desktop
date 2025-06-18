using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Comprehensive setup manager handling game area, audio, and language settings
/// </summary>
public class SettingsManager : MonoBehaviour
{
    [Header("Game Area References")]
    public RectTransform gameArea;
    public CanvasScaler canvasScaler;
    private GameManager gameManager;

    [Header("Game Area Incremental Control")]
    public IncrementSettingControl widthControl;
    public IncrementSettingControl horizontalPositionControl;
    public IncrementSettingControl heightPositionControl;
    [Header("Game Area Size")]
    public Button gameAreaSizeIncreaseButton;
    public Button gameAreaSizeDecreaseButton;
    public Button gameAreaSizeResetButton;
    [Header("UI Size")]
    public Button uiSizeIncreaseButton;
    public Button uiSizeDecreaseButton;
    public Button uiSizeResetButton;
    [Header("Switch Screen")]
    public Button switchScreenLeftButton;
    public Button switchScreenRightButton;
    public Button switchScreenResetButton;


    [Header("Panel References")]
    public GameObject gameAreaPanel;
    public GameObject audioLanguagePanel;

    [Header("Language Settings")]
    public TMP_Dropdown languageDropdown;
    public List<SystemLanguage> supportedLanguages = new List<SystemLanguage>
    {
        SystemLanguage.English,
        SystemLanguage.French,
        SystemLanguage.Spanish
    };

    private float uiScale = 1f; // optionally make this persistent
    private int screenState = 0; // 0 = Center, -1 = Left, 1 = Right

    private const float MIN_SIZE = 270f;
    private const float DEFAULT_WIDTH_SIZE = 1920f;
    private const float DEFAULT_GAME_AREA_WIDTH = 1920f;
    private const float DEFAULT_GAME_AREA_HEIGHT = 1080f;
    private const float DEFAULT_UI_SCALE = 1f;

    private const float DEFAULT_MONSTER_SCALE = 1f;
    private const float MONSTER_BOUNDS_PADDING = 50f;
    private const string DECIMAL_FORMAT = "F0";
    private const string SCALE_FORMAT = "F2";
    private const string VOLUME_FORMAT = "F1";
    private const float MIN_VOLUME_DB = -80f;
    private const float MAX_VOLUME_DB = 0f;

    private float maxScreenWidth;
    private float maxScreenHeight;
    private float initialGameAreaHeight;
    private Vector2 cachedPosition;
    [Header("Events")]
    public UnityEvent OnGameAreaChanged;

    private void Awake()
    {
        ServiceLocator.Register(this);
    }
    private void Start()
    {
        gameManager = ServiceLocator.Get<GameManager>();
        InitializeGameAreaSettings();
        InitializeLanguageSettings();


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
        // InitializeInputFields();
        // SetInitialGameAreaValues();
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
        widthControl.Initialize(DEFAULT_WIDTH_SIZE, MIN_SIZE, maxScreenWidth);
        horizontalPositionControl.Initialize(
            defaultVal: 0f,
            min: -maxScreenWidth / 2f,
            max: maxScreenWidth / 2f
        );
        heightPositionControl.Initialize(
            defaultVal: 0f,
            min: -maxScreenHeight / 2f,
            max: maxScreenHeight / 2f
        );


        RegisterGameAreaCallbacks();
    }
    // private void InitializeInputFields()
    // {
    //     // Set initial values
    //     if (widthInputField != null)
    //     {
    //         widthInputField.text = gameArea.sizeDelta.x.ToString(DECIMAL_FORMAT);
    //         widthInputField.onEndEdit.AddListener(OnWidthInputChanged);
    //     }

    //     if (heightInputField != null)
    //     {
    //         heightInputField.text = gameArea.sizeDelta.y.ToString(DECIMAL_FORMAT);
    //         heightInputField.onEndEdit.AddListener(OnHeightInputChanged);
    //     }

    //     if (horizontalPositionInputField != null)
    //     {
    //         horizontalPositionInputField.text = gameArea.anchoredPosition.x.ToString(DECIMAL_FORMAT);
    //         horizontalPositionInputField.onEndEdit.AddListener(OnHorizontalPosInputChanged);
    //     }

    //     if (verticalPositionInputField != null)
    //     {
    //         verticalPositionInputField.text = gameArea.anchoredPosition.y.ToString(DECIMAL_FORMAT);
    //         verticalPositionInputField.onEndEdit.AddListener(OnVerticalPosInputChanged);
    //     }

    //     if (monsterScaleInputField != null)
    //     {
    //         monsterScaleInputField.text = DEFAULT_MONSTER_SCALE.ToString(SCALE_FORMAT);
    //         monsterScaleInputField.onEndEdit.AddListener(OnMonsterScaleInputChanged);
    //     }

    //     if (uiScaleInputField != null)
    //     {
    //         uiScaleInputField.text = canvasScaler.scaleFactor.ToString(SCALE_FORMAT);
    //         uiScaleInputField.onEndEdit.AddListener(OnUIScaleInputChanged);
    //     }
    //     // Register input field callbacks
    //     RegisterInputFieldCallbacks();
    // }

    // private void SetInitialGameAreaValues()
    // {
    //     if (gameArea == null) return;
    //     if (widthSlider != null) widthSlider.value = gameArea.sizeDelta.x;
    //     if (heightSlider != null) heightSlider.value = gameArea.sizeDelta.y;
    //     if (horizontalPositionSlider != null) horizontalPositionSlider.value = gameArea.anchoredPosition.x;
    //     if (verticalPositionSlider != null) verticalPositionSlider.value = gameArea.anchoredPosition.y;
    //     if (monsterScaleSlider != null) monsterScaleSlider.value = DEFAULT_MONSTER_SCALE;
    //     if (uiScaleSlider != null && canvasScaler != null) uiScaleSlider.value = canvasScaler.scaleFactor;

    //     // Update text displays
    //     UpdateValueText(widthValueText, gameArea.sizeDelta.x, DECIMAL_FORMAT);
    //     UpdateValueText(heightValueText, gameArea.sizeDelta.y, DECIMAL_FORMAT);
    //     UpdateValueText(horizontalPositionValueText, gameArea.anchoredPosition.x, DECIMAL_FORMAT); UpdateValueText(verticalPositionValueText, gameArea.anchoredPosition.y, DECIMAL_FORMAT);
    // }

    #region Input Field Handlers
    // private void OnWidthInputChanged(string value)
    // {
    //     if (float.TryParse(value, out float result))
    //     {
    //         result = Mathf.Clamp(result, MIN_SIZE, maxScreenWidth);
    //         widthSlider.value = result;
    //         UpdateGameAreaWidth(result);
    //     }
    //     else
    //     {
    //         widthInputField.text = gameArea.sizeDelta.x.ToString(DECIMAL_FORMAT);
    //     }
    // }

    // private void OnHeightInputChanged(string value)
    // {
    //     if (float.TryParse(value, out float result))
    //     {
    //         result = Mathf.Clamp(result, MIN_SIZE, initialGameAreaHeight);
    //         heightSlider.value = result;
    //         UpdateGameAreaHeight(result);
    //     }
    //     else
    //     {
    //         heightInputField.text = gameArea.sizeDelta.y.ToString(DECIMAL_FORMAT);
    //     }
    // }

    // private void OnHorizontalPosInputChanged(string value)
    // {
    //     if (float.TryParse(value, out float result))
    //     {
    //         result = Mathf.Clamp(result, -maxScreenWidth / 2f, maxScreenWidth / 2f);
    //         horizontalPositionSlider.value = result;
    //         UpdateGameAreaHorizontalPosition(result);
    //     }
    //     else
    //     {
    //         horizontalPositionInputField.text = gameArea.anchoredPosition.x.ToString(DECIMAL_FORMAT);
    //     }
    // }

    // private void OnVerticalPosInputChanged(string value)
    // {
    //     if (float.TryParse(value, out float result))
    //     {
    //         result = Mathf.Clamp(result, -maxScreenHeight / 2f, maxScreenHeight / 2f);
    //         verticalPositionSlider.value = result;
    //         UpdateGameAreaVerticalPosition(result);
    //     }
    //     else
    //     {
    //         verticalPositionInputField.text = gameArea.anchoredPosition.y.ToString(DECIMAL_FORMAT);
    //     }
    // }

    // private void OnMonsterScaleInputChanged(string value)
    // {
    //     if (float.TryParse(value, out float result))
    //     {
    //         monsterScaleSlider.value = result;
    //         UpdateMonsterScale(result);
    //     }
    //     else
    //     {
    //         monsterScaleInputField.text = DEFAULT_MONSTER_SCALE.ToString(SCALE_FORMAT);
    //     }
    // }

    // private void OnUIScaleInputChanged(string value)
    // {
    //     if (float.TryParse(value, out float result))
    //     {
    //         uiScaleSlider.value = result;
    //         UpdateUIScale(result);
    //     }
    //     else
    //     {
    //         uiScaleInputField.text = canvasScaler.scaleFactor.ToString(SCALE_FORMAT);
    //     }
    // }
    #endregion

    #region Callback Registration
    private void RegisterGameAreaCallbacks()
    {
        widthControl.onValueChanged += UpdateGameAreaWidth;
        horizontalPositionControl.onValueChanged += UpdateGameAreaHorizontalPosition;
        heightPositionControl.onValueChanged += UpdateGameAreaHorizontalPosition;

        // widthSlider?.onValueChanged.AddListener(UpdateGameAreaWidth);
        // heightSlider?.onValueChanged.AddListener(UpdateGameAreaHeight);
        // horizontalPositionSlider?.onValueChanged.AddListener(UpdateGameAreaHorizontalPosition);
        // verticalPositionSlider?.onValueChanged.AddListener(UpdateGameAreaVerticalPosition);
        // monsterScaleSlider?.onValueChanged.AddListener(UpdateMonsterScale);
        // uiScaleSlider?.onValueChanged.AddListener(UpdateUIScale);
    }
    private void RegisterButtonCallbacks()
    {
        gameAreaSizeIncreaseButton.onClick.AddListener(() => AdjustGameAreaSize(0.1f));
        gameAreaSizeDecreaseButton.onClick.AddListener(() => AdjustGameAreaSize(-0.1f));
        gameAreaSizeResetButton.onClick.AddListener(() => ResetGameAreaSize());
        uiSizeIncreaseButton.onClick.AddListener(() => AdjustUIScale(0.05f));
        uiSizeDecreaseButton.onClick.AddListener(() => AdjustUIScale(-0.05f));
        uiSizeResetButton.onClick.AddListener(() => ResetUIScale());
        switchScreenLeftButton.onClick.AddListener(SwitchScreenLeft);
        switchScreenRightButton.onClick.AddListener(SwitchScreenRight);
        switchScreenResetButton.onClick.AddListener(ResetScreenLayout);
    }
    // private void RegisterInputFieldCallbacks()
    // {
    //     if (widthInputField != null) widthInputField.onEndEdit.AddListener(OnWidthInputChanged);
    //     if (heightInputField != null) heightInputField.onEndEdit.AddListener(OnHeightInputChanged);
    //     if (horizontalPositionInputField != null) horizontalPositionInputField.onEndEdit.AddListener(OnHorizontalPosInputChanged);
    //     if (verticalPositionInputField != null) verticalPositionInputField.onEndEdit.AddListener(OnVerticalPosInputChanged);
    //     if (monsterScaleInputField != null) monsterScaleInputField.onEndEdit.AddListener(OnMonsterScaleInputChanged);
    //     if (uiScaleInputField != null) uiScaleInputField.onEndEdit.AddListener(OnUIScaleInputChanged);
    // }


    private void UnregisterAllCallbacks()
    {
        // Game Area
        // widthSlider?.onValueChanged.RemoveListener(UpdateGameAreaWidth);
        // heightSlider?.onValueChanged.RemoveListener(UpdateGameAreaHeight);
        // horizontalPositionSlider?.onValueChanged.RemoveListener(UpdateGameAreaHorizontalPosition);
        // verticalPositionSlider?.onValueChanged.RemoveListener(UpdateGameAreaVerticalPosition);
        // monsterScaleSlider?.onValueChanged.RemoveListener(UpdateMonsterScale);
        // uiScaleSlider?.onValueChanged.RemoveListener(UpdateUIScale);
        // // Input Fields
        // widthInputField?.onEndEdit.RemoveListener(OnWidthInputChanged);
        // heightInputField?.onEndEdit.RemoveListener(OnHeightInputChanged);
        // horizontalPositionInputField?.onEndEdit.RemoveListener(OnHorizontalPosInputChanged);
        // verticalPositionInputField?.onEndEdit.RemoveListener(OnVerticalPosInputChanged);
        // monsterScaleInputField?.onEndEdit.RemoveListener(OnMonsterScaleInputChanged);
        // uiScaleInputField?.onEndEdit.RemoveListener(OnUIScaleInputChanged);

        gameAreaSizeIncreaseButton.onClick.RemoveAllListeners();
        gameAreaSizeDecreaseButton.onClick.RemoveAllListeners();
        gameAreaSizeResetButton.onClick.RemoveAllListeners();
        uiSizeIncreaseButton.onClick.RemoveAllListeners();
        uiSizeDecreaseButton.onClick.RemoveAllListeners();
        uiSizeResetButton.onClick.RemoveAllListeners();
        switchScreenLeftButton.onClick.RemoveAllListeners();
        switchScreenRightButton.onClick.RemoveAllListeners();
        switchScreenResetButton.onClick.RemoveAllListeners();
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

        RepositionMonstersAfterScaling();
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

        // Add this line
        RepositionMonstersAfterScaling();
    }

    public void UpdateGameAreaHorizontalPosition(float value)
    {
        if (gameArea == null) return;

        Vector2 pos = gameArea.anchoredPosition;
        pos.x = value;
        gameArea.anchoredPosition = pos;

        // No need to sync UI anymore, IncrementSettingControl already does
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
        if (canvasScaler == null) return;

        canvasScaler.matchWidthOrHeight = Mathf.Clamp01(canvasScaler.matchWidthOrHeight + delta);
        OnGameAreaChanged?.Invoke();
    }
    public void AdjustUIScale(float delta)
    {
        uiScale = Mathf.Clamp(uiScale + delta, 0.5f, 2f);
        canvasScaler.scaleFactor = uiScale;
    }
    public void ResetGameAreaSize()
    {
        UpdateGameAreaWidth(DEFAULT_GAME_AREA_WIDTH);
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

    #region Utility Methods
    private void UpdateValueText(TextMeshProUGUI textComponent, float value, string format)
    {
        if (textComponent != null)
            textComponent.text = value.ToString(format);
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
}