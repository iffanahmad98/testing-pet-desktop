using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

namespace MagicalGarden.Farm
{
    public class CameraDragMove : MonoBehaviour
    {
        [Header("Drag Boundaries")]
        public Collider2D boundaryColliderFarm;
        public Collider2D boundaryColliderHotel;
        private Collider2D currentBoundary;

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
            currentBoundary = boundaryColliderFarm; // default ke farm
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
                Vector3 newPos = transform.position + difference;
                transform.position = ClampCameraPosition(newPos, cam.orthographicSize);
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
                transform.position = ClampCameraPosition(transform.position, cam.orthographicSize);
            }
        }

        Vector3 ClampCameraPosition(Vector3 targetPos, float customZoom)
        {
            if (currentBoundary == null) return targetPos;

            Bounds bounds = currentBoundary.bounds;
            float camHeight = customZoom;
            float camWidth = camHeight * cam.aspect;

            float minX = bounds.min.x + camWidth;
            float maxX = bounds.max.x - camWidth;
            float minY = bounds.min.y + camHeight;
            float maxY = bounds.max.y - camHeight;

            float clampedX = Mathf.Clamp(targetPos.x, minX, maxX);
            float clampedY = Mathf.Clamp(targetPos.y, minY, maxY);

            return new Vector3(clampedX, clampedY, targetPos.z);
        }

        // Focus kamera ke titik target sambil zoom in/out
        public void FocusOnTarget(Vector3 target, float zoomSize = 4f, float duration = 1f, bool isHotel = false)
        {
            currentBoundary = isHotel ? boundaryColliderHotel : boundaryColliderFarm;

            if (zoomCoroutine != null)
                StopCoroutine(zoomCoroutine);
            zoomCoroutine = StartCoroutine(MoveToTarget(target, duration));
        }

        // Reset kamera ke zoom default (posisi tetap)
        public void ResetZoom(float duration = 1f)
        {
            if (zoomCoroutine != null)
                StopCoroutine(zoomCoroutine);
            zoomCoroutine = StartCoroutine(MoveToTarget(transform.position, duration));
        }

        IEnumerator MoveToTarget(Vector3 targetPosition, float duration)
        {
            yield return null; // pastikan posisi awal sudah benar

            Vector3 startPos = transform.position;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float easedT = EaseInOutSine(t);

                Vector3 pos = Vector3.Lerp(startPos, new Vector3(targetPosition.x, targetPosition.y, startPos.z), easedT);
                transform.position = pos; // langsung set tanpa clamp

                yield return null;
            }

            transform.position = new Vector3(targetPosition.x, targetPosition.y, startPos.z); // final snap
        }
        float EaseInOutSine(float t)
        {
            return -(Mathf.Cos(Mathf.PI * t) - 1) / 2;
        }
    }
}
