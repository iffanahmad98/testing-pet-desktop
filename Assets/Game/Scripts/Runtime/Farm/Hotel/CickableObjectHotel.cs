using UnityEngine;
using System.Collections;
using DG.Tweening;
using MagicalGarden.Farm;

namespace MagicalGarden.Hotel
{
    [RequireComponent(typeof(Collider2D))]
    public class ClickableObjectHotel : MonoBehaviour
    {
        [Header("Hover Effect")]
        public float hoverScaleMultiplier = 1.1f;
        public float scaleSpeed = 5f;

        private Vector3 originalScale;
        private bool isHovered = false;
        private HotelController hotelController;
        private bool isMenuShown = false;
        private Tween currentTween;
        private Coroutine autoCloseCoroutine;

        private void Start()
        {
            originalScale = transform.localScale;
            hotelController = GetComponent<HotelController>();
        }

        private void Update()
        {
            Vector3 targetScale = isHovered ? originalScale * hoverScaleMultiplier : originalScale;
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * scaleSpeed);
            if (!isMenuShown && Input.GetMouseButtonDown(1)) // right-click to open
            {
                Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Collider2D col = Physics2D.OverlapPoint(mouseWorldPos);

                if (col != null && col.gameObject == gameObject && hotelController.IsOccupied)
                {
                    ShowMenu();
                }
            }

            // Klik kiri di luar object → tutup panel
            if (isMenuShown && Input.GetMouseButtonDown(1))
            {
                Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Collider2D col = Physics2D.OverlapPoint(mouseWorldPos);

                if (col == null || col.gameObject != gameObject)
                {
                    HideMenu();
                }
            }
        }

        private void OnMouseEnter()
        {
            isHovered = true;
        }

        private void OnMouseExit()
        {
            isHovered = false;
        }

        public void ShowMenu()
        {
            if (Farm.UIManager.Instance == null || Farm.UIManager.Instance.hotelInfoPanel == null)
                return;
            Farm.UIManager.Instance.hotelInfoPanel.Setup(hotelController);
            GameObject panel = Farm.UIManager.Instance.hotelInfoPanel.transform.gameObject;
            panel.transform.localScale = Vector3.zero;
            panel.transform.position = transform.position + new Vector3(0f, 5f, 0f);
            panel.SetActive(true);
            currentTween?.Kill();
            currentTween = panel.transform.DOScale(1f, 0.25f).SetEase(Ease.OutBack);
            isMenuShown = true;

            // Reset auto close
            if (autoCloseCoroutine != null)
                StopCoroutine(autoCloseCoroutine);
            autoCloseCoroutine = StartCoroutine(AutoCloseMenuAfterSeconds(4f));
        }

        public void HideMenu()
        {
            if (Farm.UIManager.Instance != null && Farm.UIManager.Instance.hotelInfoPanel != null)
            {
                GameObject panel = Farm.UIManager.Instance.hotelInfoPanel.transform.gameObject;

                currentTween?.Kill(); // Stop animasi sebelumnya

                // DoTween scale out
                currentTween = panel.transform.DOScale(0f, 0.2f)
                    .SetEase(Ease.InBack)
                    .OnComplete(() =>
                    {
                        panel.SetActive(false);
                    });
            }

            isMenuShown = false;

            if (autoCloseCoroutine != null)
            {
                StopCoroutine(autoCloseCoroutine);
                autoCloseCoroutine = null;
            }
        }
        private IEnumerator AutoCloseMenuAfterSeconds(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            HideMenu();
        }
    }
}