using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;

public class MonsterManager : MonoBehaviour
{
    #region Fields and Variables
    [Header("Prefabs & Pool Settings")]
    public GameObject monsterPrefab;
    public GameObject foodPrefab;
    public GameObject medicinePrefab;
    public GameObject poopPrefab;
    public GameObject coinPrefab;
    public RectTransform poolContainer;
    public int initialPoolSize = 50;

    [Header("Game Settings")]
    public RectTransform gameArea;
    public Canvas mainCanvas;
    public MonsterDatabaseSO monsterDatabase;

    [Header("Food Placement Settings")]
    public GameObject foodPlacementIndicator;
    public Color validPositionColor = Color.green;
    public Color invalidPositionColor = Color.red;

    [Header("Rendering Settings")]
    public bool enableDepthSorting = true;
    private float lastSortTime = 0f;
    private float sortInterval = 0.1f;
    private Camera _mainCamera;
    private RectTransform _foodIndicatorRT;
    private Image _foodIndicatorImage;

    private Queue<GameObject> _foodPool = new Queue<GameObject>();
    private Queue<GameObject> _medicinePool = new Queue<GameObject>();
    private Queue<GameObject> _poopPool = new Queue<GameObject>();
    private Queue<GameObject> _coinPool = new Queue<GameObject>();

    private List<GameObject> _activeCoins = new List<GameObject>();
    private List<GameObject> _activePoops = new List<GameObject>();
    [HideInInspector] public List<MonsterController> activeMonsters = new List<MonsterController>();
    [HideInInspector] public int poopCollected;
    [HideInInspector] public int coinCollected;
    public List<FoodController> activeFoods = new List<FoodController>();
    public List<MedicineController> activeMedicines = new List<MedicineController>();
    private List<string> savedMonIDs = new List<string>();

    private bool isInPlacementMode = false;
    private int pendingFoodCost = 0;

    public System.Action<int> OnCoinChanged;
    public System.Action<int> OnPoopChanged;
    #endregion

    #region Initialization and Setup
    private void Awake()
    {
        ServiceLocator.Register(this);
        InitializePools();
        CacheComponents();
    }

    private void CacheComponents()
    {
        _mainCamera = mainCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : mainCanvas.worldCamera;
        _foodIndicatorRT = foodPlacementIndicator.GetComponent<RectTransform>();
        _foodIndicatorImage = foodPlacementIndicator.GetComponent<Image>();
    }

    private void InitializePools()
    {
        for (int i = 0; i < initialPoolSize; i++)
        {
            CreatePoolObject(foodPrefab, _foodPool);
            CreatePoolObject(poopPrefab, _poopPool);
            CreatePoolObject(coinPrefab, _coinPool);
        }
    }

    private void CreatePoolObject(GameObject prefab, Queue<GameObject> pool)
    {
        var obj = Instantiate(prefab, poolContainer);
        obj.SetActive(false);
        pool.Enqueue(obj);
    }
    #endregion

    #region Game Loop
    void Start() => LoadGame();

    void Update()
    {
        if (enableDepthSorting && Time.time - lastSortTime >= sortInterval)
        {
            SortMonstersByDepth();
            lastSortTime = Time.time;
        }
    }
    #endregion

    #region Monster Management
    public void SpawnMonster(MonsterDataSO monsterData = null, string id = null)
    {
        GameObject monster = CreateMonster(monsterData);

        var controller = monster.GetComponent<MonsterController>();

        if (!string.IsNullOrEmpty(id))
        {
            controller.monsterID = id;
            var (_, evolutionLevel) = GetMonsterDataAndLevelFromID(id);
            if (evolutionLevel > 0) controller.evolutionLevel = evolutionLevel;
        }
        else
        {
            var data = controller.MonsterData;
            controller.monsterID = $"{data.id}_Stage{controller.evolutionLevel}_{System.Guid.NewGuid().ToString("N")[..8]}";
        }

        controller.LoadMonData();

        var finalData = controller.MonsterData;
        monster.name = $"{finalData.monsterName}_{controller.monsterID}";

        if (string.IsNullOrEmpty(id))
        {
            RegisterMonster(controller);
        }
        else
            RegisterActiveMonster(controller);
    }

    private GameObject CreateMonster(MonsterDataSO monsterData = null)
    {
        var monster = Instantiate(monsterPrefab, gameArea);

        var monsterController = monster.GetComponent<MonsterController>();
        var rectTransform = monster.GetComponent<RectTransform>();
        var movementBounds = new MonsterBoundsHandler(this, rectTransform);
        Vector2 spawnPosition = movementBounds.GetRandomSpawnTarget();
        monster.transform.localPosition = spawnPosition;

        if (monsterController != null)
        {
            MonsterDataSO _data = monsterData;

            // If no data provided, pick random from database
            if (_data == null && monsterDatabase != null && monsterDatabase.monsters.Count > 0)
            {
                _data = monsterDatabase.monsters[UnityEngine.Random.Range(0, monsterDatabase.monsters.Count)];
            }

            if (_data != null)
            {
                monsterController.SetMonsterData(_data);
                monster.name = $"{_data.monsterName}_Temp";
            }
            else
            {
                monster.name = "Monster_Temp";
            }
        }
        else
        {
            monster.name = "Monster_Temp";
        }

        return monster;
    }

    private void RegisterMonster(MonsterController monsterController)
    {
        savedMonIDs.Add(monsterController.monsterID);
        SaveSystem.SaveMonIDs(savedMonIDs);
        RegisterActiveMonster(monsterController);
    }

    public void RegisterActiveMonster(MonsterController monster)
    {
        if (!activeMonsters.Contains(monster))
        {
            activeMonsters.Add(monster);
        }
    }

    public void RemoveSavedMonsterID(string monsterID)
    {
        activeMonsters.RemoveAll(m => m.monsterID == monsterID);
        savedMonIDs.Remove(monsterID);
        SaveSystem.SaveMonIDs(savedMonIDs);
    }

    public void AddSavedMonsterID(string monsterID)
    {
        if (!savedMonIDs.Contains(monsterID))
        {
            savedMonIDs.Add(monsterID);
            SaveSystem.SaveMonIDs(savedMonIDs);
        }
    }

    public void BuyMons(int cost = 10)
    {
        if (SpentCoin(cost)) SpawnMonster();
    }

    private (MonsterDataSO, int) GetMonsterDataAndLevelFromID(string monsterID)
    {
        var parts = monsterID.Split('_');
        if (parts.Length >= 2)
        {
            string monsterTypeId = parts[0];
            string levelPart = parts[1];
            int evolutionLevel = 0;

            if (levelPart.StartsWith("Stage"))
            {
                if (!int.TryParse(levelPart.Substring(5), out evolutionLevel))
                {
                    evolutionLevel = 0;
                }
            }

            foreach (var data in monsterDatabase.monsters)
            {
                if (data.id == monsterTypeId)
                {
                    return (data, evolutionLevel);
                }
            }
        }

        return (null, 0);
    }

    public void SortMonstersByDepth()
    {
        if (activeMonsters.Count <= 1) return;

        // Use array for better performance with large lists
        var validMonsters = new List<MonsterController>(activeMonsters.Count);

        // Filter out null/inactive monsters first
        for (int i = activeMonsters.Count - 1; i >= 0; i--)
        {
            var monster = activeMonsters[i];
            if (monster == null || !monster.gameObject.activeInHierarchy)
            {
                activeMonsters.RemoveAt(i); // Clean up invalid references
            }
            else
            {
                validMonsters.Add(monster);
            }
        }

        // Sort by Y position (higher Y = lower sibling index)
        validMonsters.Sort((a, b) =>
            b.transform.position.y.CompareTo(a.transform.position.y));

        // Update sibling indices, starting from index 1 to preserve background at index 0
        for (int i = 0; i < validMonsters.Count; i++)
        {
            validMonsters[i].transform.SetSiblingIndex(i + 1); // +1 to skip background
        }
    }
    #endregion

    #region Consumable Management
    public void SpawnItem(ItemDataSO data, Vector2 pos)
    {
        GameObject pooled = null;

        switch (data.category)
        {
            case ItemType.Food:
                pooled = GetPooledObject(_foodPool, foodPrefab);
                break;

            case ItemType.Medicine:
                pooled = GetPooledObject(_medicinePool, medicinePrefab);
                break;

            default:
                Debug.LogWarning($"SpawnItem: Unsupported item type: {data.category}");
                return;
        }

        if (pooled != null)
        {
            SetupPooledObject(pooled, gameArea, pos);

            if (pooled.TryGetComponent<IConsumable>(out var consumable))
            {
                consumable.Initialize(data);
            }

            // Register into the correct active list
            switch (data.category)
            {
                case ItemType.Food:
                    if (pooled.TryGetComponent<FoodController>(out var foodCtrl) && !activeFoods.Contains(foodCtrl))
                    {
                        activeFoods.Add(foodCtrl);
                    }
                    break;

                case ItemType.Medicine:
                    if (pooled.TryGetComponent<MedicineController>(out var medCtrl) && !activeMedicines.Contains(medCtrl))
                    {
                        activeMedicines.Add(medCtrl);
                    }
                    break;
            }
        }
    }




    #endregion

    #region Item Management
    public GameObject SpawnCoinWithArc(Vector2 startPosition, Vector2 targetPosition, CoinType type)
    {
        var coin = GetPooledObject(_coinPool, coinPrefab);
        if (coin != null)
        {
            _activeCoins.Add(coin);
            SetupPooledObject(coin, gameArea, startPosition);
            coin.GetComponent<CoinController>().Initialize(type);

            // Start arc animation coroutine
            StartCoroutine(AnimateCoinArc(coin.transform, startPosition, targetPosition));
        }
        return coin;
    }

    private IEnumerator AnimateCoinArc(Transform coinTransform, Vector2 startPos, Vector2 endPos)
    {
        float duration = 0.8f; // Arc animation duration
        float arcHeight = 100f; // How high the coin goes
        float elapsedTime = 0f;

        RectTransform coinRect = coinTransform.GetComponent<RectTransform>();
        if (coinRect == null) yield break;

        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;

            // Ease out curve for natural falling motion
            float easedT = 1f - (1f - t) * (1f - t);

            // Calculate arc position
            Vector2 currentPos = Vector2.Lerp(startPos, endPos, easedT);

            // Add vertical arc (parabolic motion)
            float arcProgress = 4f * t * (1f - t); // Peaks at t=0.5
            currentPos.y += arcHeight * arcProgress;

            coinRect.anchoredPosition = currentPos;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure final position
        coinRect.anchoredPosition = endPos;

        // Optional: Add a small bounce effect on landing
        StartCoroutine(CoinBounceEffect(coinRect));
    }

    private IEnumerator CoinBounceEffect(RectTransform coinRect)
    {
        Vector2 finalPos = coinRect.anchoredPosition;
        float bounceHeight = 15f;
        float bounceDuration = 0.3f;
        float elapsedTime = 0f;

        while (elapsedTime < bounceDuration)
        {
            float t = elapsedTime / bounceDuration;
            float bounce = bounceHeight * (1f - t) * Mathf.Sin(t * Mathf.PI * 3f); // 3 small bounces

            coinRect.anchoredPosition = finalPos + Vector2.up * bounce;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        coinRect.anchoredPosition = finalPos;
    }

    public GameObject SpawnPoopAt(Vector2 anchoredPos, PoopType type)
    {
        // Find a non-overlapping position
        Vector2 finalPos = FindNonOverlappingPosition(anchoredPos, 50f);

        var poop = GetPooledObject(_poopPool, poopPrefab);
        SetupPooledObject(poop, gameArea, finalPos);
        poop.GetComponent<PoopController>().Initialize(type);
        return poop;
    }

    private Vector2 FindNonOverlappingPosition(Vector2 preferredPos, float minDistance)
    {
        Vector2 testPos = preferredPos;
        int maxAttempts = 10;

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            bool hasOverlap = false;

            // Check against active coins and poop
            foreach (Transform child in gameArea)
            {
                if (child.gameObject.activeInHierarchy &&
                    (child.GetComponent<CoinController>() != null || child.GetComponent<PoopController>() != null))
                {
                    float distance = Vector2.Distance(testPos, child.GetComponent<RectTransform>().anchoredPosition);
                    if (distance < minDistance)
                    {
                        hasOverlap = true;
                        break;
                    }
                }
            }

            if (!hasOverlap)
                return testPos;

            // Try a random nearby position
            Vector2 randomOffset = Random.insideUnitCircle * (minDistance * 2f);
            testPos = preferredPos + randomOffset;

            // Keep within game area bounds
            var rect = gameArea.rect;
            testPos.x = Mathf.Clamp(testPos.x, rect.xMin + 25f, rect.xMax - 25f);
            testPos.y = Mathf.Clamp(testPos.y, rect.yMin + 25f, rect.yMax - 25f);
        }

        return testPos;
    }

    public bool SpentCoin(int amount)
    {
        if (coinCollected < amount) return false;

        coinCollected -= amount;
        SaveSystem.SaveCoin(coinCollected);
        OnCoinChanged?.Invoke(coinCollected);
        return true;
    }

    public void CollectPoop(int amount = 1)
    {
        poopCollected += amount;
        SaveSystem.SavePoop(poopCollected);
        OnPoopChanged?.Invoke(poopCollected);
    }
    #endregion

    #region Object Pooling
    private GameObject GetPooledObject(Queue<GameObject> pool, GameObject prefab)
    {
        return pool.Count > 0 ? pool.Dequeue() : Instantiate(prefab, poolContainer);
    }

    private void SetupPooledObject(GameObject obj, RectTransform parent, Vector2 position)
    {
        var rt = obj.GetComponent<RectTransform>();
        rt.SetParent(parent, false);

        rt.localScale = Vector3.one;
        rt.localRotation = Quaternion.identity;

        var indicatorRT = foodPlacementIndicator.GetComponent<RectTransform>();
        rt.anchorMin = indicatorRT.anchorMin;
        rt.anchorMax = indicatorRT.anchorMax;
        rt.pivot = indicatorRT.pivot;

        rt.anchoredPosition = position;
        obj.SetActive(true);
    }

    public void DespawnToPool(GameObject obj)
    {
        obj.transform.SetParent(poolContainer, false);
        obj.SetActive(false);

        // More reliable than name checking
        if (obj.TryGetComponent<FoodController>(out var food))
        {
            _foodPool.Enqueue(obj);
            activeFoods.Remove(food);
        }
        else if (obj.TryGetComponent<MedicineController>(out var medicine))
        {
            _medicinePool.Enqueue(obj);
            activeMedicines.Remove(medicine);
        }
        else if (obj.TryGetComponent<CoinController>(out _))
        {
            _coinPool.Enqueue(obj);
            _activeCoins.Remove(obj);
        }
        else if (obj.name.Contains("Poop"))
        {
            _poopPool.Enqueue(obj);
            CollectPoop();
            _activePoops.Remove(obj);
        }
    }
    #endregion

    #region Save and Load
    private void LoadGame()
    {
        coinCollected = SaveSystem.LoadCoin();
        poopCollected = SaveSystem.LoadPoop();
        savedMonIDs = SaveSystem.LoadSavedMonIDs();

        foreach (var id in savedMonIDs)
        {
            if (SaveSystem.LoadMon(id, out _))
            {
                var (monsterData, evolutionLevel) = GetMonsterDataAndLevelFromID(id);
                if (monsterData != null)
                {
                    SpawnMonster(monsterData, id);
                }
            }
        }
    }

    public void SaveAllMons()
    {
        foreach (var monster in activeMonsters)
        {
            var saveData = new MonsterSaveData
            {
                monsterId = monster.monsterID,
                lastHunger = monster.StatsHandler.CurrentHunger,
                lastHappiness = monster.StatsHandler.CurrentHappiness,
                lastLowHungerTime = monster.GetLowHungerTime(),
                isSick = monster.IsSick,
                evolutionLevel = monster.evolutionLevel,
                timeSinceCreation = monster.GetEvolutionTimeSinceCreation(),
                nutritionCount = monster.GetEvolutionFoodConsumed(),
                interactionCount = monster.GetEvolutionInteractionCount()
            };
            SaveSystem.SaveMon(saveData);
        }
        SaveSystem.SaveMonIDs(savedMonIDs);
        SaveSystem.Flush();
    }

    private void SaveGameData()
    {
        SaveAllMons();

        SaveSystem.SavePoop(poopCollected);
        SaveSystem.SaveCoin(coinCollected);
        SaveSystem.Flush();
    }
    #endregion

    #region Utility Methods
    private Vector2 ScreenToGameAreaPosition()
    {
        var cam = mainCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : mainCanvas.worldCamera;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            gameArea, Input.mousePosition, cam, out Vector2 localPoint);

        return localPoint;
    }

    public bool IsPositionInGameArea(Vector2 localPosition)
    {
        var rect = gameArea.rect;
        return localPosition.x >= rect.xMin && localPosition.x <= rect.xMax &&
               localPosition.y >= rect.yMin && localPosition.y <= rect.yMax;
    }

    public bool IsPositionClearOfObjects(Vector2 position, float radius = 40f)
    {
        // Check coins
        foreach (var coin in _activeCoins)
        {
            if (coin != null && coin.activeInHierarchy)
            {
                var coinRect = coin.GetComponent<RectTransform>();
                if (coinRect != null)
                {
                    float distance = Vector2.Distance(position, coinRect.anchoredPosition);
                    if (distance < radius) return false;
                }
            }
        }

        // Check poop
        foreach (var poop in _activePoops)
        {
            if (poop != null && poop.activeInHierarchy)
            {
                var poopRect = poop.GetComponent<RectTransform>();
                if (poopRect != null)
                {
                    float distance = Vector2.Distance(position, poopRect.anchoredPosition);
                    if (distance < radius) return false;
                }
            }
        }

        return true;
    }
    #endregion

    #region Lifecycle Events
    void OnApplicationQuit() => SaveGameData();
    void OnApplicationPause(bool pauseStatus) { if (pauseStatus) SaveGameData(); }

#if UNITY_EDITOR
    void OnDisable() { if (Application.isPlaying) SaveGameData(); }
#endif

    void OnDestroy() => ServiceLocator.Unregister<MonsterManager>();
    #endregion
}