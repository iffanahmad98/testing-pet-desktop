using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MagicalGarden.Farm.UI;

namespace MagicalGarden.Farm
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance;

        [Header("Plant ToolTip")]
        public GameObject plantInfoPanel;
        public TextMeshProUGUI plantInfoText;

        [Header("UI")]
        public TextMeshProUGUI coinText;
        public TextMeshProUGUI harvestText;
        public GameObject fertizerUI;
        public GameObject shopUI;
        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Start()
        {
            if (PlantManager.Instance != null)
            {
                PlantManager.Instance.OnHarvestChanged += UpdateUI;
                CoinManager.Instance.OnCoinChanged += UpdateUI;
            }

            UpdateUI();
        }

        public void FertilizeUIToogle()
        {
            fertizerUI.SetActive(!fertizerUI.activeSelf);
        }
        public void ShopUIToogle()
        {
            shopUI.SetActive(!shopUI.activeSelf);
        }
        private void OnDestroy()
        {
            if (PlantManager.Instance != null)
            {
                PlantManager.Instance.OnHarvestChanged -= UpdateUI;
                CoinManager.Instance.OnCoinChanged -= UpdateUI;
            }
        }

        public void UpdateUI()
        {
            int coins = CoinManager.Instance != null ? CoinManager.Instance.coins : 0;
            int harvests = PlantManager.Instance != null ? PlantManager.Instance.GetAmountHarvest() : 0;

            coinText.text = "coin : " + coins;
            harvestText.text = "harvest : " + harvests;
        }

#region ToolTip UI
        public void ShowPlantInfo(string info, Vector3 screenPos)
        {
            plantInfoPanel.SetActive(true);
            plantInfoText.text = info;
            transform.position = screenPos;
        }

        public void HidePlantInfo()
        {
            plantInfoPanel.SetActive(false);
        }
        #endregion
        public void TogglefertizerUI()
        {
            fertizerUI.SetActive(!fertizerUI.activeSelf);
        }
    }

}