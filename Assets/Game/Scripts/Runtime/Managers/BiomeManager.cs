using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

[System.Serializable]
public class BiomeLayer
{
    public GameObject layerObject;
    public string layerName;
    public bool isActive = true;
    public float parallaxSpeed = 0f; // 0 = no parallax, 1 = full speed
}

public class BiomeManager : MonoBehaviour
{
    #region Constants
    private const float PARALLAX_TIME_MULTIPLIER = 0.1f;
    private const float PARALLAX_AMPLITUDE = 10f;
    private const float SKY_USAGE_PERCENTAGE = 0.8f;
    private const float CLOUD_START_X_OFFSET = 0.6f;
    private const float CLOUD_END_X_OFFSET = 0.6f;
    private const float LANE_HEIGHT_PERCENTAGE = 0.8f;
    #endregion

    [Header("Biome Layers")]
    public BiomeLayer skyLayer;
    public BiomeLayer ambientLayer;
    public BiomeLayer groundLayer;

    [Header("Cloud System")]
    public RectTransform skyBG;
    public GameObject cloudPrefab;
    public Sprite[] cloudSprites;
    public int maxClouds = 3;
    public float cloudSpawnInterval = 5f;
    public float baseCloudSpeed = 50f;
    public Vector2 speedRange = new Vector2(1.0f, 2.0f);
    public Vector2 scaleRange = new Vector2(0.2f, 0.5f); // Smaller clouds to reduce overlap
    public Vector2 opacityRange = new Vector2(0.4f, 0.8f);
    public int cloudLanes = 2; // Increased from 3 to 4 lanes
    public float minCloudSpacing = 100f; // Increased from 50f
    public float baseWaitTime = 5f; // Reduced since we have better spacing
    private Vector2 lastSkyPosition;
    private Vector2 lastSkySize;
    private bool skyAreaChanged = false;

    [Header("Testing Controls")]
    public KeyCode toggleSkyKey = KeyCode.Alpha1;
    public KeyCode toggleAmbientKey = KeyCode.Alpha2;
    public KeyCode toggleCloudsKey = KeyCode.Alpha3;

    [Header("Events")]
    public UnityEngine.Events.UnityEvent<string, bool> OnLayerToggled;
    public UnityEngine.Events.UnityEvent<bool> OnCloudsToggled;

    [Header("Object Pooling")]
    public bool useCloudPooling = true;
    private Queue<GameObject> cloudPool = new Queue<GameObject>();

    private List<GameObject> activeClouds = new List<GameObject>();
    private Coroutine cloudSpawner;
    private bool cloudsEnabled = true;
    [SerializeField] private RectTransform gameAreaRect;

    private Dictionary<int, float> lastCloudSpawnTime = new Dictionary<int, float>(); // Track last spawn time per lane
    private Dictionary<int, float> lastCloudSpeed = new Dictionary<int, float>(); // Track last cloud speed per lane

    private float initialAreaHeight;
    private float skyStartPos;
    private float ambientStartPos;

    // ───────── tuning knobs ─────────
    [SerializeField] float ambientRatio = 0.50f;   // 0 = horizon locked, 1 = same speed as sky
    [SerializeField] float minSkyPixels = 120f;    // keep at least this many sky pixels
    
    private RectTransform skyRectTransform;
    private RectTransform ambientRectTransform;
    private RectTransform groundRectTransform;

    private void Awake()
    {
        ServiceLocator.Register(this);
        InitializeBiome();

        SettingsManager settings = ServiceLocator.Get<SettingsManager>();
        if (settings != null)
        {
            settings.OnGameAreaChanged.AddListener(OnGameAreaResized);
        }
    }

    public void OnGameAreaResized()
    {
        // Clear current clouds and restart system
        ClearAllClouds();
        InitializeLanes();
        
        // Update cached values
        if (skyBG != null)
        {
            lastSkyPosition = skyBG.anchoredPosition;
            lastSkySize = skyBG.sizeDelta;
        }
    }

    private void Start()
    {
        if (gameAreaRect == null)
        {
            gameAreaRect = transform.parent.GetComponent<RectTransform>();
        }

        // Initialize lane tracking
        InitializeLanes();
        StartCloudSystem();

        // NEW – record baseline numbers
        initialAreaHeight = gameAreaRect.sizeDelta.y;

        if (skyLayer.layerObject != null)
            skyStartPos = skyLayer.layerObject.GetComponent<RectTransform>().anchoredPosition.y;

        if (ambientLayer.layerObject != null)
            ambientStartPos = ambientLayer.layerObject.GetComponent<RectTransform>().anchoredPosition.y;

    }

    private void Update()
    {
        HandleTestingInput();
    }

    private void LateUpdate() {
        UpdateParallax();
        UpdateVerticalCompress();
    }

    private void InitializeBiome()
    {
        // Initialize layers if not set in inspector
        if (skyLayer.layerObject == null)
            Debug.LogWarning("Sky layer object is not set! Please assign it in the inspector.");
        if (ambientLayer.layerObject == null)
            Debug.LogWarning("Ambient layer object is not set! Please assign it in the inspector.");
        if (groundLayer.layerObject == null)
            Debug.LogWarning("Ground layer object is not set! Please assign it in the inspector.");

        // Set initial states
        SetLayerActive(skyLayer, skyLayer.isActive);
        SetLayerActive(ambientLayer, ambientLayer.isActive);
        SetLayerActive(groundLayer, groundLayer.isActive);

        // Cache RectTransform components
        if (skyLayer.layerObject != null)
            skyRectTransform = skyLayer.layerObject.GetComponent<RectTransform>();
        if (ambientLayer.layerObject != null)
            ambientRectTransform = ambientLayer.layerObject.GetComponent<RectTransform>();
        if (groundLayer.layerObject != null)
            groundRectTransform = groundLayer.layerObject.GetComponent<RectTransform>();
    }

    private void HandleTestingInput()
    {
        if (Input.GetKeyDown(toggleSkyKey))
        {
            ToggleLayer(ref skyLayer);
        }
        
        if (Input.GetKeyDown(toggleAmbientKey))
        {
            ToggleLayer(ref ambientLayer);
        }
        
        if (Input.GetKeyDown(toggleCloudsKey))
        {
            ToggleClouds();
        }
    }

    private void UpdateParallax()
    {
        float timeOffset = Time.time * PARALLAX_TIME_MULTIPLIER;
        
        if (skyLayer.layerObject != null && skyLayer.parallaxSpeed > 0 && skyRectTransform != null)
        {
            Vector2 pos = skyRectTransform.anchoredPosition;
            pos.x = Mathf.Sin(timeOffset * skyLayer.parallaxSpeed) * PARALLAX_AMPLITUDE;
            skyRectTransform.anchoredPosition = pos;
        }
    }
    
    private void UpdateVerticalCompress()
    {
        if (gameAreaRect == null) return;

        float lost = initialAreaHeight - gameAreaRect.rect.height;
        if (lost < 0f) lost = 0f;

        // Add null checks before accessing components
        if (skyLayer.layerObject != null)
        {
            var skyRect = skyRectTransform ?? skyLayer.layerObject.GetComponent<RectTransform>();
            float skyCrop = Mathf.Min(lost, skyRect.rect.height - minSkyPixels);
            SlideBandY(skyLayer.layerObject, skyStartPos, skyCrop);
        }

        if (ambientLayer.layerObject != null)
        {
            float ambientCrop = lost * ambientRatio;
            SlideBandY(ambientLayer.layerObject, ambientStartPos, ambientCrop);
        }
    }

    private void SlideBandY(GameObject go, float startY, float downBy)
    {
        if (go == null) return;

        RectTransform rt = null;
        if (go == skyLayer.layerObject) rt = skyRectTransform;
        else if (go == ambientLayer.layerObject) rt = ambientRectTransform;
        else if (go == groundLayer.layerObject) rt = groundRectTransform;
        else rt = go.GetComponent<RectTransform>(); // Fallback

        if (rt == null) return;

        Vector2 p = rt.anchoredPosition;
        p.y = startY - downBy;
        rt.anchoredPosition = p;
    }

    private void InitializeLanes()
    {
        lastCloudSpawnTime.Clear();
        lastCloudSpeed.Clear();

        for (int i = 0; i < cloudLanes; i++)
        {
            lastCloudSpawnTime[i] = -999f; // Long ago
            lastCloudSpeed[i] = 0f;
        }
    }

    #region Layer Management
    public void ToggleLayer(ref BiomeLayer layer)
    {
        layer.isActive = !layer.isActive;
        SetLayerActive(layer, layer.isActive);
        
        OnLayerToggled?.Invoke(layer.layerName, layer.isActive);
        
        // Show message if UIManager is available
        var uiManager = ServiceLocator.Get<UIManager>();
        if (uiManager != null)
        {
            uiManager.ShowMessage($"{layer.layerName}: {(layer.isActive ? "ON" : "OFF")}", 1f);
        }
    }

    private void SetLayerActive(BiomeLayer layer, bool active)
    {
        if (layer.layerObject != null)
        {
            layer.layerObject.SetActive(active);
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

    public void SetLayerActive(string layerName, bool active)
    {
        var layer = GetLayerByName(layerName);
        if (layer != null)
        {
            layer.isActive = active;
            SetLayerActive(layer, active);
        }
    }

    private BiomeLayer GetLayerByName(string name)
    {
        return name.ToLower() switch
        {
            "sky" => skyLayer,
            "ambient" => ambientLayer,
            "ground" => groundLayer,
            _ => null
        };
    }
    #endregion

    #region Cloud System
    private void StartCloudSystem()
    {
        if (cloudSpawner != null)
            StopCoroutine(cloudSpawner);
            
        cloudSpawner = StartCoroutine(CloudSpawnerCoroutine());
    }

    private void StopCloudSystem()
    {
        if (cloudSpawner != null)
        {
            StopCoroutine(cloudSpawner);
            cloudSpawner = null;
        }
        
        ClearAllClouds();
    }

    private IEnumerator CloudSpawnerCoroutine()
    {
        while (cloudsEnabled)
        {
            if (activeClouds.Count < maxClouds && skyLayer.isActive)
            {
                SpawnCloud();
            }
            
            yield return new WaitForSeconds(cloudSpawnInterval);
        }
    }

    private void SpawnCloud()
    {
        if (!ValidateCloudSpawning()) return;

        int availableLane = GetAvailableLane();
        if (availableLane == -1) // No available lanes
            return;

        GameObject cloud = GetPooledCloud();
        var cloudRect = cloud.GetComponent<RectTransform>();
        var cloudImage = cloud.GetComponent<Image>();
        
        if (cloudRect == null || cloudImage == null)
        {
            Destroy(cloud);
            return;
        }

        // Random sprite selection
        cloudImage.sprite = cloudSprites[Random.Range(0, cloudSprites.Length)];
        
        // Random properties
        Color cloudColor = cloudImage.color;
        cloudColor.a = Random.Range(opacityRange.x, opacityRange.y);
        cloudImage.color = cloudColor;
        
        float scale = Random.Range(scaleRange.x, scaleRange.y);
        cloud.transform.localScale = Vector3.one * scale;
        
        // Random speed for this cloud
        float speedMultiplier = Random.Range(speedRange.x, speedRange.y);
        
        // Calculate lane position
        Rect skyRect = skyBG.rect;
        float startX = -skyRect.width * 0.6f;
        float laneY = CalculateLaneY(availableLane);
        
        cloudRect.anchoredPosition = new Vector2(startX, laneY);
        
        // Update lane tracking
        lastCloudSpawnTime[availableLane] = Time.time;
        lastCloudSpeed[availableLane] = baseCloudSpeed * speedMultiplier;
        
        // Start movement
        StartCoroutine(MoveCloud(cloud, speedMultiplier));
        
        activeClouds.Add(cloud);
    }

    private bool ValidateCloudSpawning()
    {
        if (cloudSprites == null || cloudSprites.Length == 0)
        {
            Debug.LogWarning("BiomeManager: No cloud sprites assigned!");
            return false;
        }
        
        if (cloudPrefab == null)
        {
            Debug.LogWarning("BiomeManager: No cloud prefab assigned!");
            return false;
        }
        
        if (skyBG == null)
        {
            Debug.LogWarning("BiomeManager: No sky background assigned!");
            return false;
        }
        
        return true;
    }

    private int GetAvailableLane()
    {
        List<int> availableLanes = new List<int>();
        
        for (int i = 0; i < cloudLanes; i++)
        {
            if (IsLaneAvailable(i))
            {
                availableLanes.Add(i);
            }
        }
        
        if (availableLanes.Count == 0)
            return -1;
        
        // Return random available lane
        return availableLanes[Random.Range(0, availableLanes.Count)];
    }

    private bool IsLaneAvailable(int laneIndex)
    {
        // Check if this lane has ever been used
        if (!lastCloudSpawnTime.ContainsKey(laneIndex))
            return true;
        
        float timeSinceLastSpawn = Time.time - lastCloudSpawnTime[laneIndex];
        
        // Base wait time (5 seconds) + spacing-based wait time
        float baseWait = baseWaitTime;
        
        // Additional wait based on spacing and speed
        float spacingWait = 0f;
        if (lastCloudSpeed.ContainsKey(laneIndex) && lastCloudSpeed[laneIndex] > 0)
        {
            spacingWait = minCloudSpacing / lastCloudSpeed[laneIndex];
        }
        
        float totalWaitTime = baseWait + spacingWait;
        
        // Optional: Add some debug logging
        #if UNITY_EDITOR && DEBUG_CLOUDS
        if (timeSinceLastSpawn < totalWaitTime)
        {
            Debug.Log($"Lane {laneIndex}: Waiting {totalWaitTime - timeSinceLastSpawn:F1}s more (Base: {baseWait}s + Spacing: {spacingWait:F1}s)");
        }
        #endif
        
        return timeSinceLastSpawn >= totalWaitTime;
    }

    private float CalculateLaneY(int laneIndex)
    {
        if (skyBG == null) return 0f;
        
        Rect skyRect = skyBG.rect;
        
        // Work in local coordinates where (0,0) is center of skyBG
        // Sky top = +height/2, Sky bottom = -height/2
        
        float usableHeight = skyRect.height * 0.6f; // Use 60% of sky height
        float laneSpacing = usableHeight / cloudLanes;
        
        // Start from a reasonable position within sky bounds
        float startFromTop = skyRect.height * 0.2f; // 20% down from top
        float actualStartY = (skyRect.height * 0.5f) - startFromTop; // Top edge minus offset
        
        // Calculate lane position
        float laneY = actualStartY - (laneIndex * laneSpacing);
        
        // Ensure we're within sky bounds
        float minY = -skyRect.height * 0.4f; // Don't go too low
        float maxY = skyRect.height * 0.4f;  // Don't go too high
        
        laneY = Mathf.Clamp(laneY, minY, maxY);
        
        Debug.Log($"Lane {laneIndex}: Y={laneY:F1} (Sky bounds: {-skyRect.height*0.5f:F1} to {skyRect.height*0.5f:F1})");
        
        return laneY;
    }

    private IEnumerator MoveCloud(GameObject cloud, float speedMultiplier)
    {
        var cloudRect = cloud.GetComponent<RectTransform>();
        if (cloudRect == null) yield break;
        
        float speed = baseCloudSpeed * speedMultiplier;
        Rect skyRect = skyBG.rect;
        float endX = skyRect.width * 0.6f;
        
        while (cloud != null && cloudRect.anchoredPosition.x < endX)
        {
            Vector2 pos = cloudRect.anchoredPosition;
            pos.x += speed * Time.deltaTime;
            cloudRect.anchoredPosition = pos;
            
            yield return null;
        }
        
        if (cloud != null)
        {
            activeClouds.Remove(cloud);
            ReturnCloudToPool(cloud);
        }
    }

    private GameObject GetPooledCloud()
    {
        GameObject cloud;
        
        if (useCloudPooling && cloudPool.Count > 0)
        {
            cloud = cloudPool.Dequeue();
            cloud.SetActive(true); // Reactivate
            
            // Reset transform to avoid issues
            cloud.transform.localScale = Vector3.one;
            
            // Reset image properties
            var cloudImage = cloud.GetComponent<Image>();
            if (cloudImage != null)
            {
                cloudImage.color = Color.white; // Reset color/alpha
            }
        }
        else
        {
            cloud = Instantiate(cloudPrefab, skyBG);
        }
        
        return cloud;
    }

    private void ReturnCloudToPool(GameObject cloud)
    {
        if (useCloudPooling)
        {
            cloud.SetActive(false);
            cloudPool.Enqueue(cloud);
        }
        else
        {
            Destroy(cloud);
        }
    }

    public void ToggleClouds()
    {
        cloudsEnabled = !cloudsEnabled;
        
        if (cloudsEnabled)
        {
            StartCloudSystem();
        }
        else
        {
            StopCloudSystem();
        }
        
        
        var uiManager = ServiceLocator.Get<UIManager>();
        if (uiManager != null)
        {
            uiManager.ShowMessage($"Clouds: {(cloudsEnabled ? "ON" : "OFF")}", 1f);
        }
    }

    private void ClearAllClouds()
    {
        foreach (var cloud in activeClouds)
        {
            if (cloud != null)
                ReturnCloudToPool(cloud);
        }
        activeClouds.Clear();
        
        // Reset lane tracking
        InitializeLanes();
    }
    #endregion

    #region Public API
    public void SetCloudSpeed(float multiplier)
    {
        baseCloudSpeed *= multiplier;
    }

    public void SetCloudSpawnRate(float interval)
    {
        cloudSpawnInterval = interval;
    }

    public bool IsSkyLayerActive() => skyLayer.isActive;
    public bool IsAmbientLayerActive() => ambientLayer.isActive;
    public bool AreCloudsEnabled() => cloudsEnabled;
    #endregion

    private void OnDestroy()
    {
        ServiceLocator.Unregister<BiomeManager>();
        StopCloudSystem();
    }

    // Gizmos for debugging
    private void OnDrawGizmosSelected()
    {
        if (skyBG == null) return;

        Rect skyRect = skyBG.rect;

        // Draw sky area boundary (WORLD coordinates)
        Gizmos.color = Color.cyan;
        Vector3 skyWorldPos = skyBG.TransformPoint(Vector3.zero); // Convert to world
        Gizmos.DrawWireCube(skyWorldPos, new Vector3(skyBG.rect.width, skyBG.rect.height, 0));

        // Draw cloud lanes (CORRECTED coordinates)
        Gizmos.color = Color.yellow;
        
        for (int i = 0; i < cloudLanes; i++)
        {
            float laneYLocal = CalculateLaneY(i); // Local coordinate
            Vector3 laneWorldPos = skyBG.TransformPoint(new Vector3(0, laneYLocal, 0)); // Convert to world
            
            Vector3 laneStart = new Vector3(
                skyWorldPos.x - skyBG.rect.width * 0.5f, 
                laneWorldPos.y, // Use converted world position
                0
            );
            Vector3 laneEnd = new Vector3(
                skyWorldPos.x + skyBG.rect.width * 0.5f, 
                laneWorldPos.y, // Use converted world position
                0
            );
            
            Gizmos.DrawLine(laneStart, laneEnd);
            
            #if UNITY_EDITOR
            UnityEditor.Handles.Label(laneStart, $"Lane {i}");
            #endif
        }

        // Draw spawn and exit zones
        Gizmos.color = Color.green; // Spawn zone
        Vector3 spawnZone = new Vector3(
            skyBG.position.x - skyRect.width * CLOUD_START_X_OFFSET,
            skyBG.position.y,
            0
        );
        Gizmos.DrawWireCube(spawnZone, new Vector3(20f, skyRect.height, 0));
        
        Gizmos.color = Color.red; // Exit zone
        Vector3 exitZone = new Vector3(
            skyBG.position.x + skyRect.width * CLOUD_END_X_OFFSET,
            skyBG.position.y,
            0
        );
        Gizmos.DrawWireCube(exitZone, new Vector3(20f, skyRect.height, 0));
    }
}
