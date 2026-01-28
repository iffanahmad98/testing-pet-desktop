using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CloudShadeHandler : MonoBehaviour
{
    [SerializeField] Transform cloudShadeSpawnArea;
    public CloudShadeConfig[] cloudShadeConfigs;
    [System.Serializable]
    public class CloudShadeConfig
    {
        public int maxSpawn;
        public float minCooldownSpawn;
        public float maxCooldownSpawn;
        public int chanceSpawn;
        public GameObject cloudPrefab;
        public Vector3 minCloudSize;
        public Vector3 maxCloudSize;

        [HideInInspector]
        public int currentSpawn; // runtime counter
    }
    
    void Start () {
        OnStart ();
    }

    public void OnStart()
    {
        foreach (CloudShadeConfig config in cloudShadeConfigs)
        {
            StartCoroutine(SpawnRoutine(config));
        }
    }

    IEnumerator SpawnRoutine(CloudShadeConfig config)
    {
        while (true)
        {
            if (config.currentSpawn >= config.maxSpawn)
            {
                yield return null;
                continue;
            }

            float cooldown = Random.Range(
                config.minCooldownSpawn,
                config.maxCooldownSpawn
            );

            yield return new WaitForSeconds(cooldown);

            int roll = Random.Range(0, 100);
            if (roll > config.chanceSpawn)
                continue;

            SpawnCloud(config);
        }
    }


    void SpawnCloud(CloudShadeConfig config)
    {
        Vector3 scale = new Vector3(
            Random.Range(config.minCloudSize.x, config.maxCloudSize.x),
            Random.Range(config.minCloudSize.y, config.maxCloudSize.y),
            Random.Range(config.minCloudSize.z, config.maxCloudSize.z)
        );

        Vector3 spawnPos = GetRandomPositionInArea();

        GameObject cloud = Instantiate(config.cloudPrefab, spawnPos, Quaternion.identity, transform);
        cloud.transform.localScale = scale;
        cloud.SetActive (true);
        cloud.GetComponent <CloudShade> ().AddDestroyedEvent (DestroyEvent);
        config.currentSpawn++;
    }

    Vector3 GetRandomPositionInArea()
    {
        Vector3 center = cloudShadeSpawnArea.position;
        Vector3 size = cloudShadeSpawnArea.localScale;

        return new Vector3(
            Random.Range(center.x - size.x / 2f, center.x + size.x / 2f),
            Random.Range(center.y - size.y / 2f, center.y + size.y / 2f),
            Random.Range(center.z - size.z / 2f, center.z + size.z / 2f)
        );
    }

    public void DestroyEvent (int element) {
        CloudShadeConfig cloudShadeConfig = cloudShadeConfigs[element];
        cloudShadeConfig.currentSpawn --;
    }
}
