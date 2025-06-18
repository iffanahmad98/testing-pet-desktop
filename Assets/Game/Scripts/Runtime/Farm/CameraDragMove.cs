using UnityEngine;

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

        private Camera cam;

        void Start()
        {
            cam = Camera.main;
        }

        void Update()
        {
            HandleDrag();
            HandleZoom();
        }

        void HandleDrag()
        {
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
            float scroll = Input.mouseScrollDelta.y;
            if (scroll != 0f)
            {
                float targetZoom = cam.orthographicSize - scroll * zoomSpeed * Time.deltaTime;
                cam.orthographicSize = Mathf.Clamp(targetZoom, minZoom, maxZoom);
            }
        }
    }
}
