using UnityEngine;
using UnityEngine.EventSystems;
using MagicalGarden.Inventory;
using MagicalGarden.Farm;
using System.Collections;

namespace MagicalGarden.Gift
{
    /// <summary>
    /// Gift item yang bisa diklik untuk mendapatkan reward
    /// Attach ke GameObject gift prefab
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class GiftItem : MonoBehaviour, IPointerClickHandler
    {
        [Header("Gift Configuration")]
        [Tooltip("Ukuran gift (menentukan reward table)")]
        public GiftSize giftSize = GiftSize.Small;

        [Tooltip("Reward table untuk gift size ini")]
        public GiftRewardTable rewardTable;

        [Header("Animation Settings")]
        [Tooltip("Durasi animasi scale bounce")]
        public float bounceScale = 1.2f;
        public float bounceDuration = 0.3f;

        [Header("Destroy Settings")]
        [Tooltip("Auto destroy setelah dibuka?")]
        public bool autoDestroyAfterOpen = true;
        public float destroyDelay = 2f;

        // Internal
        private bool isOpened = false;

        public System.Action OnGiftOpened;

        void Awake()
        {
            // No initialization needed
        }

        /// <summary>
        /// IPointerClickHandler - dipanggil saat gift diklik
        /// </summary>
        public void OnPointerClick(PointerEventData eventData)
        {
            if (!isOpened)
            {
                OpenGift();
            }
        }

        /// <summary>
        /// Alternatif: OnMouseDown untuk click detection (jika tidak pakai EventSystem)
        /// </summary>
        void OnMouseDown()
        {
            if (!isOpened)
            {
                OpenGift();
            }
        }

        /// <summary>
        /// Buka gift dan berikan reward
        /// </summary>

        public void OpenGift()
        {
            if (isOpened) return;

            isOpened = true;

            // Get reward dari table
            GiftRewardData reward = GetReward();

            if (reward != null)
            {
                // Give reward ke player
                GiveReward(reward);

                // Visual & audio feedback
                PlayOpenFeedback();

                // Show notification
                ShowRewardNotification(reward);
            }
            else
            {
                Debug.LogWarning("GiftItem: Reward table tidak di-assign atau kosong!");
            }

            // Beri tahu listener (misalnya tutorial) bahwa gift sudah dibuka.
            OnGiftOpened?.Invoke();

            // Auto destroy jika enabled
            if (autoDestroyAfterOpen)
            {
                // Destroy(gameObject, destroyDelay);
            }

            HotelGiftHandler.instance.ClaimGift(this, true);
            Destroy(this.gameObject);
        }

        /// <summary>
        /// Get reward dari reward table
        /// </summary>
        private GiftRewardData GetReward()
        {
            if (rewardTable == null)
            {
                Debug.LogError("GiftItem: Reward table belum di-assign!");
                return null;
            }

            return rewardTable.GetRandomReward();
        }


        /// <summary>
        /// Berikan reward ke player berdasarkan tipe
        /// </summary>
        private void GiveReward(GiftRewardData reward)
        {
            int amount = reward.GetRandomAmount();

            switch (reward.rewardType)
            {
                case GiftRewardType.Coin:
                    if (MagicalGarden.Farm.CoinManager.Instance != null)
                    {
                        int coinsBefore = MagicalGarden.Farm.CoinManager.Instance.coins;
                        MagicalGarden.Farm.CoinManager.Instance.AddCoins(amount);
                        int coinsAfter = MagicalGarden.Farm.CoinManager.Instance.coins;
                        Debug.Log($"<color=yellow>[Gift] Coin Reward:</color> +{amount} coins | Before: {coinsBefore} → After: {coinsAfter}");
                    }
                    else
                    {
                        Debug.LogError($"<color=red>[Gift] ERROR: CoinManager.Instance is NULL!</color>");
                    }
                    break;

                case GiftRewardType.DoubleCoin:
                    int doubleAmount = amount * 2;
                    if (MagicalGarden.Farm.CoinManager.Instance != null)
                    {
                        int doubleCoinsBefore = MagicalGarden.Farm.CoinManager.Instance.coins;
                        MagicalGarden.Farm.CoinManager.Instance.AddCoins(doubleAmount);
                        int doubleCoinsAfter = MagicalGarden.Farm.CoinManager.Instance.coins;
                        Debug.Log($"<color=yellow>[Gift] BONUS 2x Coin:</color> +{doubleAmount} coins | Before: {doubleCoinsBefore} → After: {doubleCoinsAfter}");
                    }
                    else
                    {
                        Debug.LogError($"<color=red>[Gift] ERROR: CoinManager.Instance is NULL!</color>");
                    }
                    break;

                case GiftRewardType.Empty:
                    // Empty dikonversi ke coin
                    int emptyCoins = amount;
                    if (MagicalGarden.Farm.CoinManager.Instance != null)
                    {
                        int emptyCoinsBefore = MagicalGarden.Farm.CoinManager.Instance.coins;
                        MagicalGarden.Farm.CoinManager.Instance.AddCoins(emptyCoins);
                        int emptyCoinsAfter = MagicalGarden.Farm.CoinManager.Instance.coins;
                        Debug.Log($"<color=gray>[Gift] Empty Gift:</color> +{emptyCoins} coins (kompensasi) | Before: {emptyCoinsBefore} → After: {emptyCoinsAfter}");
                    }
                    else
                    {
                        Debug.LogError($"<color=red>[Gift] ERROR: CoinManager.Instance is NULL!</color>");
                    }
                    break;

                case GiftRewardType.FoodPack:
                case GiftRewardType.Medicine:
                case GiftRewardType.GoldenTicket:
                case GiftRewardType.Decoration:
                    // Item-based rewards
                    if (reward.itemData != null && InventoryManager.Instance != null)
                    {
                        InventoryManager.Instance.AddItem(reward.itemData, amount);
                        Debug.Log($"<color=green>[Gift] Item Reward:</color> +{amount}x {reward.itemData.displayName}");
                    }
                    else
                    {
                        Debug.LogWarning($"<color=orange>[Gift] WARNING:</color> ItemData atau InventoryManager tidak tersedia untuk {reward.rewardType}");
                    }
                    break;
            }
        }

        /// <summary>
        /// Play visual & audio feedback saat gift dibuka
        /// </summary>
        private void PlayOpenFeedback()
        {
            // Bounce animation
            StartCoroutine(BounceAnimation());

            // collect gift SFX is at index 1
            MonsterManager.instance.audio.PlayFarmSFX(1);
        }

        /// <summary>
        /// Animasi bounce saat gift dibuka
        /// </summary>
        private IEnumerator BounceAnimation()
        {
            Vector3 originalScale = transform.localScale;
            Vector3 targetScale = originalScale * bounceScale;

            // Scale up
            float elapsed = 0f;
            while (elapsed < bounceDuration / 2)
            {
                transform.localScale = Vector3.Lerp(originalScale, targetScale, elapsed / (bounceDuration / 2));
                elapsed += Time.deltaTime;
                yield return null;
            }

            // Scale down
            elapsed = 0f;
            while (elapsed < bounceDuration / 2)
            {
                transform.localScale = Vector3.Lerp(targetScale, originalScale, elapsed / (bounceDuration / 2));
                elapsed += Time.deltaTime;
                yield return null;
            }

            transform.localScale = originalScale;
        }

        /// <summary>
        /// Tampilkan notifikasi reward (bisa dikustomisasi dengan UI)
        /// </summary>
        private void ShowRewardNotification(GiftRewardData reward)
        {
            int amount = reward.GetRandomAmount();
            string message = "";

            switch (reward.rewardType)
            {
                case GiftRewardType.Coin:
                    message = $"+{amount} Coins";
                    break;
                case GiftRewardType.DoubleCoin:
                    message = $"+{amount * 2} Coins (BONUS 2x!)";
                    break;
                case GiftRewardType.Empty:
                    message = $"Empty... +{amount} Coins";
                    break;
                case GiftRewardType.FoodPack:
                case GiftRewardType.Medicine:
                case GiftRewardType.GoldenTicket:
                case GiftRewardType.Decoration:
                    if (reward.itemData != null)
                    {
                        message = $"+{amount}x {reward.itemData.displayName}";
                    }
                    break;
            }

            // TODO: Integrate dengan UI notification system jika ada
            // Untuk sekarang, spawn text notification sederhana
            SpawnFloatingText(message);
        }

        /// <summary>
        /// Spawn floating text notification (simple version)
        /// </summary>
        private void SpawnFloatingText(string text)
        {
            // TODO: Bisa dibuat lebih fancy dengan TextMeshPro dan animation
            GameObject textObj = new GameObject("FloatingText");
            textObj.transform.position = transform.position + Vector3.up;

            TextMesh textMesh = textObj.AddComponent<TextMesh>();
            textMesh.text = text;
            textMesh.fontSize = 40;
            textMesh.color = Color.yellow;
            textMesh.alignment = TextAlignment.Center;
            textMesh.anchor = TextAnchor.MiddleCenter;

            // Simple float up animation
            StartCoroutine(FloatUpText(textObj));
        }

        /// <summary>
        /// Animasi floating text naik ke atas
        /// </summary>
        private IEnumerator FloatUpText(GameObject textObj)
        {
            float duration = 1.5f;
            float elapsed = 0f;
            Vector3 startPos = textObj.transform.position;
            Vector3 endPos = startPos + Vector3.up * 2f;

            while (elapsed < duration)
            {
                textObj.transform.position = Vector3.Lerp(startPos, endPos, elapsed / duration);
                elapsed += Time.deltaTime;
                yield return null;
            }

            Destroy(textObj);
        }

        #region NPCRoboShroom
        public void OpenGiftByNPC()
        {
            HotelGiftHandler.instance.ClaimGift(this, false);
            Destroy(this.gameObject);
        }
        #endregion
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            // Draw gift size info
            Gizmos.color = giftSize == GiftSize.Large ? Color.yellow :
                          giftSize == GiftSize.Medium ? Color.cyan :
                          Color.green;

            Gizmos.DrawWireSphere(transform.position, 0.5f);

            // Label
            UnityEditor.Handles.Label(
                transform.position + Vector3.down * 0.7f,
                $"Gift: {giftSize}\n{(isOpened ? "OPENED" : "CLOSED")}",
                new GUIStyle()
                {
                    normal = new GUIStyleState() { textColor = Color.white },
                    fontSize = 10,
                    alignment = TextAnchor.MiddleCenter
                }
            );
        }
#endif
    }
}
