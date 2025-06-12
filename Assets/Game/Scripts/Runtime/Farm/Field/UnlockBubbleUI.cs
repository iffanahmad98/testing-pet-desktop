using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MagicalGarden.Farm;

namespace MagicalGarden.Farm
{
    public class UnlockBubbleUI : MonoBehaviour
    {
        public TextMeshProUGUI descText;
        public Button unlockButton;

        private FieldBlock block;
        private Vector3 worldPos;

        public void Setup(FieldBlock fieldBlock, Vector3Int tilePos)
        {
            block = fieldBlock;
            worldPos = TileManager.Instance.tilemapLocked.CellToWorld(tilePos) + new Vector3(0f, 0.5f, 0);
            transform.position = worldPos;

            descText.text = $"coin : {block.requiredCoins} | harvest : {block.requiredHarvest}";
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
                descText.text = "";
                Debug.Log("Syarat belum terpenuhi");
                descText.text = $"coin : {block.requiredCoins} | harvest : {block.requiredHarvest}";
                descText.text += "\nSyarat belum terpenuhi";
            }
        }

        bool CanUnlock()
        {
            return PlantManager.Instance.GetAmountHarvest() >= block.requiredHarvest &&
                CoinManager.Instance.coins >= block.requiredCoins;
        }
    }
}