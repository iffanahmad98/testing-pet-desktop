using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

namespace MagicalGarden.Farm
{
    public class CameraDragMove : MonoBehaviour
    {
        [Header("Drag Settings")]
        public float dragSpeed = 5f;
        private Vector3 dragOrigin;
        private bool isDragging = false;

        [Header("Zoom Settings")]
        public float zoomSpeed = 5f;
        public float minZoom = 3f;
        public float maxZoom = 12f;
        [Header("Control Flags")]
        public bool canDrag = true;
        public bool canZoom = true;

        private Camera cam;

        private Coroutine zoomCoroutine;

        void Start()
        {
            cam = Camera.main;
        }

        void Update()
        {
            if (EventSystem.current.IsPointerOverGameObject())
                return;
            HandleDrag();
            HandleZoom();
        }

        void HandleDrag()
        {
            if (!canDrag) return;
            if (Input.GetMouseButtonDown(2)) // Middle mouse
            {
                isDragging = true;
                dragOrigin = cam.ScreenToWorldPoint(Input.mousePosition);
            }

            if (Input.GetMouseButtonUp(2))
            {
                isDragging = false;
            }

            if (isDragging)
            {
                Vector3 currentPos = cam.ScreenToWorldPoint(Input.mousePosition);
                Vector3 difference = dragOrigin - currentPos;
                transform.position += difference;
            }
        }

        void HandleZoom()
        {
            if (!canZoom) return;
            float scroll = Input.mouseScrollDelta.y;
            if (scroll != 0f)
            {
                float targetZoom = cam.orthographicSize - scroll * zoomSpeed * Time.deltaTime;
                cam.orthographicSize = Mathf.Clamp(targetZoom, minZoom, maxZoom);
            }
        }

        // Call this when pet starts evolving
        public void FocusOnTarget(Vector3 target, float zoomSize = 4f, float duration = 1f)
        {
            if (zoomCoroutine != null)
                StopCoroutine(zoomCoroutine);

            zoomCoroutine = StartCoroutine(ZoomAndFocus(target, zoomSize, duration));
        }

        // Call this after evolve ends
        public void ResetZoom(float duration = 1f)
        {
            if (zoomCoroutine != null)
                StopCoroutine(zoomCoroutine);

            zoomCoroutine = StartCoroutine(ZoomAndFocus(transform.position, maxZoom/2, duration, resetPosition: true));
        }

        IEnumerator ZoomAndFocus(Vector3 targetPosition, float targetZoom, float duration, bool resetPosition = false)
        {
            Vector3 startPos = transform.position;
            float startZoom = cam.orthographicSize;

            float time = 0f;
            while (time < duration)
            {
                time += Time.deltaTime;
                float t = time / duration;

                transform.position = Vector3.Lerp(startPos, new Vector3(targetPosition.x, targetPosition.y, startPos.z), t);
                cam.orthographicSize = Mathf.Lerp(startZoom, targetZoom, t);
                yield return null;
            }

            if (resetPosition)
                transform.position = startPos; // Optional: Keep original camera pos after zoom out
        }
    }
}
