using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using DG.Tweening;
using Coffee.UIExtensions;

[System.Serializable]
public class BiomeLayer
{
    public GameObject layerObject;
    public string layerName;
    public bool isActive = true;
}

public class BiomeManager : MonoBehaviour
{
    [Header("Biome Data")]
    // [SerializeField] private BiomeDataSO[] availableBiomes;
    [SerializeField] public BiomeDatabaseSO availableBiomes;
    [SerializeField] public BiomeDataSO currentBiome { get; private set; }
    [SerializeField] private TMP_Dropdown biomeDropdown;

    [Header("Biome Layers")]
    public BiomeLayer skyLayer;
    public BiomeLayer ambientLayer;
    public Image groundLayerFilter;

    [Header("Biome Filters")]
    public CanvasGroup darkenFilter;

    [Header("Rain System")]
    public GameObject rainSystem;

    [Header("Cloud System")]
    private CloudAmbientSystem cloudSystem;
    public RectTransform skyBG;
    public RectTransform ambientBG;

    [Header("Background Positioning")]
    private Vector2 originalSkyBGPosition;
    private Vector2 originalAmbientBGPosition;
    private const float skyBGMinY = -1000f;
    private const float ambientBGMinY = -800f;
    private SettingsManager settingsManager;

    [Header("Testing Controls")]
    public KeyCode toggleSkyKey = KeyCode.Alpha1;
    public KeyCode toggleAmbientKey = KeyCode.Alpha2;
    public KeyCode toggleCloudsKey = KeyCode.Alpha3;
    public KeyCode nextBiomeKey = KeyCode.Alpha4;
    public KeyCode prevBiomeKey = KeyCode.Alpha5;

    [Header("Events")]
    public UnityEngine.Events.UnityEvent<string, bool> OnLayerToggled;
    public UnityEngine.Events.UnityEvent<bool> OnCloudsToggled;
    public UnityEngine.Events.UnityEvent<BiomeDataSO> OnBiomeChanged;

    // Spawned objects tracking
    private List<GameObject> spawnedSkyObjects = new List<GameObject>();
    private List<GameObject> spawnedEffectObjects = new List<GameObject>();

    private int currentBiomeIndex = 0;

    private void Awake()
    {
        ServiceLocator.Register(this);
        InitializeBiome();

        // Store original positions
        if (skyBG != null) originalSkyBGPosition = skyBG.anchoredPosition;
        if (ambientBG != null) originalAmbientBGPosition = ambientBG.anchoredPosition;

        // Find cloud system
        cloudSystem = GetComponent<CloudAmbientSystem>();
        if (cloudSystem == null)
        {
            Debug.LogWarning("BiomeManager: CloudAmbientSystem not found!");
        }
    }

    private void Start()
    {
        // Get reference to SettingsManager
        settingsManager = ServiceLocator.Get<SettingsManager>();
        if (settingsManager != null)
        {
            // Subscribe to game area resize events
            settingsManager.OnGameAreaChanged.AddListener(UpdateBackgroundPositions);

            // Initialize positions with current game area size
            UpdateBackgroundPositions();
        }

        // Initialize dropdown if available
        if (biomeDropdown != null)
        {
            InitializeDropdown();
            biomeDropdown.onValueChanged.AddListener(OnDropdownValueChanged);
        }

        string savedBiomeID = SaveSystem.GetActiveBiome();
        if (!string.IsNullOrEmpty(savedBiomeID))
        {
            ChangeBiomeByID(savedBiomeID);
        }
        else
        {
            DeactiveBiome();
        }

        SetSkyLayerActive(SaveSystem.IsSkyEnabled());
        SetAmbientLayerActive(SaveSystem.IsAmbientEnabled());
        ToggleClouds(SaveSystem.IsCloudEnabled());
    }

    private void Update()
    {
        HandleTestingInput();
    }

    private void InitializeBiome()
    {
        // Set initial states
        SetLayerActive(skyLayer, skyLayer.isActive);
        SetLayerActive(ambientLayer, ambientLayer.isActive);

        // Set current biome index
        if (availableBiomes != null && availableBiomes.allBiomes.Count > 0 && currentBiome != null)
        {
            for (int i = 0; i < availableBiomes.allBiomes.Count; i++)
            {
                if (availableBiomes.allBiomes[i] == currentBiome)
                {
                    currentBiomeIndex = i;
                    break;
                }
            }
        }
    }

    private void InitializeDropdown()
    {
        biomeDropdown.ClearOptions();

        List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();
        foreach (BiomeDataSO biome in availableBiomes.allBiomes)
        {
            options.Add(new TMP_Dropdown.OptionData(biome.biomeName));
        }

        biomeDropdown.AddOptions(options);

        // Set initial selection to match current biome
        if (currentBiome != null)
        {
            for (int i = 0; i < availableBiomes.allBiomes.Count; i++)
            {
                if (availableBiomes.allBiomes[i] == currentBiome)
                {
                    biomeDropdown.value = i;
                    break;
                }
            }
        }
    }

    public void ToggleFilters(string filterName, bool active)
    {
        if (filterName == "Darken")
        {
            if (darkenFilter != null)
            {
                darkenFilter.DOFade(active ? 1f : 0f, 0.5f).SetEase(Ease.InOutQuad);
            }
        }
        else
        {
            Debug.LogWarning($"BiomeManager: Unknown filter '{filterName}'");
        }
    }

    //for testing purposes
    private void HandleTestingInput()
    {
        if (Input.GetKeyDown(toggleSkyKey))
        {
            bool newSkyState = !skyLayer.isActive;
            ToggleLayer(ref skyLayer, newSkyState);
        }

        if (Input.GetKeyDown(toggleAmbientKey))
        {
            bool newAmbientState = !ambientLayer.isActive;
            ToggleLayer(ref ambientLayer, newAmbientState);
        }

        if (Input.GetKeyDown(toggleCloudsKey))
        {
            bool currentCloudState = skyBG != null && skyBG.gameObject.activeSelf;
            ToggleClouds(!currentCloudState);
        }

        // Biome cycling
        if (Input.GetKeyDown(nextBiomeKey)) ChangeBiome(1);
        if (Input.GetKeyDown(prevBiomeKey)) ChangeBiome(-1);
    }

    private void UpdateBackgroundPositions()
    {
        if (settingsManager == null) return;

        // Get current game area height
        float currentHeight = settingsManager.gameArea.sizeDelta.y;

        // Get min and max height values directly from SettingsManager constants
        float minHeight = settingsManager.GetMinGameAreaHeight();
        float maxHeight = settingsManager.GetMaxGameAreaHeight();

        // Calculate height percentage (0 = min height, 1 = max height)
        float heightPercentage = Mathf.InverseLerp(minHeight, maxHeight, currentHeight);

        // Update skyBG position
        if (skyBG != null)
        {
            Vector2 newPos = skyBG.anchoredPosition;
            newPos.y = Mathf.Lerp(skyBGMinY, originalSkyBGPosition.y, heightPercentage);
            skyBG.anchoredPosition = newPos;
        }

        // Update ambientBG position
        if (ambientBG != null)
        {
            Vector2 newPos = ambientBG.anchoredPosition;
            newPos.y = Mathf.Lerp(ambientBGMinY, originalAmbientBGPosition.y, heightPercentage);
            ambientBG.anchoredPosition = newPos;
        }
    }

    public void ToggleClouds(bool active)
    {
        // Save setting
        SaveSystem.SetCloudEnabled(active);

        // Invoke event
        OnCloudsToggled?.Invoke(active);

        // Control cloud ambient system
        if (cloudSystem != null)
        {
            cloudSystem.ToggleCloud(active);
        }

        // Optional UI feedback
        var uiManager = ServiceLocator.Get<UIManager>();
        if (uiManager != null)
        {
            uiManager.ShowMessage($"Clouds: {(active ? "ON" : "OFF")}", 1f);
        }
    }



    #region Layer Management
    public void ToggleLayer(ref BiomeLayer layer, bool active)
    {
        layer.isActive = active;
        SetLayerActive(layer, active);
        OnLayerToggled?.Invoke(layer.layerName, active);

        if (layer.layerName == "Sky")
            SaveSystem.SetSkyEnabled(active);
        else if (layer.layerName == "Ambient")
            SaveSystem.SetAmbientEnabled(active);

        var uiManager = ServiceLocator.Get<UIManager>();
        uiManager?.ShowMessage($"{layer.layerName}: {(active ? "ON" : "OFF")}", 1f);
    }
    public void DeactiveBiome()
    {
        SetSkyLayerActive(false);
        SetAmbientLayerActive(false);
    }


    private void SetLayerActive(BiomeLayer layer, bool active)
    {
        if (layer.layerObject != null)
        {
            layer.layerObject.GetComponent<Image>().enabled = active;
        }
    }

    public void SetSkyLayerActive(bool active)
    {
        skyLayer.isActive = active;
        SetLayerActive(skyLayer, active);
    }

    public void SetAmbientLayerActive(bool active)
    {
        ambientLayer.isActive = active;
        SetLayerActive(ambientLayer, active);
    }
    #endregion

    #region Biome Management

    private void OnDropdownValueChanged(int index)
    {
        if (index >= 0 && index < availableBiomes.allBiomes.Count)
        {
            ChangeBiomeByIndex(index);
        }
    }

    /// <summary>
    /// Change biome by relative offset (1 for next, -1 for previous)
    /// </summary>
    public void ChangeBiome(int offset)
    {
        if (availableBiomes == null || availableBiomes.allBiomes.Count == 0) return;

        // Calculate new index with wrap-around
        int newIndex = (currentBiomeIndex + offset) % availableBiomes.allBiomes.Count;
        if (newIndex < 0) newIndex = availableBiomes.allBiomes.Count - 1;

        ChangeBiomeByIndex(newIndex);
    }

    /// <summary>
    /// Change biome by index in the available biomes array
    /// </summary>
    public void ChangeBiomeByIndex(int index)
    {
        if (availableBiomes == null || index < 0 || index >= availableBiomes.allBiomes.Count) return;

        BiomeDataSO newBiome = availableBiomes.allBiomes[index];
        ApplyBiomeData(newBiome);
        currentBiomeIndex = index;

        // Update dropdown if available
        if (biomeDropdown != null && biomeDropdown.value != index)
        {
            biomeDropdown.SetValueWithoutNotify(index);
        }
    }
    /// <summary>
    /// Change biome by its biomeID (string)
    /// </summary>
    public void ChangeBiomeByID(string biomeID)
    {
        if (string.IsNullOrEmpty(biomeID))
        {
            // No biome set, deactivate everything
            DeactiveBiome();
            currentBiome = null;
            return;
        }

        if (availableBiomes == null) return;

        for (int i = 0; i < availableBiomes.allBiomes.Count; i++)
        {
            if (availableBiomes.allBiomes[i].biomeID == biomeID)
            {
                ChangeBiomeByIndex(i);
                return;
            }
        }

        Debug.LogWarning($"BiomeManager: Biome with ID '{biomeID}' not found!");
    }

    /// <summary>
    /// Change biome by name
    /// </summary>
    public void ChangeBiomeByName(string biomeName)
    {
        if (availableBiomes == null) return;

        for (int i = 0; i < availableBiomes.allBiomes.Count; i++)
        {
            if (availableBiomes.allBiomes[i].biomeName == biomeName)
            {
                ChangeBiomeByIndex(i);
                return;
            }
        }

        Debug.LogWarning($"BiomeManager: Biome with name '{biomeName}' not found!");
    }

    /// <summary>
    /// Apply the selected biome data
    /// </summary>
    private void ApplyBiomeData(BiomeDataSO biome)
    {
        if (biome == null) return;

        // Update current biome
        currentBiome = biome;

        // Update sky background
        if (skyBG != null && biome.skyBackground != null)
        {
            Image skyImage = skyBG.GetComponent<Image>();
            if (skyImage != null)
            {
                skyImage.sprite = biome.skyBackground;
            }
        }

        // Update ambient background
        if (ambientBG != null && biome.ambientBackground != null)
        {
            Image ambientImage = ambientBG.GetComponent<Image>();
            if (ambientImage != null)
            {
                ambientImage.sprite = biome.ambientBackground;
            }
        }

        // Set ground layer filter color and alpha
        if (groundLayerFilter != null)
        {
            Image groundImage = groundLayerFilter.GetComponent<Image>();
            CanvasGroup groundFilterCg = groundLayerFilter.GetComponent<CanvasGroup>();

            if (biome.groundBackground != null)
            {
                if (groundImage != null)
                {
                    groundImage.sprite = biome.groundBackground;
                }
            }

            if (groundFilterCg != null)
            {
                groundLayerFilter.color = Color.clear; // Reset to clear before applying new color
                groundFilterCg.alpha = 0f; // Reset alpha before applying new value

                groundLayerFilter.color = biome.groundFilterColor;
                groundFilterCg.DOFade(biome.groundFilterAlpha, 0.5f).SetEase(Ease.InOutQuad);
            }
        }

        // Update cloud system
        if (cloudSystem != null)
        {
            cloudSystem.UpdateBiomeData(biome);
        }

        // Clear existing sky objects
        ClearSpawnedObjects(spawnedSkyObjects);

        // Spawn new sky objects
        if (biome.skyObjects != null && biome.skyObjects.Length > 0 && skyBG != null)
        {
            foreach (GameObject prefab in biome.skyObjects)
            {
                if (prefab != null)
                {
                    GameObject obj = Instantiate(prefab, skyBG);
                    spawnedSkyObjects.Add(obj);
                }
            }
        }

        // Clear existing effect objects
        ClearSpawnedObjects(spawnedEffectObjects);

        // Spawn new effect objects
        if (biome.effectPrefabs != null && biome.effectPrefabs.Length > 0 && skyBG != null)
        {
            foreach (GameObject prefab in biome.effectPrefabs)
            {
                if (prefab != null)
                {
                    GameObject obj = Instantiate(prefab, skyBG);
                    spawnedEffectObjects.Add(obj);
                }
            }
        }

        // Invoke the biome changed event
        OnBiomeChanged?.Invoke(biome);

        // Show message if UIManager is available
        var uiManager = ServiceLocator.Get<UIManager>();
        if (uiManager != null)
        {
            uiManager.ShowMessage($"Biome: {biome.biomeName}", 1.5f);
        }
    }

    private void ClearSpawnedObjects(List<GameObject> objects)
    {
        foreach (GameObject obj in objects)
        {
            if (obj != null)
            {
                Destroy(obj);
            }
        }

        objects.Clear();
    }
    #endregion

    public void ToggleRainSystem()
    {
        if (rainSystem != null)
        {
            rainSystem.SetActive(!rainSystem.activeSelf);
            SaveSystem.GetPlayerConfig().isRainEnabled = rainSystem.activeSelf;

            // Optional: Show message if UIManager is available
            var uiManager = ServiceLocator.Get<UIManager>();
            if (uiManager != null)
            {
                uiManager.ShowMessage($"Rain System: {(!rainSystem.activeSelf ? "Disabled" : "Enabled")}", 1f);
            }
        }
        else
        {
            Debug.LogWarning("BiomeManager: Rain system not assigned!");
        }
    }

    private void OnDestroy()
    {
        var settingsManager = ServiceLocator.Get<SettingsManager>();
        if (settingsManager != null)
        {
            settingsManager.OnGameAreaChanged.RemoveListener(UpdateBackgroundPositions);
        }

        if (biomeDropdown != null)
        {
            biomeDropdown.onValueChanged.RemoveListener(OnDropdownValueChanged);
        }

        ClearSpawnedObjects(spawnedSkyObjects);
        ClearSpawnedObjects(spawnedEffectObjects);

        ServiceLocator.Unregister<BiomeManager>();
    }
}
