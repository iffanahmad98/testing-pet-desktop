using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeeSpawner : MonoBehaviour
{
    [Header("Burst (Cluster) Settings")]
    [Range(0f, 1f)] public float burstChance = 0.25f;          // peluang muncul cluster
    public Vector2Int burstCountRange = new Vector2Int(2, 3);  // lebah biasanya lebih sedikit dalam cluster

    [Header("Refs")]
    public GameObject beePrefab;
    public List<Transform> flowers;              // kosongkan = cari by Tag "Flower" saat Start

    [Header("Spawn Settings")]
    public int maxConcurrent = 8;                               // lebah biasanya lebih sedikit dari kupu-kupu
    public Vector2 spawnIntervalRange = new Vector2(5f, 10f);   // jeda antar spawn
    public Vector2 lifeTimeRange      = new Vector2(6f, 14f);   // durasi hidup lebah
    public float   spawnRadiusNearFlower = 1.0f;                // radius sekitar bunga

    [Header("Buzz Settings")]
    public float moveSpeed = 0.8f;         // lebah sedikit lebih cepat dari kupu-kupu
    public float buzzAmp = 0.4f;           // amplitudo buzz
    public float buzzFreq = 2.2f;          // frekuensi buzz (lebih cepat dari butterfly)

    // pool
    readonly Queue<BeeAgent> pool = new Queue<BeeAgent>();
    int aliveCount;

    void Start()
    {
        if (beePrefab == null) { Debug.LogWarning("Bee prefab belum di-assign."); enabled = false; return; }

        if (flowers == null || flowers.Count == 0)
        {
            var found = GameObject.FindGameObjectsWithTag("Flower");
            flowers = new List<Transform>();
            foreach (var f in found) flowers.Add(f.transform);
        }

        if (flowers.Count == 0)
            Debug.LogWarning("Tidak ditemukan bunga (Tag: Flower). Tambahkan manual ke list 'flowers' atau tag object bunga.");

        StartCoroutine(SpawnLoop());
    }

    IEnumerator SpawnLoop()
    {
        while (true)
        {
            float wait = Random.Range(spawnIntervalRange.x, spawnIntervalRange.y);
            yield return new WaitForSeconds(wait);

            if (aliveCount >= maxConcurrent || flowers.Count == 0)
            {
                yield return null;
                continue;
            }

            // pilih bunga acak & posisi sekitar
            Transform flower = flowers[Random.Range(0, flowers.Count)];

            // tentukan jumlah lebah (cluster atau single)
            int count = 1;
            if (Random.value < burstChance)
                count = Random.Range(burstCountRange.x, burstCountRange.y + 1);

            for (int i = 0; i < count; i++)
            {
                if (aliveCount >= maxConcurrent) break;

                Vector2 offset = Random.insideUnitCircle * spawnRadiusNearFlower;
                Vector3 pos = flower.position + new Vector3(offset.x, offset.y, 0f);

                BeeAgent agent = GetAgent();
                float life = Random.Range(lifeTimeRange.x, lifeTimeRange.y);

                agent.gameObject.SetActive(true);
                agent.Spawn(pos, life, moveSpeed, buzzAmp, buzzFreq, OnAgentDespawn);
                aliveCount++;

                yield return null; // jeda seframe antar lebah dalam 1 cluster
            }
        }
    }

    BeeAgent GetAgent()
    {
        if (pool.Count > 0) return pool.Dequeue();
        var go = Instantiate(beePrefab);
        var agent = go.GetComponent<BeeAgent>();
        if (!agent) agent = go.AddComponent<BeeAgent>();
        return agent;
    }

    void OnAgentDespawn(BeeAgent agent)
    {
        aliveCount = Mathf.Max(0, aliveCount - 1);
        agent.gameObject.SetActive(false);
        pool.Enqueue(agent);
    }
}
