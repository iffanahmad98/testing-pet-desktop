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

    private RectTransform _canvasRect;
    private RectTransform _target;
    private Vector2 _offset;
    private Vector2 _velocity;
    private float _swayTime;

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
            pointerRect.gameObject.SetActive(false);
        }

        ServiceLocator.Register<ITutorialPointer>(this);
    }

    private void OnDestroy()
    {
        ServiceLocator.Unregister<ITutorialPointer>();
    }

    public void PointTo(RectTransform target, Vector2 offset)
    {
        if (target == null)
            return;

        if (_target == target && pointerRect != null && pointerRect.gameObject.activeSelf)
        {
            _offset = offset;
            return;
        }

        _target = target;
        _offset = offset;

        if (pointerRect == null || _canvasRect == null || rootCanvas == null)
            return;

        pointerRect.gameObject.SetActive(true);
        _swayTime = 0f;
    }

    public void Hide()
    {
        _target = null;
        if (pointerRect != null)
        {
            pointerRect.gameObject.SetActive(false);
        }
    }

    private void LateUpdate()
    {
        if (_target == null || pointerRect == null || _canvasRect == null || rootCanvas == null)
            return;

        var cam = rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null
            : rootCanvas.worldCamera;

        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(cam, _target.position);
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvasRect, screenPos, cam, out var localPoint))
        {
            _swayTime += Time.deltaTime;

            float baseDir = -1f;
            if (Mathf.Abs(_offset.x) > 0.01f)
            {
                baseDir = -Mathf.Sign(_offset.x);
            }

            float sway = Mathf.Sin(_swayTime * swaySpeed) * swayAmplitude;
            Vector2 swayOffset = new Vector2(sway * baseDir, 0f);

            Vector2 desired = localPoint + _offset + swayOffset;
            pointerRect.anchoredPosition = Vector2.SmoothDamp(pointerRect.anchoredPosition, desired, ref _velocity, followSmoothTime);
        }
    }
}
