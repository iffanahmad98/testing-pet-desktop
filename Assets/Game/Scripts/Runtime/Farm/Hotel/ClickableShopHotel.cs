using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MagicalGarden.Hotel
{
    [RequireComponent(typeof(Collider2D))]
    public class ClickableShopHotel : MonoBehaviour
    {
        [SerializeField] private GameObject _shopUI;
        [SerializeField] private float _openDelay;

        [Header("Animation")]
        [SerializeField] private Animator animator;
        [SerializeField] private string clickedAnimationName = "clicked";

        [Header("Hover Effect")]
        [SerializeField] private bool enableHoverEffect = true;
        [SerializeField] private float hoverScaleMultiplier = 1.1f;
        [SerializeField] private float scaleSpeed = 5f;

        [Header("Hotel Reference")]
        [SerializeField] private HotelController hotelController;
        [SerializeField] private HotelInformation hotelInformation;

        private Vector3 originalScale;
        private bool isHovered = false;

        private void Start()
        {
            originalScale = transform.localScale;

            // Get Animator component if not assigned
            if (animator == null)
            {
                animator = GetComponent<Animator>();
            }
        }

        private void Update()
        {
            // Handle hover scale effect
            if (enableHoverEffect)
            {
                Vector3 targetScale = isHovered ? originalScale * hoverScaleMultiplier : originalScale;
                transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * scaleSpeed);
            }
        }

        private void OnMouseEnter()
        {
            // Ignore if pointer is over UI
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            isHovered = true;
        }

        private void OnMouseExit()
        {
            // Ignore if pointer is over UI
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            isHovered = false;
        }

        private void OnMouseDown()
        {
            // Ignore if pointer is over UI
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            PlayClickedAnimation();

            StartCoroutine(OpenShopWithDelay());
        }

        private IEnumerator OpenShopWithDelay()
        {
            // Tunggu animasi selesai
            yield return new WaitForSeconds(_openDelay);

            if (_shopUI != null)
            {
                _shopUI.SetActive(true);

                // Set HotelController reference to HotelInformation if both are assigned
                if (hotelInformation != null && hotelController != null)
                {
                    hotelInformation.Setup(hotelController);
                }
            }
        }

        /// <summary>
        /// Play the clicked animation
        /// </summary>
        public void PlayClickedAnimation()
        {
            if (animator != null)
            {
                // Try to trigger animation by trigger name
                animator.SetTrigger(clickedAnimationName);

                // Alternative: directly play animation state
                // animator.Play(clickedAnimationName);
            }
            else
            {
                Debug.LogWarning($"[ClickableShopHotel] Animator is not assigned on {gameObject.name}");
            }
        }
    }
}
