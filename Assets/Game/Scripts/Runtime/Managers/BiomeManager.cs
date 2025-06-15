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
    public Sprite[] cloudSprites; // Just the sprites
    public int maxClouds = 6;
    public float cloudSpawnInterval = 3f;
    public float baseCloudSpeed = 20f;
    public Vector2 speedRange = new Vector2(0.5f, 1.5f); // Random speed multiplier
    public Vector2 scaleRange = new Vector2(0.8f, 1.2f); // Random scale
    public Vector2 opacityRange = new Vector2(0.4f, 0.8f); // Random opacity

    [Header("Testing Controls")]
    public KeyCode toggleSkyKey = KeyCode.Alpha1;
    public KeyCode toggleAmbientKey = KeyCode.Alpha2;
    public KeyCode toggleCloudsKey = KeyCode.Alpha3;

    private List<GameObject> activeClouds = new List<GameObject>();
    private Coroutine cloudSpawner;
    private bool cloudsEnabled = true;
    private RectTransform gameAreaRect;

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
        
        // Set starting position using SkyBG dimensions
        Rect skyRect = skyBG.rect;
        float startX = -skyRect.width * 0.6f; // Start off-screen left
        float randomY = Random.Range(-skyRect.height * 0.4f, skyRect.height * 0.4f); // Use 80% of sky height
        
        cloudRect.anchoredPosition = new Vector2(startX, randomY);
        
        // Start movement with random speed
        StartCoroutine(MoveCloudSimple(cloud, speedMultiplier));
        
        activeClouds.Add(cloud);
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
