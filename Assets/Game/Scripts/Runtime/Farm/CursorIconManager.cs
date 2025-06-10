using UnityEngine;
using UnityEngine.UI;

namespace MagicalGarden.Farm
{
    public class CursorIconManager : MonoBehaviour
    {
        public static CursorIconManager Instance;
        public Image seedIconImage;

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

        public void HideSeedIcon()
        {
            seedIconImage.gameObject.SetActive(false);
        }
    }
}