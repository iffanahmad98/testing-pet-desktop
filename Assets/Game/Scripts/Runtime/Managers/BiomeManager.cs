using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using DG.Tweening;
using System.Collections;

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
    [SerializeField] public BiomeDatabaseSO availableBiomes;
    [SerializeField] public BiomeDataSO currentBiome { get; private set; }

    [Header("Biome Layers")]
    public BiomeLayer skyLayer;
    public BiomeLayer ambientLayer;
    public Image groundLayerFilter;
    public Image groundLayerOverlay;
    public CanvasGroup groundLayerCanvasGroup;
    public Image baseGround;

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
    private const float skyBGMinY = -1200f;
    private const float ambientBGMinY = -900f;
    private SettingsManager settingsManager;

    [Header("Events")]
    public UnityEngine.Events.UnityEvent<bool> OnCloudsToggled;

    // Spawned objects tracking
    private List<GameObject> spawnedSkyObjects = new List<GameObject>();
    private List<GameObject> spawnedEffectObjects = new List<GameObject>();

    private void Awake()
    {
        ServiceLocator.Register(this);

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
        StartCoroutine(InitializeBiome());
    }

    private IEnumerator InitializeBiome()
    {
        yield return new WaitForSeconds(0.1f); // Ensure all systems are ready
        // Get reference to SettingsManager
        settingsManager = ServiceLocator.Get<SettingsManager>();
        if (settingsManager != null)
        {
            // Subscribe to game area resize events
            settingsManager.OnGameAreaChanged.AddListener(UpdateBackgroundPositions);
            // Initialize positions with current game area size
            UpdateBackgroundPositions();
        }

        yield return new WaitForSeconds(0.1f); // Allow time for settings to initialize

        string savedBiomeID = SaveSystem.GetActiveBiome();
        if (!string.IsNullOrEmpty(savedBiomeID))
        {
            ChangeBiomeByID(savedBiomeID);
            SetSkyLayerActive(SaveSystem.IsSkyEnabled());
            SetAmbientLayerActive(SaveSystem.IsAmbientEnabled());
            ToggleClouds(SaveSystem.IsCloudEnabled());
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
    }



    #region Layer Management
    public void ToggleLayer(ref BiomeLayer layer, bool active)
    {
        layer.isActive = active;
        SetLayerActive(layer, active);

        if (layer.layerName == "Sky")
            SaveSystem.SetSkyEnabled(active);
        else if (layer.layerName == "Ambient")
            SaveSystem.SetAmbientEnabled(active);
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

    /// <summary>
    /// Change biome by its biomeID (string)
    /// </summary>
    public void ChangeBiomeByID(string biomeID)
    {
        if (string.IsNullOrEmpty(biomeID)) return;

        if (availableBiomes == null) return;

        for (int i = 0; i < availableBiomes.allBiomes.Count; i++)
        {
            if (availableBiomes.allBiomes[i].biomeID == biomeID)
            {
                BiomeDataSO newBiome = availableBiomes.allBiomes[i];
                ApplyBiomeData(newBiome);
                return;
            }
        }
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
                BiomeDataSO newBiome = availableBiomes.allBiomes[i];
                ApplyBiomeData(newBiome);
                return;
            }
        }
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
        
        SetSkyLayerActive(true);
        SetAmbientLayerActive(true);
        ToggleClouds(true);

        // Set ground layer filter color and alpha
        if (groundLayerFilter != null)
        {
            Image groundImage = groundLayerFilter.GetComponent<Image>();
            CanvasGroup groundFilterCg = groundLayerFilter.GetComponent<CanvasGroup>();

            if (biome.groundBackground != null)
            {
                if (groundImage != null)
                {
                    groundLayerOverlay.sprite = biome.groundBackground;
                    groundLayerCanvasGroup.DOFade(1f, 0.5f).SetEase(Ease.InOutQuad).OnPlay(() =>
                    {
                        baseGround.enabled = false;
                    });
                }
            }

            if (groundFilterCg != null)
            {
                groundLayerFilter.color = Color.clear; 
                groundFilterCg.alpha = 0f; 
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

        SaveSystem.AddOwnedBiome(biome.biomeID);
        SaveSystem.SetActiveBiome(biome.biomeID);
        SaveSystem.SetSkyEnabled(true);
        SaveSystem.SetAmbientEnabled(true);
        SaveSystem.SetCloudEnabled(true);
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

        ClearSpawnedObjects(spawnedSkyObjects);
        ClearSpawnedObjects(spawnedEffectObjects);

        ServiceLocator.Unregister<BiomeManager>();
    }
}
