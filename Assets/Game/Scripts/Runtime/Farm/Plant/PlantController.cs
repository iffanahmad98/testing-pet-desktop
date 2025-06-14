using UnityEngine;
using System;
using UnityEngine.Tilemaps;
using MagicalGarden.Inventory;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MagicalGarden.Farm
{
    public class PlantController : MonoBehaviour
    {
        public SeedBase seed;
        private float updateTimer = 0f;
        public ItemData fertilizer;

        void Start()
        {
            if (seed == null)
            {
                Debug.LogWarning("No seed assigned!");
                enabled = false;
            }
        }

        void Update()
        {
            updateTimer += Time.deltaTime;
            if (updateTimer >= PlantManager.Instance.updateIntervalSeconds)
            {
                // float deltaHours = (float)(DateTime.Now - seed.lastUpdateTime).TotalHours;
                DateTime now = PlantManager.Instance.simulatedNow;
                float deltaHours = (float)(now - seed.lastUpdateTime).TotalHours;
                if (deltaHours <= 0f)
                {
                    updateTimer = 0f;
                    return;
                }
                seed.Update(deltaHours, fertilizer?.boost ?? 0);
                seed.lastUpdateTime = now;
                updateTimer = 0f;

                // Example: auto destroy dead plant
                if (seed.status == PlantStatus.Mati)
                {
                    // Destroy(gameObject);
                }
            }
        }
        
        public ItemData Fertilize
        {
            get => fertilizer;
            set => fertilizer = value;
        }

        public string GetPlantStatusText()
        {
            if (TileManager.Instance == null || TileManager.Instance.tilemapSoil == null || seed == null)
                return "";

            DateTime now = PlantManager.Instance != null ? PlantManager.Instance.simulatedNow : DateTime.Now;

            bool neverWatered = seed.lastWateredTime == default(DateTime);
            DateTime referenceTime = neverWatered ? seed.plantedTime : seed.lastWateredTime;
            double hoursSinceWatered = (now - referenceTime).TotalHours;

            float fertilizerBoost = Fertilize?.boost ?? 0f;
            float multiplier = 1f + fertilizerBoost / 100f;

            string debugText =
                $"Stage: {seed.stage + 1}\n" +
                $"Status: {seed.status}\n" +
                $"TimeInStage: {TimeSpan.FromHours(seed.timeInStage):hh\\:mm\\:ss}\n" +
                $"- Boost: {multiplier:0.0}x";

            // Estimasi ke stage berikutnya
            var growthRequirements = seed.GetGrowthRequirements();
            if (seed.stage < growthRequirements.Count)
            {
                float target = growthRequirements[seed.stage].requiredHours;
                float remainingHours = (target - seed.timeInStage) / multiplier;
                if (remainingHours < 0) remainingHours = 0;
                debugText += $"\n- Estimasi: {TimeSpan.FromHours(remainingHours):hh\\:mm\\:ss} ke stage berikutnya";
            }

            if (seed.status == PlantStatus.Mati || hoursSinceWatered >= 48)
            {
                debugText += "\n- Tanaman mati";
            }
            else if (seed.status == PlantStatus.Layu || hoursSinceWatered >= 24 || neverWatered)
            {
                double waktuMenujuMati = 48 - hoursSinceWatered;
                if (waktuMenujuMati < 0) waktuMenujuMati = 0;
                debugText += $"\n- Layu → Mati dalam: {waktuMenujuMati:F1}h";

                if (neverWatered)
                    debugText += "\n- Belum pernah disiram";
            }
            else if (hoursSinceWatered >= 1)
            {
                debugText += $"\n- Tidak disiram {hoursSinceWatered:F1}h";
            }
            else
            {
                debugText += "\n- Disiram < 1 jam lalu (aktif tumbuh)";
            }

            return debugText;
        }

        #if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (TileManager.Instance == null || TileManager.Instance.tilemapSoil == null || seed == null)
                return;

            DateTime now = PlantManager.Instance != null ? PlantManager.Instance.simulatedNow : DateTime.Now;

            bool neverWatered = seed.lastWateredTime == default(DateTime);
            DateTime referenceTime = neverWatered ? seed.plantedTime : seed.lastWateredTime;
            double hoursSinceWatered = (now - referenceTime).TotalHours;

            Vector3 worldPos = TileManager.Instance.tilemapSoil.CellToWorld(seed.cellPosition) + new Vector3(0f, 1f, 0f);

            string debugText =
                $"Stage: {seed.stage + 1}\n" +
                $"Status: {seed.status}\n" +
                $"TimeInStage: {seed.timeInStage:F1}h";

            if (seed.status == PlantStatus.Mati || hoursSinceWatered >= 48)
            {
                debugText += "\n❌ Tanaman mati";
            }
            else if (seed.status == PlantStatus.Layu || hoursSinceWatered >= 24 || neverWatered)
            {
                double waktuMenujuMati = 48 - hoursSinceWatered;
                if (waktuMenujuMati < 0) waktuMenujuMati = 0;
                debugText += $"\n⚠️ Layu → Mati dalam: {waktuMenujuMati:F1}h";

                // Kalau belum pernah disiram, tambahkan catatan khusus
                if (neverWatered)
                    debugText += "\n⚠️ Belum pernah disiram";
            }
            else if (hoursSinceWatered >= 1)
            {
                debugText += $"\n⚠️ Tidak disiram {hoursSinceWatered:F1}h";
            }
            else
            {
                debugText += "\n✅ Disiram < 1 jam lalu (aktif tumbuh)";
            }

            GUIStyle style = new GUIStyle(UnityEditor.EditorStyles.boldLabel)
            {
                normal = new GUIStyleState { textColor = Color.black },
                fontSize = 12
            };

            if (seed.IsReadyToHarvest())
            {
                debugText = "🌾 Ready to Harvest!";
                style.fontSize = 16;
                style.normal.textColor = Color.yellow;
            }
            else if (seed.status == PlantStatus.Mati || hoursSinceWatered >= 48)
            {
                style.normal.textColor = Color.red;
            }
            else if (seed.status == PlantStatus.Layu || hoursSinceWatered >= 24 || neverWatered)
            {
                style.normal.textColor = new Color(0.8f, 0.5f, 0f); // orange
            }

            UnityEditor.Handles.Label(worldPos, debugText, style);
        }
        #endif
    }
}