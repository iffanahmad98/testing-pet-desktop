using MagicalGarden.Farm;
using MagicalGarden.Manager;
using System;
using System.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;
using Random = UnityEngine.Random;

public class FrogAI : MonoBehaviour
{
    [SerializeField]
    private string _currentState;

    [Header("Animation")]
    public Animator animator;
    public Animator shadowAnimator;
    public SpriteRenderer spriteRenderer;
    public Transform shadowTransform;
    [Header("Movement Settings")]
    public float hopSpeed = 3f; // kecepatan lompat (lebih cepat dari walk)
    public float hopHeight = 0.5f; // tinggi lompatan
    public Tilemap terrainTilemap;
    [Tooltip("Pixel offset from tilemap edges to prevent stopping at edge")]
    public float edgeOffset = 0.2f;

    [Header("State Probabilities")]
    [Range(0f, 1f)] public float idleProbability = 0.7f; // katak lebih banyak diam
    [Range(0f, 1f)] public float hopProbability = 0.3f;  // jarang lompat

    [Header("Timing Settings - Hop Interval")]
    public float minHopInterval = 8f;  // interval lompat minimum
    public float maxHopInterval = 12f; // interval lompat maksimum

    [Header("Other Timing Settings")]
    public float minIdleTime = 2f;
    public float maxIdleTime = 5f;
    public float eatDuration = 3f;

    [Header("Lifetime Settings")]
    public float minLifetime = 30f;  // waktu hidup minimum (30 detik)
    public float maxLifetime = 50f;  // waktu hidup maksimum (50 detik)

    // Internal state
    private Vector2Int currentTile;
    private string currentState = "";
    private int lastDirection = -1; // -1 = left, 1 = right
    private Coroutine stateLoopCoroutine;
    private Coroutine lifetimeCoroutine;
    private float nextHopTime; // waktu untuk hop berikutnya
    private float currentLifetime;
    private Action<FrogAI> onDespawnCallback;

    void Start()
    {
        // Get components
        if (animator == null)
            animator = GetComponent<Animator>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        // Get tilemap from TileManager if not assigned (using main tilemap)
        if (terrainTilemap == null && TileManager.Instance != null)
        {
            terrainTilemap = TileManager.Instance.tilemapSoil;
        }
    }

    /// <summary>
    /// Spawn frog at position with specified lifetime
    /// Called by FrogSpawner
    /// </summary>
    public void Spawn(Vector3 position, float lifetime, Action<FrogAI> despawnCallback)
    {
        // Ensure components are assigned
        EnsureComponentsAssigned();

        transform.position = position;
        currentLifetime = lifetime;
        onDespawnCallback = despawnCallback;

        // Initialize current tile position
        Vector3Int cellPos = terrainTilemap.WorldToCell(position);
        currentTile = new Vector2Int(cellPos.x, cellPos.y);

        // Set initial hop time
        nextHopTime = Time.time + Random.Range(minHopInterval, maxHopInterval);

        // Start AI state loop
        if (stateLoopCoroutine != null)
            StopCoroutine(stateLoopCoroutine);
        stateLoopCoroutine = StartCoroutine(StateLoop());

        // Start lifetime countdown
        if (lifetimeCoroutine != null)
            StopCoroutine(lifetimeCoroutine);
        lifetimeCoroutine = StartCoroutine(LifetimeCountdown());
    }

    /// <summary>
    /// Ensure all required components are assigned
    /// </summary>
    private void EnsureComponentsAssigned()
    {
        // Get components
        if (animator == null)
            animator = GetComponent<Animator>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        // Get tilemap from TileManager if not assigned (using main tilemap)
        if (terrainTilemap == null && TileManager.Instance != null)
        {
            terrainTilemap = TileManager.Instance.tilemapSoil;
        }
    }

    /// <summary>
    /// Lifetime countdown - frog will despawn after this time
    /// </summary>
    private IEnumerator LifetimeCountdown()
    {
        yield return new WaitForSeconds(currentLifetime);
        Despawn();
    }

    /// <summary>
    /// Despawn this frog and notify the spawner
    /// </summary>
    private void Despawn()
    {
        // Stop all coroutines
        if (stateLoopCoroutine != null)
        {
            StopCoroutine(stateLoopCoroutine);
            stateLoopCoroutine = null;
        }

        if (lifetimeCoroutine != null)
        {
            StopCoroutine(lifetimeCoroutine);
            lifetimeCoroutine = null;
        }

        // Notify spawner
        onDespawnCallback?.Invoke(this);
    }

    void OnDestroy()
    {
        if (stateLoopCoroutine != null)
        {
            StopCoroutine(stateLoopCoroutine);
        }

        if (lifetimeCoroutine != null)
        {
            StopCoroutine(lifetimeCoroutine);
        }
    }

    #region State Machine

    private IEnumerator StateLoop()
    {
        while (true)
        {
            // Check if it's time to hop
            if (Time.time >= nextHopTime)
            {
                _currentState = "hop";
                yield return HopState();

                // Set next hop time (8-12 detik dari sekarang)
                nextHopTime = Time.time + Random.Range(minHopInterval, maxHopInterval);
            }
            else
            {
                // Choose random state based on probabilities (idle only, no hop here)
                string chosenState = GetRandomState();
                _currentState = chosenState;
                yield return HandleState(chosenState);
            }

            // Brief wait before checking next state
            yield return new WaitForSeconds(Random.Range(0.5f, 1.5f));
        }
    }

    private string GetRandomState()
    {
        // Hanya return idle karena hop diatur by timer
        return "idle";
    }

    private IEnumerator HandleState(string stateName)
    {
        switch (stateName)
        {
            case "idle":
                yield return IdleState();
                break;
            case "hop":
                yield return HopState();
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
            yield return new WaitForSeconds(Random.Range(minIdleTime, maxIdleTime));
        }
    }

    private IEnumerator HopState()
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

        bool hopped = false;

        // Try each direction until we find a valid path
        foreach (var dir in directions)
        {
            Vector2Int targetTile = currentTile + dir;

            // Check if path is clear
            if (!IsWalkableTile(targetTile))
            {
                continue;
            }

            // Start hop animation
            SetAnimation("hop");

            // Get world position of target tile
            Vector3 startPos = transform.position;
            Vector3 targetPos = GridToWorld(targetTile);

            // Flip sprite based on direction
            FlipByTarget(startPos, targetPos);

            // Hop to target with arc motion
            float duration = 1f / hopSpeed;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;

                // Linear horizontal movement
                Vector3 currentPos = Vector3.Lerp(startPos, targetPos, t);

                // Parabolic vertical arc
                float arc = hopHeight * Mathf.Sin(t * Mathf.PI);
                currentPos.y += arc;

                transform.position = currentPos;
                yield return null;
            }

            transform.position = targetPos;
            currentTile = targetTile;
            hopped = true;
            break;
        }

        // After hopping, check if we should eat
        if (hopped && IsPlantAtCurrentPosition())
        {
            yield return EatState();
        }
        else
        {
            // Brief idle after hopping
            SetAnimation("idle");
            yield return new WaitForSeconds(Random.Range(0.5f, 1.0f));
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
        animator.SetBool("hop", false);
        animator.SetBool("eat", false);

        shadowAnimator.SetBool ("idle", false);
        shadowAnimator.SetBool ("hop", false);
        // Set the requested animation
        switch (animName)
        {
            case "idle":
                animator.SetBool("idle", true);
                shadowAnimator.SetBool ("idle", true);
                break;
            case "hop":
                animator.SetBool("hop", true);
                shadowAnimator.SetBool ("hop", true);
                break;
            case "eat":
                animator.SetBool("eat", true);
                break;
            default:
                animator.SetBool("idle", true);
                shadowAnimator.SetBool ("idle", true);
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
            shadowTransform.localScale = new Vector3 (-1,1,1);
            lastDirection = 1;
        }
        else if (deltaX < -0.01f)
        {
            spriteRenderer.flipX = false; // facing left
            shadowTransform.localScale = new Vector3 (1,1,1);
            lastDirection = -1;
        }
    }

    #endregion

    #region Tile Helpers

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

    #region Debug
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (terrainTilemap == null)
            return;

        // Draw current tile
        Vector3 tileWorldPos = GridToWorld(currentTile);
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(tileWorldPos, Vector3.one * 0.5f);

        // Calculate remaining lifetime
        float remainingLife = currentLifetime - Time.time;
        if (lifetimeCoroutine != null)
        {
            // Show more accurate remaining time
            remainingLife = Mathf.Max(0, remainingLife);
        }

        // Draw state info
        UnityEditor.Handles.Label(
            transform.position + Vector3.up * 0.5f,
            $"State: {currentState}\nTile: {currentTile}\nNext Hop: {Mathf.Max(0, nextHopTime - Time.time):F1}s\nLifetime: {remainingLife:F1}s",
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
