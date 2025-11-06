using UnityEngine;
using MagicalGarden.Farm;

namespace MagicalGarden.Gift
{
    /// <summary>
    /// Debug helper untuk troubleshoot gift system
    /// Attach ke GameObject di scene untuk monitoring
    /// </summary>
    public class GiftSystemDebugger : MonoBehaviour
    {
        [Header("Debug Options")]
        [Tooltip("Show debug info di console saat Start?")]
        public bool logOnStart = true;

        [Tooltip("Show periodic updates?")]
        public bool periodicLogging = false;
        public float logInterval = 5f;

        private float lastLogTime;

        void Start()
        {
            if (logOnStart)
            {
                LogSystemState();
            }
        }

        void Update()
        {
            if (periodicLogging && Time.time - lastLogTime >= logInterval)
            {
                LogSystemState();
                lastLogTime = Time.time;
            }
        }

        /// <summary>
        /// Log current state of gift system dependencies
        /// </summary>
        public void LogSystemState()
        {
            Debug.Log("========== GIFT SYSTEM DEBUG ==========");

            // Check Farm CoinManager (Instance-based)
            if (MagicalGarden.Farm.CoinManager.Instance != null)
            {
                Debug.Log($"<color=green>✓ Farm.CoinManager.Instance: OK</color>");
                Debug.Log($"  Current Coins: {MagicalGarden.Farm.CoinManager.Instance.coins}");
            }
            else
            {
                Debug.LogError($"<color=red>✗ Farm.CoinManager.Instance: NULL!</color>");
                Debug.LogError($"  <color=red>COIN REWARDS TIDAK AKAN BEKERJA!</color>");
                Debug.LogError($"  <color=yellow>Solution: Pastikan ada GameObject dengan CoinManager component di scene FarmGame</color>");
            }

            // Check InventoryManager
            if (MagicalGarden.Inventory.InventoryManager.Instance != null)
            {
                Debug.Log($"<color=green>✓ InventoryManager: OK</color>");
                Debug.Log($"  Total Items: {MagicalGarden.Inventory.InventoryManager.Instance.items.Count}");
            }
            else
            {
                Debug.LogWarning($"<color=orange>! InventoryManager: NULL</color>");
                Debug.LogWarning($"  <color=yellow>Item rewards tidak akan bekerja!</color>");
            }

            // Check CoinDisplayUI
            var coinDisplayUI = FindObjectOfType<CoinDisplayUI>();
            if (coinDisplayUI != null)
            {
                Debug.Log($"<color=green>✓ CoinDisplayUI: Found</color>");
            }
            else
            {
                Debug.LogWarning($"<color=orange>! CoinDisplayUI: Not found in scene</color>");
            }

            Debug.Log("=======================================");
        }

        /// <summary>
        /// Test add coins manually
        /// </summary>
        [ContextMenu("Test Add 100 Coins")]
        public void TestAddCoins()
        {
            if (MagicalGarden.Farm.CoinManager.Instance == null)
            {
                Debug.LogError("<color=red>[TEST] FAILED: Farm.CoinManager.Instance is NULL!</color>");
                return;
            }

            Debug.Log("<color=cyan>[TEST] Adding 100 coins...</color>");
            int before = MagicalGarden.Farm.CoinManager.Instance.coins;
            MagicalGarden.Farm.CoinManager.Instance.AddCoins(100);
            int after = MagicalGarden.Farm.CoinManager.Instance.coins;

            if (before == after)
            {
                Debug.LogError($"<color=red>[TEST] FAILED: Coins tidak bertambah! ({before} → {after})</color>");
            }
            else
            {
                Debug.Log($"<color=green>[TEST] SUCCESS: Coins bertambah! ({before} → {after})</color>");
            }
        }

        /// <summary>
        /// Manual trigger untuk check state
        /// </summary>
        [ContextMenu("Log System State")]
        public void ManualLogState()
        {
            LogSystemState();
        }

    }
}
