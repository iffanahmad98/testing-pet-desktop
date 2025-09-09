using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButterflySpawner : MonoBehaviour
{
    [Header("Burst (Cluster) Settings")]
    [Range(0f, 1f)] public float burstChance = 0.3f;          // peluang muncul cluster
    public Vector2Int burstCountRange = new Vector2Int(2, 4);

    [Header("Refs")]
    public GameObject butterflyPrefab;
    public List<Transform> flowers;              // kosongkan = cari by Tag "Flower" saat Start

    [Header("Spawn Settings")]
    public int maxConcurrent = 10;
    public Vector2 spawnIntervalRange = new Vector2(6f, 12f);   // jeda antar spawn
    public Vector2 lifeTimeRange      = new Vector2(8f, 16f);   // durasi hidup kupu
    public float   spawnRadiusNearFlower = 0.8f;                // radius sekitar bunga

    [Header("Flutter Settings")]
    public float moveSpeed = 0.7f;         // kecepatan dasar
    public float flutterAmp = 0.35f;       // amplitudo flutter
    public float flutterFreq = 1.8f;       // frekuensi flutter

    // pool
    readonly Queue<ButterflyAgent> pool = new Queue<ButterflyAgent>();
    int aliveCount;

    void Start()
    {
        if (butterflyPrefab == null) { Debug.LogWarning("Butterfly prefab belum di-assign."); enabled = false; return; }

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

            // tentukan jumlah kupu (cluster atau single)
            int count = 1;
            if (Random.value < burstChance)
                count = Random.Range(burstCountRange.x, burstCountRange.y + 1);

            for (int i = 0; i < count; i++)
            {
                if (aliveCount >= maxConcurrent) break;

                Vector2 offset = Random.insideUnitCircle * spawnRadiusNearFlower;
                Vector3 pos = flower.position + new Vector3(offset.x, offset.y, 0f);

                ButterflyAgent agent = GetAgent();
                float life = Random.Range(lifeTimeRange.x, lifeTimeRange.y);

                agent.gameObject.SetActive(true);
                agent.Spawn(pos, life, moveSpeed, flutterAmp, flutterFreq, OnAgentDespawn);
                aliveCount++;

                yield return null; // jeda seframe antar kupu dalam 1 cluster
            }
        }
    }

    ButterflyAgent GetAgent()
    {
        if (pool.Count > 0) return pool.Dequeue();
        var go = Instantiate(butterflyPrefab);
        var agent = go.GetComponent<ButterflyAgent>();
        if (!agent) agent = go.AddComponent<ButterflyAgent>();
        return agent;
    }

    void OnAgentDespawn(ButterflyAgent agent)
    {
        aliveCount = Mathf.Max(0, aliveCount - 1);
        agent.gameObject.SetActive(false);
        pool.Enqueue(agent);
    }
}