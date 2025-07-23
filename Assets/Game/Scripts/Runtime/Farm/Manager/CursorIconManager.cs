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
        [Header("Pour Prefab")]
        public GameObject pourAnimPrefab;
        Sprite currentSprite;

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

        public void PlayPourAnimation(string animationStateName)
        {
            if (pourAnimPrefab == null) return;

            // ⛔ Hindari overwrite jika sprite kosong
            if (seedIconImage.sprite != null)
                currentSprite = seedIconImage.sprite;
            HideSeedIcon();
            GameObject animObj = Instantiate(pourAnimPrefab, seedIconImage.canvas.transform);
            animObj.transform.position = Input.mousePosition;
            Animator anim = animObj.GetComponent<Animator>();
            if (anim != null)
            {
                anim.Play(animationStateName, 0, 0);
            }
            float animDuration = 1f;
            Destroy(animObj, animDuration);
            Invoke(nameof(RestoreSeedIcon), animDuration);
        }
        void RestoreSeedIcon()
        {
            ShowSeedIcon(currentSprite);
        }
    }
}