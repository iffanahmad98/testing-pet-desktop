using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class TutorialCursorGuide : MonoBehaviour
{
    [Header("Cursor Visual")]
    [SerializeField] private RectTransform cursorIcon;
    [Tooltip("Posisi awal animasi cursor (misalnya dari luar layar atau dari tombol tertentu).")]
    [SerializeField] private RectTransform startPoint;

    [Header("Animation Settings")]
    [SerializeField] private float moveDuration = 1f;
    [SerializeField] private float lockDuration = 0.5f;
    [Tooltip("Seberapa jauh (dalam pixel layar) gerakan mouse yang dianggap sebagai 'bergerak' untuk membatalkan animasi.")]
    [SerializeField] private float cancelMoveThreshold = 10f;
    [Tooltip("Jika true, animasi akan berhenti ketika cursor digerakkan melewati threshold.")]
    [SerializeField] private bool cancelOnMouseMove = true;
    [Tooltip("Total durasi maksimal animasi cursor (dalam detik). Setelah ini animasi akan berhenti sendiri.")]
    [SerializeField] private float maxDuration = 20f;

    [Header("Cursor Manager Integration")]
    [Tooltip("Tipe cursor yang akan dipakai untuk animasi tutorial.")]
    [SerializeField] private CursorType tutorialCursorType = CursorType.Default;
    [Tooltip("Jika true, sistem cursor juga akan diganti saat animasi berjalan, lalu di-reset ketika animasi selesai.")]
    [SerializeField] private bool changeSystemCursor = true;

    private bool _isPlaying;
    private bool _canCancel;
    private float _elapsed;
    private float _totalElapsed;
    private Vector3 _lockMousePosition;

    private CursorManager _cursorManager;
    private Image _cursorImage;
    private RawImage _cursorRawImage;
    private readonly Dictionary<Texture2D, Sprite> _spriteCache = new Dictionary<Texture2D, Sprite>();

    private void Awake()
    {
        if (cursorIcon != null)
        {
            _cursorImage = cursorIcon.GetComponent<Image>();
            _cursorRawImage = cursorIcon.GetComponent<RawImage>();
        }

        TryResolveCursorManager();
    }

    private void TryResolveCursorManager()
    {
        if (_cursorManager != null)
            return;

        _cursorManager = ServiceLocator.Get<CursorManager>();
        if (_cursorManager == null)
        {
            Debug.LogWarning("TutorialCursorGuide: CursorManager tidak ditemukan di ServiceLocator.");
        }
    }

    public void PlayGuideToTarget(RectTransform target)
    {
        if (cursorIcon == null || startPoint == null || target == null)
        {
            Debug.LogWarning("TutorialCursorGuide: reference belum lengkap (cursorIcon/startPoint/target).");
            return;
        }

        TryResolveCursorManager();

        if (_cursorManager != null)
        {
            var tex = _cursorManager.GetTexture(tutorialCursorType);
            if (tex != null)
            {
                if (_cursorImage != null)
                {
                    _cursorImage.sprite = GetOrCreateSprite(tex);
                    _cursorImage.SetNativeSize();
                }
                else if (_cursorRawImage != null)
                {
                    _cursorRawImage.texture = tex;
                    _cursorRawImage.SetNativeSize();
                }
            }

            if (changeSystemCursor)
            {
                _cursorManager.Set(tutorialCursorType);
            }
        }

        _isPlaying = true;
        _canCancel = false;
        _elapsed = 0f;
        _totalElapsed = 0f;
        _lockMousePosition = Input.mousePosition;

        cursorIcon.gameObject.SetActive(true);
        cursorIcon.DOKill();

        cursorIcon.position = startPoint.position;

        cursorIcon.DOMove(target.position, moveDuration)
            .SetEase(Ease.OutQuad)
            .SetLoops(-1, LoopType.Yoyo);
    }

    private Sprite GetOrCreateSprite(Texture2D tex)
    {
        if (tex == null)
            return null;

        if (_spriteCache.TryGetValue(tex, out var sprite))
            return sprite;

        sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
        _spriteCache[tex] = sprite;
        return sprite;
    }

    private void Update()
    {
        if (!_isPlaying)
            return;

        float dt = Time.unscaledDeltaTime;
        _elapsed += dt;
        _totalElapsed += dt;

        if (!_canCancel && _elapsed >= lockDuration)
        {
            _canCancel = true;
            _lockMousePosition = Input.mousePosition;
        }

        if (_canCancel && cancelOnMouseMove)
        {
            Vector3 delta = Input.mousePosition - _lockMousePosition;
            if (delta.sqrMagnitude > cancelMoveThreshold * cancelMoveThreshold)
            {
                StopGuide();
            }
        }

        if (maxDuration > 0f && _totalElapsed >= maxDuration)
        {
            StopGuide();
        }
    }

    public void StopGuide()
    {
        _isPlaying = false;

        if (cursorIcon != null)
        {
            cursorIcon.DOKill();
            cursorIcon.gameObject.SetActive(false);
        }

        if (changeSystemCursor && _cursorManager != null)
        {
            _cursorManager.Reset();
        }
    }
}
