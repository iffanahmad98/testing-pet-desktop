using System.Collections.Generic;
using UnityEngine;

namespace MagicalGarden.Farm
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance;
        public CameraDragMove cameraRig;

        [Header("Cloud Settings")]
        public List<Sprite> cloudDays;
        public List<Sprite> cloudEvenings;
        public GameObject cloudPrefab;
        public int cloudCount = 5;
        public Vector2 cloudSpeedRange = new Vector2(0.5f, 1.5f); // ✅ per-cloud speed
        public Vector2 cloudScaleRange = new Vector2(0.8f, 1.2f);
        public float minSpawnDistanceX = 2f;

        [Header("Cloud Spawn/Destroy Area")]
        public Collider2D spawnArea;
        public Collider2D destroyArea;

        private List<GameObject> cloudInstances = new List<GameObject>();
        private Dictionary<GameObject, float> cloudSpeeds = new Dictionary<GameObject, float>(); // ✅ menyimpan kecepatan per cloud

        void Awake()
        {
            Instance = this;
        }

        void Start()
        {
            SpawnClouds();
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.P)) // Ganti dengan shortcut yang kamu mau
            {
                TogglePause();
            }
            UpdateCloudMovement();
        }

        public void DisableCameraRig()
        {
            cameraRig.canDrag = false;
            cameraRig.canZoom = false;
        }

        public void EnableCameraRig()
        {
            cameraRig.canDrag = true;
            cameraRig.canZoom = true;
        }

        public bool HasEnoughPetsInInventory(int requiredCount)
        {
            return true;
        }

        void SpawnClouds()
        {
            if (spawnArea == null || destroyArea == null)
            {
                Debug.LogWarning("Spawn/Destroy area collider belum diassign!");
                return;
            }

            List<float> usedXPositions = new List<float>();
            int maxAttempts = 10;

            for (int i = 0; i < cloudCount; i++)
            {
                Vector3 spawnPos = Vector3.zero;
                bool positionFound = false;

                for (int attempt = 0; attempt < maxAttempts; attempt++)
                {
                    float randomX = Random.Range(destroyArea.bounds.min.x, spawnArea.bounds.max.x);
                    float randomY = Random.Range(spawnArea.bounds.min.y, spawnArea.bounds.max.y);

                    bool tooClose = false;
                    foreach (float usedX in usedXPositions)
                    {
                        if (Mathf.Abs(usedX - randomX) < minSpawnDistanceX)
                        {
                            tooClose = true;
                            break;
                        }
                    }

                    if (!tooClose)
                    {
                        spawnPos = new Vector3(randomX, randomY, 0f);
                        usedXPositions.Add(randomX);
                        positionFound = true;
                        break;
                    }
                }

                if (!positionFound)
                {
                    float fallbackX = Random.Range(destroyArea.bounds.min.x, spawnArea.bounds.max.x);
                    float fallbackY = Random.Range(spawnArea.bounds.min.y, spawnArea.bounds.max.y);
                    spawnPos = new Vector3(fallbackX, fallbackY, 0f);
                }

                var cloud = Instantiate(cloudPrefab, spawnPos, Quaternion.identity);
                cloudInstances.Add(cloud);

                // ✅ acak kecepatan dan simpan
                float randomSpeed = Random.Range(cloudSpeedRange.x, cloudSpeedRange.y);
                cloudSpeeds[cloud] = randomSpeed;

                // ✅ acak ukuran
                float randomScale = Random.Range(cloudScaleRange.x, cloudScaleRange.y);
                cloud.transform.localScale = Vector3.one * randomScale;

                var spriteRenderer = cloud.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null && cloudDays.Count > 0)
                {
                    spriteRenderer.sprite = cloudDays[Random.Range(0, cloudDays.Count)];
                }
            }
        }

        void UpdateCloudMovement()
        {
            foreach (var cloud in cloudInstances)
            {
                float speed = cloudSpeeds.ContainsKey(cloud) ? cloudSpeeds[cloud] : 1f;
                cloud.transform.position += Vector3.left * speed * Time.deltaTime;

                if (cloud.transform.position.x < destroyArea.bounds.min.x)
                {
                    float resetX = spawnArea.bounds.max.x + Random.Range(0f, 1f);
                    float resetY = Random.Range(spawnArea.bounds.min.y, spawnArea.bounds.max.y);
                    cloud.transform.position = new Vector3(resetX, resetY, 0f);

                    float randomScale = Random.Range(cloudScaleRange.x, cloudScaleRange.y);
                    cloud.transform.localScale = Vector3.one * randomScale;

                    // ✅ reset juga kecepatannya
                    float newSpeed = Random.Range(cloudSpeedRange.x, cloudSpeedRange.y);
                    cloudSpeeds[cloud] = newSpeed;
                }
            }
        }

        public void SetCloudTime(string timeOfDay)
        {
            List<Sprite> targetList = null;

            if (timeOfDay == "day")
                targetList = cloudDays;
            else if (timeOfDay == "evening")
                targetList = cloudEvenings;

            foreach (var cloud in cloudInstances)
            {
                var spriteRenderer = cloud.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null)
                {
                    if (targetList != null && targetList.Count > 0)
                    {
                        spriteRenderer.sprite = targetList[Random.Range(0, targetList.Count)];
                    }
                    else
                    {
                        spriteRenderer.sprite = null; // ⛔ kosongkan sprite kalau waktu tidak valid
                    }
                }
            }
        }

        private bool isPaused = false;

        void TogglePause()
        {
            isPaused = !isPaused;
            Time.timeScale = isPaused ? 0f : 1f;

            Debug.Log(isPaused ? "Game Paused" : "Game Resumed");
            // Bisa tambahkan buka panel pause UI di sini juga
        }

#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            if (spawnArea != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireCube(spawnArea.bounds.center, spawnArea.bounds.size);
            }

            if (destroyArea != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireCube(destroyArea.bounds.center, destroyArea.bounds.size);
            }
        }
#endif
    }
}
