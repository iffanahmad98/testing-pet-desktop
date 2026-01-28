using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MagicalGarden.Manager;

namespace MagicalGarden.Farm
{
    public class UnlockBubbleUI : MonoBehaviour
    {
        public TextMeshProUGUI target1Text;
        public TextMeshProUGUI target2Text;
        public Button unlockButton;
        public Sprite unlockSprite;
        public Sprite lockSprite;
        
        private FieldBlock block;
        private Vector3 worldPos;

        [Header ("Sprite Request")]
        [SerializeField] Image bubblePanel;
        [SerializeField] Image requestImage2;
        [SerializeField] Sprite coinSprite, fruitSprite, eggSprite;
        [Header ("Eligibility")]
        FarmAreaEligibleDataSO eligibleData;
        [Header ("Vfx")]
        public GameObject purchaseVfx;

        public void Setup(FieldBlock fieldBlock, Vector3Int tilePos, FarmAreaEligibleDataSO eligibleDataValue)
        {
            block = fieldBlock;
            worldPos = TileManager.Instance.tilemapSoil.CellToWorld(tilePos) + new Vector3(0f, 0.5f, 0);
            transform.position = worldPos;
            eligibleData = eligibleDataValue;
          //  coinText.text = block.requiredCoins.ToString();
          //  harvestText.text = block.requiredHarvest.ToString();
            target1Text.text = eligibleData.GetPrice ().ToString ();
            if (eligibleData.IsEligibleEnoughPetMonsterOnly ()) {
                bubblePanel.gameObject.SetActive (true);
            } else {
                bubblePanel.gameObject.SetActive (false);
            }

            if (eligibleData.GetHarvestFruit () == 0) {
                target2Text.text = eligibleData.GetHarvestEgg ().ToString ();
                requestImage2.sprite = eggSprite;
            } else {
                target2Text.text = eligibleData.GetHarvestFruit ().ToString ();
                requestImage2.sprite = fruitSprite;
            }
            
            if (CanUnlock ()) {
                unlockButton.image.sprite = unlockSprite;
            } else {
                unlockButton.image.sprite = lockSprite;
            }

            unlockButton.onClick.RemoveAllListeners();
            unlockButton.onClick.AddListener(OnUnlockClicked);
        }

        void OnUnlockClicked()
        {
            if (CanUnlock())
            {
               // CoinManager.Instance.SpendCoins(block.requiredCoins);
                CoinManager.Instance.SpendCoins (eligibleData.GetPrice ());
                FieldManager.Instance.UnlockBlock(block.blockId);
                PlantManager.Instance.PurchaseFarmArea (block.numberId); 
                Debug.Log (" Block Id : " + block.numberId);
                InstantiateVfxPurchase ();
                Destroy(gameObject); // Remove the bubble
            }
            else
            {
                Debug.Log("Syarat belum terpenuhi");
            }
        }

        /*
        bool CanUnlock()
        {
            return PlantManager.Instance.GetAmountHarvest() >= block.requiredHarvest &&
                CoinManager.Instance.coins >= block.requiredCoins;
        }
        */
        void InstantiateVfxPurchase () {
            GameObject cloneVfx = GameObject.Instantiate (purchaseVfx);
            cloneVfx.transform.position = worldPos;
            cloneVfx.SetActive (true);
        }

        bool CanUnlock()
        {
            return eligibleData.IsEligible ();
        }

        
    }
}