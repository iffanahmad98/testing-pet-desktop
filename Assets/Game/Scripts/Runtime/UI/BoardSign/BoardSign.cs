using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Board Sign controller with buttons to adjust game area height and navigate between scenes
/// </summary>
public class BoardSign : MonoBehaviour
{
    [Header("Game Area Height Buttons")]
    [SerializeField] private Button setMaxHeightButton;
    [SerializeField] private Button setMinHeightButton;

    [Header("Scene Navigation Buttons")]
    [SerializeField] private Button goToFarmButton;
    [SerializeField] private Button goToHotelButton;

    [Header("Scene Names")]
    [SerializeField] private string farmGameSceneName = "FarmGame"; // Scene yang memiliki kedua map (Farm & Hotel)

    private MonsterManager monsterManager;

    private void Start()
    {
        // Get references to managers
        monsterManager = ServiceLocator.Get<MonsterManager>();

        if (monsterManager == null)
        {
            Debug.LogError("MonsterManager not found! BoardSign requires MonsterManager for depth sorting.");
        }
        else
        {
            // Register this BoardSign to the pumpkinObjects list for depth sorting (same as pumpkin flow)
            if (!monsterManager.pumpkinObjects.Contains(transform))
            {
                monsterManager.pumpkinObjects.Add(transform);
                Debug.Log("BoardSign added to depth sorting list (pumpkin flow).");
            }
        }

        // Register button callbacks
        RegisterButtonCallbacks();
    }

    private void OnDestroy()
    {
        // Unregister button callbacks to prevent memory leaks
        UnregisterButtonCallbacks();

        // Remove from depth sorting list (same as pumpkin flow)
        if (monsterManager != null && monsterManager.pumpkinObjects.Contains(transform))
        {
            monsterManager.pumpkinObjects.Remove(transform);
        }
    }

    private void RegisterButtonCallbacks()
    {
        if (setMaxHeightButton != null)
            setMaxHeightButton.onClick.AddListener(OnSetMaxHeight);
        else
            Debug.LogWarning("Set Max Height Button is not assigned in BoardSign!");

        if (setMinHeightButton != null)
            setMinHeightButton.onClick.AddListener(OnSetMinHeight);
        else
            Debug.LogWarning("Set Min Height Button is not assigned in BoardSign!");

        if (goToFarmButton != null)
            goToFarmButton.onClick.AddListener(OnGoToFarm);
        else
            Debug.LogWarning("Go To Farm Button is not assigned in BoardSign!");

        if (goToHotelButton != null)
            goToHotelButton.onClick.AddListener(OnGoToHotel);
        else
            Debug.LogWarning("Go To Hotel Button is not assigned in BoardSign!");
    }

    private void UnregisterButtonCallbacks()
    {
        if (setMaxHeightButton != null)
            setMaxHeightButton.onClick.RemoveListener(OnSetMaxHeight);

        if (setMinHeightButton != null)
            setMinHeightButton.onClick.RemoveListener(OnSetMinHeight);

        if (goToFarmButton != null)
            goToFarmButton.onClick.RemoveListener(OnGoToFarm);

        if (goToHotelButton != null)
            goToHotelButton.onClick.RemoveListener(OnGoToHotel);
    }

    /// <summary>
    /// Sets the game area height to maximum value
    /// </summary>
    private void OnSetMaxHeight()
    {
        var settingsManager = ServiceLocator.Get<SettingsManager>();
        if (settingsManager == null)
        {
            Debug.LogError("Cannot set max height: SettingsManager is null!");
            return;
        }

        float maxHeight = settingsManager.GetMaxGameAreaHeight();
        settingsManager.UpdateGameAreaHeight(maxHeight);

        Debug.Log($"Game area height set to maximum: {maxHeight}");
    }

    /// <summary>
    /// Sets the game area height to minimum value
    /// </summary>
    private void OnSetMinHeight()
    {
        var settingsManager = ServiceLocator.Get<SettingsManager>();
        if (settingsManager == null)
        {
            Debug.LogError("Cannot set min height: SettingsManager is null!");
            return;
        }

        float minHeight = settingsManager.GetMinGameAreaHeight();
        settingsManager.UpdateGameAreaHeight(minHeight);

        Debug.Log($"Game area height set to minimum: {minHeight}");
    }

    /// <summary>
    /// Switch to FarmGame scene and set focus to Farm area
    /// </summary>
    private void OnGoToFarm()
    {
        var sceneLoader = AdditiveSceneLoader.Instance;
        if (sceneLoader == null)
        {
            Debug.LogError("[BoardSign] AdditiveSceneLoader instance not found!");
            return;
        }

        // Set focus target to Farm before switching scene
        SceneFocusManager.SetFocusTarget(SceneFocusManager.FocusTarget.Farm);

        Debug.Log($"[BoardSign] Switching to FarmGame scene with Farm focus target");
        sceneLoader.SwitchToFarmScene();
    }

    /// <summary>
    /// Switch to FarmGame scene and set focus to Hotel area
    /// </summary>
    private void OnGoToHotel()
    {
        var sceneLoader = AdditiveSceneLoader.Instance;
        if (sceneLoader == null)
        {
            Debug.LogError("[BoardSign] AdditiveSceneLoader instance not found!");
            return;
        }

        // Set focus target to Hotel before switching scene
        SceneFocusManager.SetFocusTarget(SceneFocusManager.FocusTarget.Hotel);

        Debug.Log($"[BoardSign] Switching to FarmGame scene with Hotel focus target");
        sceneLoader.SwitchToFarmScene();
    }

    /// <summary>
    /// Public method to manually set FarmGame scene name
    /// </summary>
    public void SetFarmGameSceneName(string sceneName)
    {
        farmGameSceneName = sceneName;
    }
}
