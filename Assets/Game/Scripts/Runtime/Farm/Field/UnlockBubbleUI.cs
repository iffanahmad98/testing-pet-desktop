using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MagicalGarden.Manager;

namespace MagicalGarden.Farm
{
    public class UnlockBubbleUI : MonoBehaviour
    {
        public TextMeshProUGUI coinText;
        public TextMeshProUGUI harvestText;
        public Button unlockButton;
        public Sprite unlockSprite;
        public Sprite lockSprite;
        
        private FieldBlock block;
        private Vector3 worldPos;

        public void Setup(FieldBlock fieldBlock, Vector3Int tilePos)
        {
            block = fieldBlock;
            worldPos = TileManager.Instance.tilemapSoil.CellToWorld(tilePos) + new Vector3(0f, 0.5f, 0);
            transform.position = worldPos;
            coinText.text = block.requiredCoins.ToString();
            harvestText.text = block.requiredHarvest.ToString();
            unlockButton.onClick.RemoveAllListeners();
            unlockButton.onClick.AddListener(OnUnlockClicked);
        }

        void OnUnlockClicked()
        {
            if (CanUnlock())
            {
                CoinManager.Instance.SpendCoins(block.requiredCoins);
                FieldManager.Instance.UnlockBlock(block.blockId);
                Destroy(gameObject); // Remove the bubble
            }
            else
            {
                Debug.Log("Syarat belum terpenuhi");
            }
        }

        bool CanUnlock()
        {
            return PlantManager.Instance.GetAmountHarvest() >= block.requiredHarvest &&
                CoinManager.Instance.coins >= block.requiredCoins;
        }
    }
}