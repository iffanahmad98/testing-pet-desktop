using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

[System.Serializable]
public class CloudData
{
    public Sprite cloudSprite;
    public float speed = 1f;
    public float minScale = 0.8f;
    public float maxScale = 1.2f;
    public float opacity = 0.8f;
}

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
    [Header("Biome Layers")]
    public BiomeLayer skyLayer;
    public BiomeLayer ambientLayer;
    public BiomeLayer groundLayer;

    [Header("Cloud System - Simple Version")]
    public RectTransform skyBG;
    public GameObject cloudPrefab;
    public Sprite[] cloudSprites;
    public int maxClouds = 6;
    public float cloudSpawnInterval = 3f;
    public float baseCloudSpeed = 20f;
    public Vector2 speedRange = new Vector2(0.5f, 1.5f);
    public Vector2 scaleRange = new Vector2(0.6f, 1.0f); // Smaller clouds to reduce overlap
    public Vector2 opacityRange = new Vector2(0.4f, 0.8f);
    public int cloudLanes = 4; // Increased from 3 to 4 lanes
    public float minCloudSpacing = 100f; // Increased from 50f
    public float baseWaitTime = 5f; // Reduced since we have better spacing

    [Header("Testing Controls")]
    public KeyCode toggleSkyKey = KeyCode.Alpha1;
    public KeyCode toggleAmbientKey = KeyCode.Alpha2;
    public KeyCode toggleCloudsKey = KeyCode.Alpha3;

    private List<GameObject> activeClouds = new List<GameObject>();
    private Coroutine cloudSpawner;
    private bool cloudsEnabled = true;
    private RectTransform gameAreaRect;

    private Dictionary<int, float> lastCloudSpawnTime = new Dictionary<int, float>(); // Track last spawn time per lane
    private Dictionary<int, float> lastCloudSpeed = new Dictionary<int, float>(); // Track last cloud speed per lane

    private void Awake()
    {
        ServiceLocator.Register(this);
        InitializeBiome();
    }

    private void Start()
    {
        gameAreaRect = GetComponentInParent<RectTransform>();
        if (gameAreaRect == null)
        {
            gameAreaRect = transform.parent.GetComponent<RectTransform>();
        }

        // Initialize lane tracking
        InitializeLanes();
        StartCloudSystem();
    }

    private void Update()
    {
        HandleTestingInput();
        UpdateParallax();
    }

    private void InitializeBiome()
    {
        // Initialize layers if not set in inspector
        if (skyLayer.layerObject == null)
            skyLayer.layerObject = transform.Find("SkyBG")?.gameObject;
        if (ambientLayer.layerObject == null)
            ambientLayer.layerObject = transform.Find("AmbientBG")?.gameObject;
        if (groundLayer.layerObject == null)
            groundLayer.layerObject = transform.Find("GroundBG")?.gameObject;

        // Set initial states
        SetLayerActive(skyLayer, skyLayer.isActive);
        SetLayerActive(ambientLayer, ambientLayer.isActive);
        SetLayerActive(groundLayer, groundLayer.isActive);
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
        // Simple parallax based on time (you can later make this based on camera movement)
        float timeOffset = Time.time * 0.1f;
        
        if (skyLayer.layerObject != null && skyLayer.parallaxSpeed > 0)
        {
            var rectTransform = skyLayer.layerObject.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                Vector2 pos = rectTransform.anchoredPosition;
                pos.x = Mathf.Sin(timeOffset * skyLayer.parallaxSpeed) * 10f;
                rectTransform.anchoredPosition = pos;
            }
        }
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
        if (cloudSprites == null || cloudSprites.Length == 0 || cloudPrefab == null || skyBG == null)
            return;

        int availableLane = GetAvailableLane();
        if (availableLane == -1) // No available lanes
            return;

        GameObject cloud = Instantiate(cloudPrefab, skyBG);
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
        StartCoroutine(MoveCloudSimple(cloud, speedMultiplier));
        
        activeClouds.Add(cloud);
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
        if (timeSinceLastSpawn < totalWaitTime)
        {
            Debug.Log($"Lane {laneIndex}: Waiting {totalWaitTime - timeSinceLastSpawn:F1}s more (Base: {baseWait}s + Spacing: {spacingWait:F1}s)");
        }
        
        return timeSinceLastSpawn >= totalWaitTime;
    }

    private float CalculateLaneY(int laneIndex)
    {
        Rect skyRect = skyBG.rect;
        float laneHeight = skyRect.height * 0.8f; // Use 80% of sky height
        float laneSpacing = laneHeight / (cloudLanes + 1); // +1 for even spacing
        
        float startY = -laneHeight * 0.5f;
        return startY + ((laneIndex + 1) * laneSpacing);
    }

    private IEnumerator MoveCloudSimple(GameObject cloud, float speedMultiplier)
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
                Destroy(cloud);
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
        if (skyBG != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(skyBG.position, new Vector3(skyBG.rect.width, skyBG.rect.height, 0));
        }
    }
}
