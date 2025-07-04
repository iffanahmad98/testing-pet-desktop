using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CloudAmbientSystem : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private BiomeDataSO biomeData;
    [SerializeField] private GameObject cloudPrefab; // Base cloud prefab with Image component

    [Header("Pool Settings")]
    [SerializeField] private int poolSize = 20;
    [SerializeField] private Transform poolContainer;

    [Header("Spawn Settings")]
    // [SerializeField] private float spawnHeightRatio = 0.5f; // Top half of the sky
    [SerializeField] private float spawnMargin = 50f; // Margin from edges
    [SerializeField] private float minDistanceBetweenClouds = 250f; // INCREASED from 150 to 250
    [SerializeField] private float minTimeBetweenSpawns = 3.0f; // INCREASED from 1.5 to 3.0

    // Internal variables
    private BiomeManager biomeManager;
    private RectTransform skyBG;
    private List<GameObject> cloudPool;
    private List<GameObject> activeClouds;
    private bool isSystemActive = true;
    private Coroutine spawnRoutine;

    private void Awake()
    {
        // Initialize pools
        cloudPool = new List<GameObject>(poolSize);
        activeClouds = new List<GameObject>();

        // Create pool container if needed
        if (poolContainer == null)
        {
            poolContainer = new GameObject("CloudPool").transform;
            poolContainer.SetParent(transform);
            poolContainer.localPosition = Vector3.zero;
        }

        // Initialize object pool
        InitializeCloudPool();

        // Get BiomeManager reference directly from the same GameObject
        biomeManager = GetComponent<BiomeManager>();
        if (biomeManager == null)
        {
            Debug.LogError("CloudAmbientSystem: BiomeManager component not found on the same GameObject!");
        }
    }

    private void Start()
    {
        if (biomeManager != null)
        {
            skyBG = biomeManager.skyBG;
            biomeManager.OnCloudsToggled.AddListener(SetSystemActive);
        }
        else
        {
            Debug.LogError("CloudAmbientSystem: BiomeManager not found!");
            return;
        }

        // Start spawning
        spawnRoutine = StartCoroutine(SpawnCloudRoutine());
    }

    private void InitializeCloudPool()
    {
        if (cloudPrefab == null)
        {
            Debug.LogError("CloudAmbientSystem: Cloud prefab is not assigned!");
            return;
        }

        for (int i = 0; i < poolSize; i++)
        {
            GameObject cloud = Instantiate(cloudPrefab, poolContainer);
            cloud.SetActive(false);
            cloudPool.Add(cloud);
        }
    }

    private GameObject GetPooledCloud()
    {
        // Look for an available cloud in the pool
        foreach (GameObject cloud in cloudPool)
        {
            if (!cloud.activeInHierarchy)
            {
                return cloud;
            }
        }

        // If pool is exhausted, return null
        return null;
    }

    // Modify SpawnCloudRoutine to be more reliable
    private IEnumerator SpawnCloudRoutine()
    {
        // Initial wait to ensure everything is ready
        yield return new WaitForSeconds(3.0f); // Increased initial wait

        float lastSpawnTime = Time.time;

        while (true)
        {
            if (isSystemActive && biomeData != null && skyBG != null)
            {
                float timeSinceLastSpawn = Time.time - lastSpawnTime;

                // Basic check with improved timing
                if (timeSinceLastSpawn > minTimeBetweenSpawns &&
                    activeClouds.Count < (biomeData.maxClouds > 0 ? biomeData.maxClouds : 5))
                {
                    // Only spawn if there's enough horizontal space
                    if (HasEnoughHorizontalSpace())
                    {
                        SpawnCloud();
                        lastSpawnTime = Time.time;

                        // Add stronger variable delay based on cloud density
                        float extraDelay = Mathf.Lerp(0f, 5f, (float)activeClouds.Count / biomeData.maxClouds);
                        lastSpawnTime += extraDelay; // Increase effective delay as more clouds spawn

                        // Additional random delay variation (1-3 seconds)
                        lastSpawnTime += Random.Range(1f, 3f);

                        // Wait longer after spawning to prevent rapid successive checks
                        yield return new WaitForSeconds(1.0f);
                    }
                    else
                    {
                        // Wait longer if we can't spawn due to space constraints
                        yield return new WaitForSeconds(1.0f);
                    }
                }
            }

            yield return new WaitForSeconds(0.5f);
        }
    }

    // Add this new method to check for horizontal space
    private bool HasEnoughHorizontalSpace()
    {
        // Use the ACTUAL spawn X position calculation to match what SpawnCloud uses
        float skyWidth = skyBG.rect.width;
        float spawnXPosition = -skyWidth / 2 - 100f; // Estimate for cloud width
        float requiredClearance = minDistanceBetweenClouds; // Use the serialized field value

        foreach (GameObject activeCloud in activeClouds)
        {
            if (activeCloud == null) continue;

            RectTransform cloudRect = activeCloud.GetComponent<RectTransform>();
            if (cloudRect == null) continue;

            // Check if there's any cloud too close to spawn position
            float xDistance = Mathf.Abs(cloudRect.anchoredPosition.x - spawnXPosition);

            // Also consider the cloud's progress - only check clouds near the left side
            if (cloudRect.anchoredPosition.x < 0f && xDistance < requiredClearance)
            {
                return false; // Too close, don't spawn
            }
        }

        return true; // Enough space to spawn
    }

    private void SpawnCloud()
    {
        GameObject cloud = GetPooledCloud();
        if (cloud == null) return;

        // Configure the cloud
        Image cloudImage = cloud.GetComponent<Image>();
        RectTransform cloudRect = cloud.GetComponent<RectTransform>();

        if (cloudImage == null || cloudRect == null) return;

        // Set random sprite
        cloudImage.sprite = biomeData.cloudSprites[Random.Range(0, biomeData.cloudSprites.Length)];

        // Ensure full visibility
        cloudImage.color = Color.white;

        // Set random scale
        float scale = Random.Range(biomeData.cloudMinScale, biomeData.cloudMaxScale);
        cloudRect.localScale = new Vector3(scale, scale, 1f);

        // Calculate spawn position in top 40% of sky
        float skyHeight = skyBG.rect.height;
        float skyWidth = skyBG.rect.width;

        // Modified: Use only top 40% of sky area (0% is top, 100% is bottom)
        float topAreaPercentage = 0.4f; // 40% from top
        float topPosition = 0f; // Top of the sky
        float bottomPosition = skyHeight * topAreaPercentage; // 40% down from top

        // Random Y position within the top 40%
        float randomY = Random.Range(topPosition + spawnMargin, bottomPosition - spawnMargin);

        // Position cloud just off-screen to the LEFT
        cloudRect.SetParent(skyBG, false);

        // FIXED: Make sure clouds start completely off-screen to the left
        // This works regardless of skyBG anchoring
        Vector3[] skyCorners = new Vector3[4];
        skyBG.GetWorldCorners(skyCorners);

        // Calculate cloud width in local space
        float cloudWidth = cloudRect.rect.width * scale;

        // Position fully outside left edge of skyBG
        // Use -skyWidth/2 - cloudWidth for center anchors, or -cloudWidth for left anchors
        float xPosition = -skyWidth / 2 - cloudWidth; // This works for center anchors

        // If using stretch or left anchors, use this instead:
        // float xPosition = -cloudWidth - spawnMargin;

        // Set proper anchored position OUTSIDE the sky BG (left) and within top 40%
        cloudRect.anchoredPosition = new Vector2(xPosition, randomY);

        // Set up movement data with better speed randomization
        float speed = Random.Range(biomeData.cloudMinSpeed, biomeData.cloudMaxSpeed);

        // Add speed variation based on cloud size for better layering effect
        // Smaller clouds move slower, larger clouds move faster
        speed *= Mathf.Lerp(0.7f, 1.3f, (scale - biomeData.cloudMinScale) / (biomeData.cloudMaxScale - biomeData.cloudMinScale));

        Cloud mover = cloud.GetComponent<Cloud>() ?? cloud.AddComponent<Cloud>();
        mover.Initialize(speed, this, skyBG);

        // Ensure cloud is in front by setting sibling index
        cloud.transform.SetAsLastSibling();

        // Activate cloud and track it
        cloud.SetActive(true);
        activeClouds.Add(cloud);
    }

    public void ReturnToPool(GameObject cloud)
    {
        if (cloud != null)
        {
            cloud.SetActive(false);
            activeClouds.Remove(cloud);
        }
    }

    public void SetSystemActive(bool active)
    {
        isSystemActive = active;

        // If turning off, disable all current clouds
        if (!active)
        {
            foreach (GameObject cloud in activeClouds.ToArray())
            {
                ReturnToPool(cloud);
            }
            activeClouds.Clear();
        }
    }

    // Method to update biome data when switching biomes
    public void UpdateBiomeData(BiomeDataSO newBiomeData)
    {
        if (newBiomeData == null) return;

        // Update biome data reference
        biomeData = newBiomeData;

        // Clear existing clouds when changing biome
        foreach (GameObject cloud in activeClouds.ToArray())
        {
            ReturnToPool(cloud);
        }
        activeClouds.Clear();

        // The SpawnCloudRoutine will automatically use the new data
    }
    public void ToggleCloud(bool active)
    {
        if (active)
        {
            if (spawnRoutine == null)
            {
                spawnRoutine = StartCoroutine(SpawnCloudRoutine());
            }
        }
        else
        {
            if (spawnRoutine != null)
            {
                StopCoroutine(spawnRoutine);
                spawnRoutine = null;
            }

            foreach (GameObject cloud in activeClouds.ToArray())
            {
                ReturnToPool(cloud);
            }
            activeClouds.Clear();
        }
    }


    private void OnDestroy()
    {
        if (spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
        }

        if (biomeManager != null)
        {
            biomeManager.OnCloudsToggled.RemoveListener(SetSystemActive);
        }
    }
}

// Helper component to handle individual cloud movement
public class Cloud : MonoBehaviour
{
    private float speed;
    private CloudAmbientSystem parentSystem;
    private RectTransform rectTransform;
    private RectTransform skyBG;
    private Vector2 lastPosition;
    private bool loggedError = false;

    public void Initialize(float moveSpeed, CloudAmbientSystem system, RectTransform skyBGRect)
    {
        speed = moveSpeed;
        parentSystem = system;
        skyBG = skyBGRect;
        rectTransform = GetComponent<RectTransform>();
        lastPosition = rectTransform != null ? rectTransform.anchoredPosition : Vector2.zero;
    }

    private void Update()
    {
        if (rectTransform == null)
        {
            if (!loggedError)
            {
                Debug.LogError($"CloudMover: RectTransform is null on {gameObject.name}");
                loggedError = true;
            }
            return;
        }
        if (skyBG == null)
        {
            if (!loggedError)
            {
                Debug.LogError($"CloudMover: skyBG is null on {gameObject.name}");
                loggedError = true;
            }
            return;
        }

        // Move cloud from LEFT to RIGHT (changed from -= to +=)
        Vector2 position = rectTransform.anchoredPosition;
        position.x += speed * Time.deltaTime;
        rectTransform.anchoredPosition = position;

        // Debug if position isn't changing
        if (Vector2.Distance(lastPosition, position) < 0.01f && Time.frameCount % 300 == 0)
        {
            Debug.LogWarning($"Cloud not moving: Pos({position}), Speed({speed})");
        }
        lastPosition = position;

        // Check if cloud is off screen to the RIGHT (was left before)
        if (position.x > skyBG.rect.width + rectTransform.rect.width)
        {
            // Return to pool when off screen
            if (parentSystem != null)
            {
                parentSystem.ReturnToPool(gameObject);
            }
        }
    }
}