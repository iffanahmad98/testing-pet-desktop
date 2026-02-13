using UnityEngine;

[DisallowMultipleComponent]
public class TutorialHandPointer : MonoBehaviour, ITutorialPointer
{
    [Header("References")]
    [Tooltip("RectTransform dari image tangan yang akan dianimasikan.")]
    [SerializeField] private RectTransform pointerRect;

    [Tooltip("Canvas utama tempat pointer ini berada. Kalau kosong akan cari Canvas terdekat.")]
    [SerializeField] private Canvas rootCanvas;

    [Header("Movement Settings")]
    [Tooltip("Seberapa halus pointer mengikuti target (semakin besar semakin lambat).")]
    [SerializeField] private float followSmoothTime = 0.15f;

    [Header("Sway Animation")]
    [Tooltip("Jarak goyangan kiri-kanan (dalam satuan anchoredPosition).")]
    [SerializeField] private float swayAmplitude = 20f;

    [Tooltip("Kecepatan goyangan kiri-kanan.")]
    [SerializeField] private float swaySpeed = 3f;

    [Header("World Target Sway (PointToWorld)")]
    [Tooltip("Jarak goyangan kiri-kanan khusus saat mengikuti target dunia (PointToWorld).")]
    [SerializeField] private float worldSwayAmplitude = 25f;

    [Tooltip("Kecepatan goyangan khusus saat mengikuti target dunia (PointToWorld).")]
    [SerializeField] private float worldSwaySpeed = 3.5f;

    [Header("Sorting (Layer)")]
    [Tooltip("Sorting order tinggi supaya pointer selalu di depan tanpa perlu mengubah hierarki.")]
    [SerializeField] private int sortingOrderOnTop = 5000;

    private RectTransform _canvasRect;
    private RectTransform _target;
    private Transform _worldTarget;
    private Vector2 _offset;
    private Vector3 _worldOffset;
    private Vector2 _velocity;
    private float _swayTime;
    private Canvas _pointerCanvas;
    private System.Func<RectTransform> _targetResolver;

    private void Awake()
    {
        if (rootCanvas == null)
        {
            rootCanvas = GetComponentInParent<Canvas>();
        }


        if (rootCanvas != null)
        {
            _canvasRect = rootCanvas.transform as RectTransform;
        }

        if (pointerRect == null)
        {
            pointerRect = transform as RectTransform;
        }

        if (pointerRect != null)
        {
            _pointerCanvas = pointerRect.GetComponent<Canvas>();
            if (_pointerCanvas == null)
            {
                _pointerCanvas = pointerRect.gameObject.AddComponent<Canvas>();
            }

            if (rootCanvas != null)
            {
                _pointerCanvas.sortingLayerID = rootCanvas.sortingLayerID;
            }

            _pointerCanvas.overrideSorting = true;
            _pointerCanvas.sortingOrder = sortingOrderOnTop;

            pointerRect.gameObject.SetActive(false);

            // Ensure hand pointer doesn't block raycasts to buttons underneath
            var pointerImage = pointerRect.GetComponent<UnityEngine.UI.Image>();
            if (pointerImage != null)
            {
                pointerImage.raycastTarget = false;
                Debug.Log("[TutorialHandPointer] Awake: Set hand pointer raycastTarget = false to allow clicks through");
            }

            var childImages = pointerRect.GetComponentsInChildren<UnityEngine.UI.Image>(true);
            foreach (var img in childImages)
            {
                img.raycastTarget = false;
            }
            Debug.Log($"[TutorialHandPointer] Awake: Disabled raycastTarget on {childImages.Length} Image components");
            var canvasGroup = pointerRect.GetComponent<UnityEngine.CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.blocksRaycasts = false;
                Debug.Log("[TutorialHandPointer] Awake: Set CanvasGroup blocksRaycasts = false to allow clicks through");
            }
        }

        ServiceLocator.Register<ITutorialPointer>(this);
    }

    private void OnDestroy()
    {
        ServiceLocator.Unregister<ITutorialPointer>();
    }

    public void PointTo(RectTransform target, Vector2 offset)
    {
        Debug.Log($"[TutorialHandPointer] PointTo called - target={(target != null ? target.name : "null")}, offset={offset}, hasResolver={_targetResolver != null}");

        if (target == null)
        {
            Debug.LogWarning("[TutorialHandPointer] PointTo: target is null, aborting");
            return;
        }

        _worldTarget = null;

        if (pointerRect != null)
        {
            if (pointerRect.parent != null && !pointerRect.parent.gameObject.activeInHierarchy)
            {
                Debug.LogWarning($"[TutorialHandPointer] PointTo: pointerRect parent is not active! Parent={pointerRect.parent.name}");
            }

            pointerRect.gameObject.SetActive(true);
            Debug.Log($"[TutorialHandPointer] PointTo: pointerRect activated - activeSelf={pointerRect.gameObject.activeSelf}, activeInHierarchy={pointerRect.gameObject.activeInHierarchy}");
        }
        else
        {
            Debug.LogWarning("[TutorialHandPointer] PointTo: pointerRect is null!");
        }

        if (_target == target && pointerRect != null && pointerRect.gameObject.activeSelf)
        {
            Debug.Log("[TutorialHandPointer] PointTo: target unchanged, only updating offset");
            _offset = offset;
            return;
        }

        _target = target;
        _offset = offset;

        if (pointerRect == null || _canvasRect == null || rootCanvas == null)
        {
            Debug.LogWarning($"[TutorialHandPointer] PointTo: Missing references - pointerRect={pointerRect != null}, canvasRect={_canvasRect != null}, rootCanvas={rootCanvas != null}");
            return;
        }

        Debug.Log($"[TutorialHandPointer] PointTo: Setup complete - _target={(_target != null ? _target.name : "null")}, offset={_offset}, hasResolver={_targetResolver != null}");
        _swayTime = 0f;
    }

    public void PointTo(RectTransform target, Vector2 offset, System.Func<RectTransform> targetResolver)
    {
        Debug.Log($"[TutorialHandPointer] PointTo with resolver called - target={(target != null ? target.name : "null")}");
        _targetResolver = targetResolver;
        PointTo(target, offset);
        Debug.Log($"[TutorialHandPointer] PointTo with resolver done - _target={(_target != null ? _target.name : "null")}, _targetResolver={_targetResolver != null}");
    }

    public void PointToWorld(Transform worldTarget, Vector3 worldOffset)
    {
        Debug.Log("Hotel Tutorial : Pointoworld called" + (worldTarget != null ? worldTarget.name : "null"));
        if (worldTarget == null)
            return;
        Debug.Log("Hotel Tutorial : Pointoworld execute");
        _target = null;
        _worldTarget = worldTarget;
        _worldOffset = worldOffset;

        if (pointerRect != null)
        {
            pointerRect.gameObject.SetActive(true);
            Debug.Log("Hotel Tutorial : Pointer activated");
        }

        if (pointerRect == null || _canvasRect == null || rootCanvas == null)
        {
            Debug.LogWarning("[TutorialHandPointer] PointToWorld gagal: pointerRect atau canvas belum siap.");
            return;
        }
        Debug.Log("Hotel Tutorial : Pointer and Canvas ready");
        _swayTime = 0f;
    }

    public void Hide()
    {
        Debug.Log("Hotel Tutorial : Hide called");
        _target = null;
        _worldTarget = null;
        _targetResolver = null;
        if (pointerRect != null)
        {
            Debug.Log("Hotel Tutorial : Pointer deactivated");
            pointerRect.gameObject.SetActive(false);
        }
    }

    private void LateUpdate()
    {

        if (pointerRect == null)
        {
            return;
        }

        if (!pointerRect.gameObject.activeSelf)
        {
            return;
        }

        // Try to resolve target if resolver is available and target is null
        if (_targetResolver != null && _target == null && _worldTarget == null)
        {
            _target = _targetResolver();
            if (_target == null)
            {
                return;
            }
        }

        // Check if we have the necessary references
        if ((_target == null && _worldTarget == null) ||
            _canvasRect == null ||
            rootCanvas == null)
        {
            return;
        }

        Vector2 localPoint;

        // =============================
        // UI TARGET (RectTransform)
        // =============================
        if (_target != null)
        {
            Camera cam = rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : rootCanvas.worldCamera;
            Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(cam, _target.position);

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    _canvasRect,
                    screenPos,
                    cam,
                    out localPoint))
            {
                Debug.LogWarning($"[TutorialHandPointer] LateUpdate: ScreenPointToLocalPointInRectangle failed for target {_target.name}");
                return;
            }
        }
        // =============================
        // WORLD TARGET (Transform)
        // =============================
        else if (_worldTarget != null)
        {
            Camera worldCam = Camera.main;

            if (worldCam == null)
                return;

            Vector3 worldPos = _worldTarget.position + _worldOffset;
            Vector3 screenPos3D = worldCam.WorldToScreenPoint(worldPos);

            if (screenPos3D.z < 0)
            {
                pointerRect.gameObject.SetActive(false);
                return;
            }

            Vector2 screenPos = screenPos3D;

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    _canvasRect,
                    screenPos,
                    null,
                    out localPoint))
                return;
        }
        else
        {
            return;
        }

        // =============================
        // SWAY ANIMATION
        // =============================
        _swayTime += Time.deltaTime;

        float amplitude = (_worldTarget != null ? worldSwayAmplitude : swayAmplitude);
        float speed = (_worldTarget != null ? worldSwaySpeed : swaySpeed);

        float baseDir = -1f;
        if (Mathf.Abs(_offset.x) > 0.01f)
        {
            baseDir = -Mathf.Sign(_offset.x);
        }

        float sway = Mathf.Sin(_swayTime * speed) * amplitude;
        Vector2 swayOffset = new Vector2(sway * baseDir, 0f);

        Vector2 desired = localPoint + _offset + swayOffset;

        pointerRect.anchoredPosition = Vector2.SmoothDamp(
            pointerRect.anchoredPosition,
            desired,
            ref _velocity,
            followSmoothTime);
    }

}
