using System.Collections.Generic;
using UnityEngine;

namespace MagicalGarden.Farm
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance;
        public CameraDragMove cameraRig;

        [Header("Cloud Settings")]
        public bool moveRightInstead = false;
        public List<Sprite> cloudDays;
        public List<Sprite> cloudEvenings;
        public GameObject cloudPrefab;
        public int cloudCount = 5;
        public Vector2 cloudSpeedRange = new Vector2(0.5f, 1.5f);
        public Vector2 cloudScaleRange = new Vector2(0.8f, 1.2f);

        [Header("Cloud Spawn/Destroy Area")]
        public Collider2D spawnArea;
        public Collider2D destroyArea;
        Collider2D currentSpawnArea;
        Collider2D currentDestroyArea;

        private List<GameObject> cloudInstances = new List<GameObject>();
        private Dictionary<GameObject, float> cloudSpeeds = new Dictionary<GameObject, float>();
        
        // Daftar untuk melacak posisi awan yang aktif, untuk pengecekan overlap
        private List<Vector2> activeCloudXRanges = new List<Vector2>();
        void Awake()
        {
            Instance = this;
        }

        void Start()
        {
            SpawnClouds();
            currentSpawnArea = moveRightInstead ? destroyArea : spawnArea;
            currentDestroyArea = moveRightInstead ? spawnArea : destroyArea;
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.P)) // Ganti dengan shortcut yang kamu mau
            {
                TogglePause();
            }
            UpdateCloudMovement();
        }

        public void SaveGame()
        {
            PlantManager.Instance.SaveToJson();
        }

        public void LoadGame()
        {
            PlantManager.Instance.LoadFromJson();
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

            List<Vector2> usedXRanges = new List<Vector2>();
            int maxAttemptsPerCloud = 20; // Ubah nama variabel agar lebih jelas
            float padding = 0.2f;

            for (int i = 0; i < cloudCount; i++)
            {
                Sprite chosenSprite = cloudDays.Count > 0
                    ? cloudDays[Random.Range(0, cloudDays.Count)]
                    : null;

                if (chosenSprite == null)
                {
                    Debug.LogWarning("❌ Sprite cloud kosong");
                    continue;
                }

                // 1. TENTUKAN SKALA DAN LEBAR AWAN TERLEBIH DAHULU
                float finalScale = Random.Range(cloudScaleRange.x, cloudScaleRange.y);
                float spriteWidthUnit = chosenSprite.bounds.size.x;
                float worldWidth = spriteWidthUnit * finalScale + padding; // Lebar awan yang sebenarnya di dunia game

                Vector3 spawnPos = Vector3.zero;
                bool positionFound = false;

                // 2. SEKARANG, CARI POSISI UNTUK AWAN DENGAN UKURAN YANG SUDAH PASTI
                for (int attempt = 0; attempt < maxAttemptsPerCloud; attempt++)
                {
                    float randomX = Random.Range(destroyArea.bounds.min.x, spawnArea.bounds.max.x);
                    float randomY = Random.Range(spawnArea.bounds.min.y, spawnArea.bounds.max.y);

                    float cloudMinX = randomX - worldWidth / 2f;
                    float cloudMaxX = randomX + worldWidth / 2f;

                    bool overlaps = false;
                    foreach (var range in usedXRanges)
                    {
                        // Pengecekan overlap: jika rentang baru tumpang tindih dengan rentang yang sudah ada
                        if (cloudMaxX > range.x && cloudMinX < range.y)
                        {
                            overlaps = true;
                            break;
                        }
                    }

                    if (!overlaps)
                    {
                        spawnPos = new Vector3(randomX, randomY, 0f);
                        usedXRanges.Add(new Vector2(cloudMinX, cloudMaxX)); // Simpan rentang yang sudah dipakai
                        positionFound = true;
                        break; // Posisi ditemukan, keluar dari loop percobaan
                    }
                }

                if (!positionFound)
                {
                    // Jika setelah banyak percobaan tetap tidak menemukan tempat,
                    // lebih baik jangan dipaksa spawn, atau log sebuah peringatan.
                    Debug.LogWarning($"Tidak dapat menemukan posisi untuk awan ke-{i + 1} setelah {maxAttemptsPerCloud} percobaan. Mungkin area spawn terlalu padat.");
                    continue; // Lanjut ke awan berikutnya
                }

                // 3. BUAT AWAN DENGAN POSISI DAN SKALA YANG SUDAH VALID
                GameObject cloud = Instantiate(cloudPrefab, spawnPos, Quaternion.identity);
                cloud.transform.localScale = Vector3.one * finalScale;

                var sr = cloud.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.sprite = chosenSprite;
                }

                cloudInstances.Add(cloud);

                float speed = Random.Range(cloudSpeedRange.x, cloudSpeedRange.y);
                cloudSpeeds[cloud] = speed;
                Debug.Log($"🌥️ Total awan berhasil di-spawn: {cloudInstances.Count} dari {cloudCount}");
            }
        }


        void UpdateCloudMovement()
        {
            foreach (var cloud in cloudInstances)
            {
                float speed = cloudSpeeds.ContainsKey(cloud) ? cloudSpeeds[cloud] : 1f;
                Vector3 direction = moveRightInstead ? Vector3.right : Vector3.left;
                cloud.transform.position += direction * speed * Time.deltaTime;

                bool outOfBounds = moveRightInstead
                    ? cloud.transform.position.x > currentDestroyArea.bounds.max.x
                    : cloud.transform.position.x < currentDestroyArea.bounds.min.x;

                if (outOfBounds)
                {
                    float resetX = moveRightInstead
                        ? currentSpawnArea.bounds.min.x + Random.Range(0f, 1f)
                        : currentSpawnArea.bounds.max.x + Random.Range(0f, 1f);

                    float resetY = Random.Range(currentSpawnArea.bounds.min.y, currentSpawnArea.bounds.max.y);
                    cloud.transform.position = new Vector3(resetX, resetY, 0f);

                    float randomScale = Random.Range(cloudScaleRange.x, cloudScaleRange.y);
                    cloud.transform.localScale = Vector3.one * randomScale;

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
