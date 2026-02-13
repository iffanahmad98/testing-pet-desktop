using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

namespace MagicalGarden.Farm
{
    public partial class CameraDragMove : MonoBehaviour
    {
        #region Inspector Fields

        public CameraDragLocker cameraDragLocker;

        [Header("Drag Boundaries")]
        public Collider2D boundaryColliderFarm;
        public Collider2D boundaryColliderHotel;

        [Header("Drag Settings")]
        public float dragSpeed = 5f;
        public bool allowLeftClickDrag = true;

        [Header("Zoom Settings")]
        public float zoomSpeed = 5f;
        public float minZoom = 3f;
        public float maxZoom = 12f;

        [Header("Keyboard/Pad Pan")]
        public float panSpeed = 10f;

        [Header("Control Flags")]
        public bool canDrag = true;
        public bool canZoom = true;

        #endregion

        #region Private Fields

        private Collider2D currentBoundary;
        private Vector3 dragOrigin;
        private bool isDragging = false;
        private Camera cam;
        private Coroutine zoomCoroutine;

        private bool isLockedByTutorial = false;

        #endregion

        #region Properties

        public bool IsLockedByTutorial => isLockedByTutorial;

        #endregion

        #region Unity Lifecycle

        private void Start()
        {
            cam = Camera.main;
            currentBoundary = boundaryColliderFarm; // default ke farm
        }

        private void Update()
        {
            if (isLockedByTutorial)
                return;

            HandleKeyboardPan();

            if (!cameraDragLocker.IsCan())
                return;

            if (EventSystem.current.IsPointerOverGameObject())
                return;

            HandleDrag();
            HandleZoom();
        }

        #endregion

        #region Tutorial Lock API
        public void LockForTutorial()
        {
            isLockedByTutorial = true;
            isDragging = false; // Cancel any ongoing drag
            Debug.Log("[CameraDragMove] Locked for tutorial");
        }

        public void UnlockAfterTutorial()
        {
            isLockedByTutorial = false;
            Debug.Log("[CameraDragMove] Unlocked after tutorial");
        }

        #endregion

        #region Boundary Switching

        public void SetBoundary(bool isHotel)
        {
            currentBoundary = isHotel ? boundaryColliderHotel : boundaryColliderFarm;
        }

        #endregion

    }
}
