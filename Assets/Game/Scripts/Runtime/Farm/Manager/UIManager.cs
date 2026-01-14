using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Collections;
using MagicalGarden.Hotel;

namespace MagicalGarden.Farm
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance;
        private Tween currentTween;

        [Header("Crop Information")]
        public CropInformation plantInfoPanel;
        private Coroutine hideInfoCoroutine;
        public Sprite goodCropIcon;
        public Sprite wiltCropIcon;
        public Sprite dieCropIcon;
        [Space(10)]
        public Sprite gardenFertiIcon;
        public Sprite moonFertiIcon;
        public Sprite nectarFertiIcon;
        public Sprite sapFertiIcon;
        [Header("Hotel Information")]
        public HotelInformation hotelInfoPanel;
        private Coroutine hideInfoHotelCoroutine;
        private bool isHotelInfoVisible = false;
        [Header("UI")]
        public TextMeshProUGUI coinText;
        public TextMeshProUGUI harvestText;
        [Header("Pop Up")]
        public GameObject fertizerUI;
        public GameObject shopUI;
        public GameObject guestUI;
        public GameObject inventoryUI;
        public GameObject menuBar;
        private float showY;
        private float hideY;
        private bool isInitialized = false;
        [Header ("UI (World)")]
        public Canvas uIWorldNonScaleable;
        
        //coroutine for click popupHotel
        [HideInInspector] public Coroutine autoCloseCoroutine;
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
            ToggleUI(fertizerUI);
        }
        public void ShopUIToogle()
        {
           // ToggleUI(shopUI); old
            FarmShop.instance.OnDisplay ();
        }
        public void GuestUIToogle()
        {
            ToggleUI(guestUI);
        }
        public void InventoryUIToogle()
        {
            ToggleUI(inventoryUI);
        }
        private void ToggleUI(GameObject targetUI)
        {
            // Debug.LogError("dsdsa");
            // bool isActive = targetUI.activeSelf;

            // Matikan semua UI
            fertizerUI.SetActive(false);
            shopUI.SetActive(false);
            guestUI.SetActive(false);
            inventoryUI.SetActive(false);

            // Jika sebelumnya tidak aktif, nyalakan yang diklik
            // if (!isActive)
            // {
            targetUI.SetActive(true);
            // }
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
        [ContextMenu("Toggle Menu Bar")]
        public void ToggleMenuBar()
        {
            RectTransform rect = menuBar.GetComponent<RectTransform>();

            // ✅ Simpan posisi awal (hanya sekali)
            if (!isInitialized)
            {
                showY = rect.anchoredPosition.y;
                hideY = showY - rect.rect.height;
                isInitialized = true;
            }

            if (!menuBar.activeSelf)
            {
                // Mulai dari posisi tersembunyi
                rect.anchoredPosition = new Vector2(0, hideY);
                menuBar.SetActive(true);
                rect.DOAnchorPosY(showY, 0.5f).SetEase(Ease.OutBack);
            }
            else
            {
                rect.DOAnchorPosY(hideY, 0.3f).SetEase(Ease.InBack).OnComplete(() =>
                {
                    menuBar.SetActive(false);
                });
            }
        }

        #region ToolTip UI
        // public void ShowMenu()
        // {
        //     if (currentOpenMenuHotel != null && currentOpenMenuHotel != this)
        //     {
        //         currentOpenMenuHotel.HideMenu();
        //     }
        //     currentOpenMenuHotel = this;
        //     if (Farm.UIManager.Instance == null || Farm.UIManager.Instance.hotelInfoPanel == null)
        //         return;
        //     Farm.UIManager.Instance.hotelInfoPanel.Setup(hotelController);
        //     GameObject panel = Farm.UIManager.Instance.hotelInfoPanel.transform.gameObject;
        //     panel.transform.localScale = Vector3.zero;
        //     panel.transform.position = transform.position + new Vector3(0f, 5f, 0f);
        //     panel.SetActive(true);
        //     currentTween?.Kill();
        //     currentTween = panel.transform.DOScale(1f, 0.25f).SetEase(Ease.OutBack);
        //     isMenuShown = true;
        //     currentOpenMenuHotel = this;

        //     // Reset auto close
        //     Coroutine cr = Farm.UIManager.Instance.autoCloseCoroutine;
        //     if (cr != null)
        //         StopCoroutine(Farm.UIManager.Instance.autoCloseCoroutine);
        //     Farm.UIManager.Instance.autoCloseCoroutine = StartCoroutine(AutoCloseMenuAfterSeconds(4f));
        //     justOpenedThisFrame = true;
        //     StartCoroutine(ClearJustOpenedFlag());
        // }

        #region Show Hotel Information
        public void ShowHotelInfo(HotelController hotelController,Vector3 screenPos)
        {
            if (hotelInfoPanel == null) return;
            GameObject panel = hotelInfoPanel.transform.gameObject;
            panel.transform.localScale = Vector3.one;
            panel.transform.position = hotelController.gameObject.transform.position + new Vector3(0f, 7f, 0f);
            panel.SetActive(true);
            hotelInfoPanel.Setup(hotelController);
            if (hideInfoHotelCoroutine != null)
                StopCoroutine(hideInfoHotelCoroutine);
            hideInfoHotelCoroutine = StartCoroutine(AutoHideHotelInfo());
        }
        public void HideHotelInfo()
        {
            currentTween?.Kill();
            currentTween = hotelInfoPanel.transform.DOScale(0f, 0.2f)
                .SetEase(Ease.InBack)
                .OnComplete(() =>
                {
                    hotelInfoPanel.transform.gameObject.SetActive(false);
                });
        }
        private IEnumerator AutoHideHotelInfo()
        {
            yield return new WaitForSeconds(4f);
            HideHotelInfo();
        }
        #endregion
        #region Show Plant Information
        public void ShowPlantInfo(PlantController plant, Vector3 screenPos)
        {
            if (plantInfoPanel == null) return;
            if (plant.seed.IsReadyToHarvest()) return;
            plantInfoPanel.transform.gameObject.SetActive(true);
            plantInfoPanel.transform.position = screenPos;
            plantInfoPanel.titleCrop.text = plant.seed.itemData.itemId;
            plantInfoPanel.cropImage.sprite = plant.seed.itemData.iconCrop;
            if (plant.seed.status == PlantStatus.Normal)
            {
                plantInfoPanel.statusLifeImage.sprite = goodCropIcon;
                plantInfoPanel.statusLifeText.text = "Good";
            }
            else if (plant.seed.status == PlantStatus.Layu)
            {
                plantInfoPanel.statusLifeImage.sprite = wiltCropIcon;
                plantInfoPanel.statusLifeText.text = "Dry";
            }
            else
            {
                plantInfoPanel.statusLifeImage.sprite = dieCropIcon;
                plantInfoPanel.statusLifeText.text = "Die";
            }
            if (plant.fertilizer == null)
            {
                plantInfoPanel.SetFertiInfo(false);
            }
            else
            {
                plantInfoPanel.SetFertiInfo(true);
                plantInfoPanel.fertiTypeImage.sprite = plant.fertilizer.icon;
                plantInfoPanel.fertiGrowthSpeedText.text = $"+{plant.fertilizer.boost}% growth speed";
            }

            // plantInfoPanel.fertiGrowthSpeedText.text = "";
            plantInfoPanel.waterTimeText.text = plant.GetLastWateredTimeText();
            plantInfoPanel.lifeTimeText.text = plant.GetTimeUntilWiltOrDeathText();
            if (hideInfoCoroutine != null)
                StopCoroutine(hideInfoCoroutine);

            currentTween?.Kill();

            // Munculkan dengan animasi
            plantInfoPanel.transform.localScale = Vector3.zero;
            // plantInfoPanel.transform.gameObject.SetActive(true);

            currentTween = plantInfoPanel.transform.DOScale(1f, 0.25f).SetEase(Ease.OutBack);

            hideInfoCoroutine = StartCoroutine(AutoHidePlantInfo());
        }

        public void HidePlantInfo()
        {
            currentTween?.Kill();

            currentTween = plantInfoPanel.transform.DOScale(0f, 0.2f)
                .SetEase(Ease.InBack)
                .OnComplete(() =>
                {
                    plantInfoPanel.transform.gameObject.SetActive(false);
                });
        }
        private IEnumerator AutoHidePlantInfo()
        {
            yield return new WaitForSeconds(4f);
            HidePlantInfo();
        }
        #endregion
#endregion

        #region UI
    
        public void SetUIEqualsFocus (string sceneFocus) { // BoardSign
          //  Debug.Log ("Focus Scene 3 " + sceneFocus);

            if (sceneFocus == "Farm") {
                SetUIFarm ();
            } else if (sceneFocus == "Hotel") {
                SetUIHotel ();
            }
        }

        void SetUIFarm () { // SceneFocusManager
            menuBar.gameObject.SetActive (true);
        }

        void SetUIHotel () { // SceneFocusManager
            menuBar.gameObject.SetActive (false);
        }

        public void ShowUIFarmBar () { // FarmShop
            menuBar.gameObject.SetActive (true);
        }

        public void HideUIFarmBar () {
            menuBar.gameObject.SetActive (false);
        }
        #endregion
    
    
    }

}