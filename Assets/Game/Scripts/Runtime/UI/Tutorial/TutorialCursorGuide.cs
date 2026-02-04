using DG.Tweening;
using UnityEngine;

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

    private bool _isPlaying;
    private bool _canCancel;
    private float _elapsed;
    private Vector3 _lockMousePosition;
    public void PlayGuideToTarget(RectTransform target)
    {
        if (cursorIcon == null || startPoint == null || target == null)
        {
            Debug.LogWarning("TutorialCursorGuide: reference belum lengkap (cursorIcon/startPoint/target).");
            return;
        }

        _isPlaying = true;
        _canCancel = false;
        _elapsed = 0f;
        _lockMousePosition = Input.mousePosition;

        cursorIcon.gameObject.SetActive(true);
        cursorIcon.DOKill();

        cursorIcon.position = startPoint.position;

        cursorIcon.DOMove(target.position, moveDuration)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                {
                    StopGuide();
                }
            });
    }

    private void Update()
    {
        if (!_isPlaying)
            return;

        _elapsed += Time.unscaledDeltaTime;

        if (!_canCancel && _elapsed >= lockDuration)
        {
            _canCancel = true;
            _lockMousePosition = Input.mousePosition;
        }

        if (_canCancel)
        {
            Vector3 delta = Input.mousePosition - _lockMousePosition;
            if (delta.sqrMagnitude > cancelMoveThreshold * cancelMoveThreshold)
            {
                StopGuide();
            }
        }
    }

    public void StopGuide()
    {
        _isPlaying = false;
        cursorIcon.DOKill();
        if (cursorIcon != null)
        {
            cursorIcon.gameObject.SetActive(false);
        }
    }
}
