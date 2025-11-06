using MagicalGarden.Manager;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrogSpawner : MonoBehaviour
{
    [Header("Prefab & Spawn Points")]
    public GameObject frogPrefab;
    public List<Transform> spawnPoints = new List<Transform>();  // titik-titik spawn kodok

    [Header("Spawn Settings")]
    public int baseMaxConcurrent = 5;                           // jumlah maksimum kodok concurrent di siang hari
    public Vector2 spawnIntervalRange = new Vector2(8f, 15f);   // jeda antar spawn
    public Vector2 lifeTimeRange = new Vector2(30f, 50f);       // durasi hidup kodok (30-50 detik)
    public float spawnRadiusNearPoint = 0.5f;                   // radius sekitar spawn point

    [Header("Nighttime Settings")]
    [Tooltip("Jam mulai malam (contoh: 18 untuk jam 6 sore)")]
    public int nightStartHour = 18;                             // jam mulai malam (18:00)
    [Tooltip("Jam selesai malam (contoh: 6 untuk jam 6 pagi)")]
    public int nightEndHour = 6;                                // jam selesai malam (06:00)
    [Range(0f, 1f)]
    [Tooltip("Persentase peningkatan spawn di malam hari (0.3 = 30%)")]
    public float nighttimeSpawnBonus = 0.3f;                    // bonus 30% di malam hari

    [Header("Debug Info")]
    [SerializeField] private bool isNighttime = false;
    [SerializeField] private int currentMaxConcurrent = 5;
    [SerializeField] private int aliveCount = 0;

    // Object pool
    private readonly Queue<FrogAI> pool = new Queue<FrogAI>();

    void Start()
    {
        if (frogPrefab == null)
        {
            Debug.LogWarning("FrogSpawner: Frog prefab belum di-assign!");
            enabled = false;
            return;
        }

        if (spawnPoints == null || spawnPoints.Count == 0)
        {
            Debug.LogWarning("FrogSpawner: Tidak ada spawn points! Tambahkan spawn points untuk spawning kodok.");
            enabled = false;
            return;
        }

        StartCoroutine(SpawnLoop());
    }

    void Update()
    {
        // Update nighttime status
        UpdateNighttimeStatus();
    }

    /// <summary>
    /// Update status malam hari dan hitung max concurrent berdasarkan waktu
    /// </summary>
    private void UpdateNighttimeStatus()
    {
        if (TimeManager.Instance == null)
        {
            isNighttime = false;
            currentMaxConcurrent = baseMaxConcurrent;
            return;
        }

        int currentHour = TimeManager.Instance.currentTime.Hour;

        // Check if nighttime (18:00 - 06:00)
        if (nightStartHour > nightEndHour)
        {
            // Night crosses midnight (e.g., 18:00 to 06:00)
            isNighttime = currentHour >= nightStartHour || currentHour < nightEndHour;
        }
        else
        {
            // Night doesn't cross midnight (e.g., 20:00 to 04:00)
            isNighttime = currentHour >= nightStartHour && currentHour < nightEndHour;
        }

        // Calculate max concurrent based on time of day
        if (isNighttime)
        {
            currentMaxConcurrent = Mathf.RoundToInt(baseMaxConcurrent * (1f + nighttimeSpawnBonus));
        }
        else
        {
            currentMaxConcurrent = baseMaxConcurrent;
        }
    }

    /// <summary>
    /// Spawn loop - continuously spawn frogs at intervals
    /// </summary>
    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            float wait = Random.Range(spawnIntervalRange.x, spawnIntervalRange.y);
            yield return new WaitForSeconds(wait);

            // Check if we can spawn more frogs
            if (aliveCount >= currentMaxConcurrent || spawnPoints.Count == 0)
            {
                yield return null;
                continue;
            }

            // Choose random spawn point
            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Count)];

            // Add random offset around spawn point
            Vector2 offset = Random.insideUnitCircle * spawnRadiusNearPoint;
            Vector3 spawnPos = spawnPoint.position + new Vector3(offset.x, offset.y, 0f);

            // Get frog from pool and spawn
            FrogAI frog = GetFrogFromPool();
            float lifetime = Random.Range(lifeTimeRange.x, lifeTimeRange.y);

            frog.gameObject.SetActive(true);
            frog.Spawn(spawnPos, lifetime, OnFrogDespawn);

            aliveCount++;

            yield return null;
        }
    }

    /// <summary>
    /// Get a frog from the pool or create a new one
    /// </summary>
    private FrogAI GetFrogFromPool()
    {
        if (pool.Count > 0)
        {
            return pool.Dequeue();
        }

        // Create new frog
        GameObject frogObj = Instantiate(frogPrefab);
        FrogAI frog = frogObj.GetComponent<FrogAI>();

        if (frog == null)
        {
            Debug.LogError("FrogSpawner: Frog prefab tidak memiliki FrogAI component!");
            frog = frogObj.AddComponent<FrogAI>();
        }

        return frog;
    }

    /// <summary>
    /// Called when a frog despawns (lifetime expired)
    /// </summary>
    private void OnFrogDespawn(FrogAI frog)
    {
        aliveCount = Mathf.Max(0, aliveCount - 1);
        frog.gameObject.SetActive(false);
        pool.Enqueue(frog);
    }

    #region Debug
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (spawnPoints == null || spawnPoints.Count == 0)
            return;

        // Draw spawn points
        foreach (Transform spawnPoint in spawnPoints)
        {
            if (spawnPoint == null)
                continue;

            // Draw spawn point
            Gizmos.color = isNighttime ? Color.cyan : Color.yellow;
            Gizmos.DrawWireSphere(spawnPoint.position, spawnRadiusNearPoint);

            // Draw spawn point label
            UnityEditor.Handles.Label(
                spawnPoint.position + Vector3.up * 0.8f,
                $"Spawn Point\n{(isNighttime ? "NIGHT" : "DAY")}",
                new GUIStyle()
                {
                    normal = new GUIStyleState() { textColor = isNighttime ? Color.cyan : Color.yellow },
                    fontSize = 10,
                    alignment = UnityEngine.TextAnchor.MiddleCenter
                }
            );
        }

        // Draw spawner info at center
        if (spawnPoints.Count > 0)
        {
            Vector3 center = Vector3.zero;
            foreach (Transform sp in spawnPoints)
            {
                if (sp != null)
                    center += sp.position;
            }
            center /= spawnPoints.Count;

            UnityEditor.Handles.Label(
                center + Vector3.up * 2f,
                $"Frog Spawner\nAlive: {aliveCount}/{currentMaxConcurrent}\n{(isNighttime ? $"NIGHT (+{nighttimeSpawnBonus * 100:F0}%)" : "DAY")}",
                new GUIStyle()
                {
                    normal = new GUIStyleState() { textColor = Color.green },
                    fontSize = 12,
                    fontStyle = FontStyle.Bold,
                    alignment = UnityEngine.TextAnchor.MiddleCenter
                }
            );
        }
    }
#endif
    #endregion
}
