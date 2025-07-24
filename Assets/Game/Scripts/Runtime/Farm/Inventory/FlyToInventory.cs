using UnityEngine;
using UnityEngine.UI;

namespace MagicalGarden.Inventory
{
    public class FlyToInventory : MonoBehaviour
    {
        public float duration = 0.6f;
        private Vector3 startPos;
        private Vector3 targetPos;
        private Image iconImage;
        private float elapsed;
        private bool isFlying = false;

        public void Init(Sprite icon, Vector3 worldStart, RectTransform uiTarget)
        {
            startPos = Camera.main.WorldToScreenPoint(worldStart);
            targetPos = uiTarget.position;

            iconImage = GetComponent<Image>();
            iconImage.sprite = icon;
            transform.position = startPos;

            isFlying = true;
            elapsed = 0f;
        }

        void Update()
        {
            if (!isFlying) return;

            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            transform.position = Vector3.Lerp(startPos, targetPos, t);

            if (t >= 1f)
            {
                isFlying = false;
                Destroy(gameObject);
            }
        }
    }
}