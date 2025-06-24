using UnityEngine;
using UnityEngine.UI;

namespace MagicalGarden.Farm
{
    public class CursorIconManager : MonoBehaviour
    {
        public static CursorIconManager Instance;
        public Image seedIconImage;
        public Sprite wateringIconImage;
        public Sprite removeIconImage;
        public Sprite harvestIconImage;

        private RectTransform iconRect;

        void Awake()
        {
            Instance = this;
            iconRect = seedIconImage.rectTransform;
            seedIconImage.gameObject.SetActive(false);
        }

        void Update()
        {
            if (seedIconImage.gameObject.activeSelf)
            {
                iconRect.position = Input.mousePosition;
            }
        }

        public void ShowSeedIcon(Sprite icon)
        {
            seedIconImage.sprite = icon;
            seedIconImage.gameObject.SetActive(true);
        }
        public void ShowWateringIcon()
        {
            seedIconImage.sprite = wateringIconImage;
            seedIconImage.gameObject.SetActive(true);
        }

        public void ShowHarvestIcon()
        {
            seedIconImage.sprite = harvestIconImage;
            seedIconImage.gameObject.SetActive(true);
        }
        public void ShowRemoveIcon()
        {
            seedIconImage.sprite = removeIconImage;
            seedIconImage.gameObject.SetActive(true);
        }

        public void HideSeedIcon()
        {
            seedIconImage.sprite = null;
            seedIconImage.gameObject.SetActive(false);
        }
    }
}