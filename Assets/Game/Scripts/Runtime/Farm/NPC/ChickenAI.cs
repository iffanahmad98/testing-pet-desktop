using MagicalGarden.Farm;
using MagicalGarden.Manager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ChickenAI : MonoBehaviour
{
    [SerializeField]
    private string _currentState;

    [Header("Animation")]
    public Animator animator;
    public SpriteRenderer spriteRenderer;

    [Header("Movement Settings")]
    public float walkSpeed = 1f;
    public Tilemap terrainTilemap;
    [Tooltip("Pixel offset from tilemap edges to prevent stopping at edge")]
    public float edgeOffset = 0.2f;
    [Tooltip("Enable physics-based collision detection to prevent passing through objects")]
    public bool usePhysicsCollision = true;

    [Header("Physics Components (Auto-detected)")]
    [Tooltip("Rigidbody2D component - will be auto-detected or added")]
    public Rigidbody2D rb2d;
    [Tooltip("Collider2D component - will be auto-detected")]
    public Collider2D col2d;

    [Header("State Probabilities")]
    [Range(0f, 1f)] public float idleProbability = 0.3f;
    [Range(0f, 1f)] public float walkProbability = 0.7f;

    [Header("Timing Settings")]
    public float minIdleTime = 1f;
    public float maxIdleTime = 3f;
    public float minWaitBetweenStates = 0.5f;
    public float maxWaitBetweenStates = 1.5f;
    public float eatDuration = 3f;

    [Header("Gift Drop Settings")]
    [Tooltip("Prefab objek gift/chest yang akan di-spawn (legacy - gunakan giftPrefabs untuk multiple sizes)")]
    public GameObject giftPrefab;

    [Header("Multiple Gift Sizes (Optional)")]
    [Tooltip("Small gift prefab (default jika tidak di-set)")]
    public GameObject smallGiftPrefab;
    [Tooltip("Medium gift prefab")]
    public GameObject mediumGiftPrefab;
    [Tooltip("Large gift prefab")]
    public GameObject largeGiftPrefab;

    [Header("Gift Size Probabilities")]
    [Tooltip("Chance untuk spawn Small gift (0-100%)")]
    [Range(0f, 100f)] public float smallGiftChance = 70f;
    [Tooltip("Chance untuk spawn Medium gift (0-100%)")]
    [Range(0f, 100f)] public float mediumGiftChance = 25f;
    [Tooltip("Chance untuk spawn Large gift (0-100%)")]
    [Range(0f, 100f)] public float largeGiftChance = 5f;

    [Header("Drop Timing")]
    [Tooltip("Interval waktu dalam MENIT untuk drop gift")]
    public float dropIntervalMinutes = 5f;
    [Tooltip("Maksimal jumlah gift yang bisa ada di dunia")]
    public int maxGiftCount = 4;
    [Tooltip("Offset posisi spawn gift dari posisi chicken")]
    public Vector3 giftSpawnOffset = Vector3.zero;

    [Tooltip("Interval waktu berkokoknya ayam (dalam detik).")]
    public float chickenCluckInterval = 60;
    private Coroutine cluckCoroutine;

    [Tooltip("Interval waktu melenguh sapi (dalam detik).")]
    public float cowMooInterval = 60;
    private Coroutine mooCoroutine;

    // Internal state
    private Vector2Int currentTile;
    private string currentState = "";
    private int lastDirection = -1; // -1 = left, 1 = right
    private Coroutine stateLoopCoroutine;

    // Gift management
    private Queue<GameObject> spawnedGifts = new Queue<GameObject>();
    private Coroutine giftDropCoroutine;

    void Start()
    {
        // Get components
        if (animator == null)
            animator = GetComponent<Animator>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        // Setup physics components if enabled
        if (usePhysicsCollision)
        {
            SetupPhysicsComponents();
        }

        // Get tilemap from TileManager if not assigned
        if (terrainTilemap == null && TileManager.Instance != null)
        {
            terrainTilemap = TileManager.Instance.tilemapWalkingAreaHotel;
        }

        // Initialize current tile position
        Vector3 worldPos = transform.position;
        Vector3Int cellPos = terrainTilemap.WorldToCell(worldPos);
        currentTile = new Vector2Int(cellPos.x, cellPos.y);

        // Start AI state loop
        stateLoopCoroutine = StartCoroutine(StateLoop());

        // Start gift drop system if prefab is assigned
        if (giftPrefab != null && dropIntervalMinutes > 0)
        {
            giftDropCoroutine = StartCoroutine(GiftDropLoop());
        }

        StartAnimalSoundCoroutine();
    }

    public void StartAnimalSoundCoroutine()
    {
        Debug.Log("Starts animal sound coroutine");
        if (chickenCluckInterval > 0 && cluckCoroutine == null)
            cluckCoroutine = StartCoroutine(MakeChickenCluck());

        if (cowMooInterval > 0 && mooCoroutine == null)
            mooCoroutine = StartCoroutine(MakeCowMoo());
    }

    private void SetupPhysicsComponents()
    {
        // Get or add Rigidbody2D
        if (rb2d == null)
        {
            rb2d = GetComponent<Rigidbody2D>();
            if (rb2d == null)
            {
                rb2d = gameObject.AddComponent<Rigidbody2D>();
                Debug.Log("ChickenAI: Rigidbody2D added automatically");
            }
        }

        // Configure Rigidbody2D for kinematic movement
        rb2d.bodyType = RigidbodyType2D.Kinematic;
        rb2d.gravityScale = 0f;

        // Get Collider2D (should be manually added in prefab/scene)
        if (col2d == null)
        {
            col2d = GetComponent<Collider2D>();
            if (col2d == null)
            {
                // Auto-add a CircleCollider2D as fallback
                CircleCollider2D circleCol = gameObject.AddComponent<CircleCollider2D>();
                circleCol.radius = 0.3f; // Adjust based on chicken size
                col2d = circleCol;
                Debug.Log("ChickenAI: CircleCollider2D added automatically with radius 0.3");
            }
        }
    }

    void OnDestroy()
    {
        if (stateLoopCoroutine != null)
        {
            StopCoroutine(stateLoopCoroutine);
        }

        if (giftDropCoroutine != null)
        {
            StopCoroutine(giftDropCoroutine);
        }

        StopAnimalSoundCoroutine();

        // Clean up spawned gifts
        CleanupAllGifts();
    }

    public void StopAnimalSoundCoroutine()
    {
        if (cluckCoroutine != null)
        {
            StopCoroutine(cluckCoroutine);
            cluckCoroutine = null;
            Debug.Log("Stops cluckCoroutine");
        }

        if (mooCoroutine != null)
        {
            StopCoroutine(mooCoroutine);
            mooCoroutine = null;
            Debug.Log("Stops mooCoroutine");
        }

        MonsterManager.instance.audio.StopAllSFX();
    }

    #region State Machine

    private IEnumerator StateLoop()
    {
        while (true)
        {
            // Choose random state based on probabilities
            string chosenState = GetRandomState();
            _currentState = chosenState;
            // Execute state
            yield return HandleState(chosenState);

            // Wait before next state
            yield return new WaitForSeconds(Random.Range(minWaitBetweenStates, maxWaitBetweenStates));
        }
    }

    private string GetRandomState()
    {
        // Normalize probabilities
        float total = idleProbability + walkProbability;
        float normalizedIdle = idleProbability / total;

        float rand = Random.value;

        if (rand < normalizedIdle)
            return "idle";
        else
            return "walk";
    }

    private IEnumerator HandleState(string stateName)
    {
        switch (stateName)
        {
            case "idle":
                yield return IdleState();
                break;
            case "walk":
                yield return WalkState();
                break;
            default:
                yield return IdleState();
                break;
        }
    }

    #endregion

    #region States

    private IEnumerator IdleState()
    {
        // Check if there's a plant at current position
        if (IsPlantAtCurrentPosition())
        {
            // If there's a plant, eat instead of idle
            yield return EatState();
        }
        else
        {
            // Normal idle
            SetAnimation("idle");

            // farm chicken cluck sfx is at index 3
            MonsterManager.instance.audio.PlayFarmSFX(3);
            yield return new WaitForSeconds(Random.Range(minIdleTime, maxIdleTime));
        }
    }

    private IEnumerator WalkState()
    {
        // Update current tile position
        Vector3Int tile = terrainTilemap.WorldToCell(transform.position);
        currentTile = new Vector2Int(tile.x, tile.y);

        // Possible directions
        Vector2Int[] directions = new Vector2Int[]
        {
            new Vector2Int(1, 0),   // right
            new Vector2Int(-1, 0),  // left
            new Vector2Int(0, 1),   // up
            new Vector2Int(0, -1)   // down
        };

        // Shuffle directions for random wander
        ShuffleArray(directions);

        bool moved = false;

        // Try each direction until we find a valid path
        foreach (var dir in directions)
        {
            Vector2Int targetTile = currentTile + dir;

            // Check if path is clear
            if (!IsWalkableTile(targetTile))
            {
                continue;
            }

            // Get world position of target tile
            Vector3 targetPos = GridToWorld(targetTile);

            // Additional physics collision check
            if (usePhysicsCollision && !CanMoveToPosition(targetPos))
            {
                continue;
            }

            // Start walking animation
            SetAnimation("walk");

            // Flip sprite based on direction
            FlipByTarget(transform.position, targetPos);

            // Move to target
            while (Vector3.Distance(transform.position, targetPos) > 0.01f)
            {
                // Use physics-based movement if enabled
                if (usePhysicsCollision && rb2d != null)
                {
                    Vector3 newPos = Vector3.MoveTowards(transform.position, targetPos, walkSpeed * Time.deltaTime);
                    rb2d.MovePosition(newPos);
                }
                else
                {
                    transform.position = Vector3.MoveTowards(transform.position, targetPos, walkSpeed * Time.deltaTime);
                }
                yield return null;
            }

            transform.position = targetPos;
            currentTile = targetTile;
            moved = true;
            break;
        }

        // After moving, check if we should eat
        if (moved && IsPlantAtCurrentPosition())
        {
            yield return EatState();
        }
        else
        {
            // Brief idle after walking
            SetAnimation("idle");
            yield return new WaitForSeconds(Random.Range(0.3f, 0.8f));
        }
    }

    private IEnumerator EatState()
    {
        SetAnimation("eat");
        yield return new WaitForSeconds(eatDuration);
    }

    #endregion

    #region Animation

    private void SetAnimation(string animName)
    {
        if (animator == null)
            return;

        if (currentState == animName)
            return;

        currentState = animName;

        // Reset all animation states
        animator.SetBool("idle", false);
        animator.SetBool("walk", false);
        animator.SetBool("eat", false);

        // Set the requested animation
        switch (animName)
        {
            case "idle":
                animator.SetBool("idle", true);
                break;
            case "walk":
                animator.SetBool("walk", true);
                break;
            case "eat":
                animator.SetBool("eat", true);
                break;
            default:
                animator.SetBool("idle", true);
                break;
        }
    }

    private void FlipByTarget(Vector3 currentPos, Vector3 targetPos)
    {
        if (spriteRenderer == null)
            return;

        float deltaX = targetPos.x - currentPos.x;

        if (deltaX > 0.01f)
        {
            spriteRenderer.flipX = true; // facing right
            lastDirection = 1;
        }
        else if (deltaX < -0.01f)
        {
            spriteRenderer.flipX = false; // facing left
            lastDirection = -1;
        }
    }

    #endregion

    #region Tile Helpers

    /// <summary>
    /// Check if chicken can move to target position without colliding with obstacles
    /// Uses Physics2D raycast/overlap check
    /// </summary>
    private bool CanMoveToPosition(Vector3 targetPos)
    {
        if (col2d == null)
            return true; // No collider, allow movement

        // Get current position
        Vector2 currentPos = transform.position;
        Vector2 target = targetPos;
        Vector2 direction = (target - currentPos).normalized;
        float distance = Vector2.Distance(currentPos, target);

        // Method 1: Raycast from current to target position
        RaycastHit2D hit = Physics2D.Raycast(currentPos, direction, distance, LayerMask.GetMask("Default", "Obstacle"));
        if (hit.collider != null && hit.collider != col2d)
        {
            // Hit something that's not ourselves
            Debug.Log($"ChickenAI: Collision detected with {hit.collider.name}, cannot move");
            return false;
        }

        // Method 2: Check if target position overlaps with any collider
        Collider2D[] overlaps = Physics2D.OverlapCircleAll(target, 0.2f, LayerMask.GetMask("Default", "Obstacle"));
        foreach (var overlap in overlaps)
        {
            // Ignore self and triggers
            if (overlap == col2d || overlap.isTrigger)
                continue;

            // Found a blocking collider at target position
            Debug.Log($"ChickenAI: Target position blocked by {overlap.name}");
            return false;
        }

        return true; // Path is clear
    }

    private bool IsWalkableTile(Vector2Int tileCoord)
    {
        Vector3Int gridPos = new Vector3Int(tileCoord.x, tileCoord.y, 0);
        var tile = terrainTilemap.GetTile(gridPos);

        if (tile is CustomTile customTile)
        {
            return customTile.tileType == TileType.Walkable ||
                   customTile.tileType == TileType.WalkableElevated;
        }

        return false;
    }

    private Vector3 GridToWorld(Vector2Int tile)
    {
        Vector3Int gridPos = new Vector3Int(tile.x, tile.y, 0);
        Vector3 worldPos = terrainTilemap.CellToWorld(gridPos) + terrainTilemap.cellSize / 2;

        var tileBase = terrainTilemap.GetTile(gridPos);
        if (tileBase is CustomTile customTile && customTile.tileType == TileType.WalkableElevated)
        {
            worldPos.y += customTile.offsetElevated;
        }

        // Apply edge offset to prevent stopping exactly at edge
        worldPos = ApplyEdgeOffset(tile, worldPos);

        return new Vector3(worldPos.x, worldPos.y, transform.position.z);
    }

    private Vector3 ApplyEdgeOffset(Vector2Int tile, Vector3 worldPos)
    {
        // Check all 4 directions for edges
        bool hasLeftEdge = !IsWalkableTile(tile + new Vector2Int(-1, 0));
        bool hasRightEdge = !IsWalkableTile(tile + new Vector2Int(1, 0));
        bool hasTopEdge = !IsWalkableTile(tile + new Vector2Int(0, 1));
        bool hasBottomEdge = !IsWalkableTile(tile + new Vector2Int(0, -1));

        // Apply offset to move away from edges
        if (hasLeftEdge)
            worldPos.x += edgeOffset;
        if (hasRightEdge)
            worldPos.x -= edgeOffset;
        if (hasBottomEdge)
            worldPos.y += edgeOffset;
        if (hasTopEdge)
            worldPos.y -= edgeOffset;

        return worldPos;
    }

    private bool IsPlantAtCurrentPosition()
    {
        if (PlantManager.Instance == null)
            return false;

        // Get current tile position
        Vector3Int cellPos = new Vector3Int(currentTile.x, currentTile.y, 0);

        // Check if there's a plant at this position in PlantManager
        var allPlants = PlantManager.Instance.GetAllSeeds();
        foreach (var plant in allPlants)
        {
            if (plant.seed.cellPosition == cellPos)
            {
                // There's a plant here (bush/flower/etc)
                return true;
            }
        }

        return false;
    }

    #endregion

    #region Utilities

    private void ShuffleArray<T>(T[] array)
    {
        for (int i = array.Length - 1; i > 0; i--)
        {
            int rand = Random.Range(0, i + 1);
            (array[i], array[rand]) = (array[rand], array[i]);
        }
    }

    #endregion

    #region Gift Drop System

    /// <summary>
    /// Coroutine yang berjalan terus-menerus untuk drop gift secara berkala
    /// </summary>
    private IEnumerator GiftDropLoop()
    {
        while (true)
        {
            // Konversi menit ke detik
            float dropIntervalSeconds = dropIntervalMinutes * 60f;

            // Tunggu sesuai interval
            yield return new WaitForSeconds(dropIntervalSeconds);

            // Spawn gift
            SpawnGift();
        }
    }

    private IEnumerator MakeChickenCluck()
    {
        while (true)
        {
            Debug.Log("MakeChickenCluck() called");
            yield return new WaitForSeconds(chickenCluckInterval);

            // Chicken cluck is at index 3
            MonsterManager.instance.audio.PlayFarmSFX(3);
        }
    }

    private IEnumerator MakeCowMoo()
    {
        while (true)
        {
            yield return new WaitForSeconds(cowMooInterval);

            // Cow moo is at index 4
            MonsterManager.instance.audio.PlayFarmSFX(4);
        }
    }

    /// <summary>
    /// Spawn gift di posisi chicken saat ini
    /// Jika sudah mencapai maxGiftCount, hapus gift tertua
    /// </summary>
    private void SpawnGift()
    {
        // Get random gift prefab based on size
        GameObject prefabToSpawn = GetRandomGiftPrefab();

        if (prefabToSpawn == null)
        {
            Debug.LogWarning("ChickenAI: Tidak ada gift prefab yang di-assign!");
            return;
        }

        // Cek apakah sudah mencapai maksimal gift
        if (spawnedGifts.Count >= maxGiftCount)
        {
            // Hapus gift tertua (yang pertama di-spawn)
            GameObject oldestGift = spawnedGifts.Dequeue();
            if (oldestGift != null)
            {
                Destroy(oldestGift);
                Debug.Log($"ChickenAI: Gift tertua dihapus. Sisa gift: {spawnedGifts.Count}");
            }
        }

        // Spawn gift baru di posisi chicken
        Vector3 spawnPosition = transform.position + giftSpawnOffset;
        GameObject newGift = Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity);

        // Drop gift sfx is at index 2
        MonsterManager.instance.audio.PlayFarmSFX(2);

        // Simpan reference gift ke queue
        spawnedGifts.Enqueue(newGift);

        Debug.Log($"ChickenAI: Gift di-spawn ({prefabToSpawn.name})! Total gift aktif: {spawnedGifts.Count}/{maxGiftCount}");
    }

    /// <summary>
    /// Get random gift prefab berdasarkan probability
    /// </summary>
    private GameObject GetRandomGiftPrefab()
    {
        // Jika tidak ada specific size prefabs, gunakan legacy giftPrefab
        if (smallGiftPrefab == null && mediumGiftPrefab == null && largeGiftPrefab == null)
        {
            return giftPrefab;
        }

        // Normalize probabilities
        float totalChance = smallGiftChance + mediumGiftChance + largeGiftChance;

        if (totalChance <= 0f)
        {
            // Fallback jika semua probability 0
            return smallGiftPrefab ?? mediumGiftPrefab ?? largeGiftPrefab ?? giftPrefab;
        }

        float normalizedSmall = smallGiftChance / totalChance;
        float normalizedMedium = mediumGiftChance / totalChance;
        // normalizedLarge automatically = remaining probability

        // Random value 0-1
        float rand = Random.value;
        float cumulative = 0f;

        // Check Small
        cumulative += normalizedSmall;
        if (rand <= cumulative && smallGiftPrefab != null)
        {
            return smallGiftPrefab;
        }

        // Check Medium
        cumulative += normalizedMedium;
        if (rand <= cumulative && mediumGiftPrefab != null)
        {
            return mediumGiftPrefab;
        }

        // Large (or fallback)
        if (largeGiftPrefab != null)
        {
            return largeGiftPrefab;
        }

        // Fallback ke yang tersedia
        return smallGiftPrefab ?? mediumGiftPrefab ?? giftPrefab;
    }

    /// <summary>
    /// Hapus semua gift yang sudah di-spawn
    /// Dipanggil saat ChickenAI di-destroy
    /// </summary>
    private void CleanupAllGifts()
    {
        while (spawnedGifts.Count > 0)
        {
            GameObject gift = spawnedGifts.Dequeue();
            if (gift != null)
            {
                Destroy(gift);
            }
        }
        Debug.Log("ChickenAI: Semua gift dibersihkan.");
    }

    /// <summary>
    /// Method publik untuk manual spawn gift (optional, untuk testing)
    /// </summary>
    public void ManualSpawnGift()
    {
        SpawnGift();
    }

    /// <summary>
    /// Method untuk cek jumlah gift yang aktif saat ini
    /// </summary>
    public int GetActiveGiftCount()
    {
        // Bersihkan null references (jika ada gift yang dihapus dari luar)
        while (spawnedGifts.Count > 0 && spawnedGifts.Peek() == null)
        {
            spawnedGifts.Dequeue();
        }

        return spawnedGifts.Count;
    }

    #endregion

    #region Debug
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (terrainTilemap == null)
            return;

        // Draw current tile
        Vector3 tileWorldPos = GridToWorld(currentTile);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(tileWorldPos, Vector3.one * 0.5f);

        // Draw gift spawn position
        if (giftPrefab != null)
        {
            Vector3 giftSpawnPos = transform.position + giftSpawnOffset;
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(giftSpawnPos, 0.3f);
        }

        // Draw state info
        string giftInfo = giftPrefab != null ? $"\nGifts: {spawnedGifts.Count}/{maxGiftCount}" : "";
        UnityEditor.Handles.Label(
            transform.position + Vector3.up * 0.5f,
            $"State: {currentState}\nTile: {currentTile}{giftInfo}",
            new GUIStyle()
            {
                normal = new GUIStyleState() { textColor = Color.white },
                fontSize = 10
            }
        );
    }
#endif
    #endregion
}
