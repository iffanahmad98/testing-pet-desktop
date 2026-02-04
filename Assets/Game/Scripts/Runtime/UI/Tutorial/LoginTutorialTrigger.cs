using UnityEngine;
public class LoginTutorialTrigger : MonoBehaviour
{
    [Header("References")]
    public TutorialManager tutorialManager;
    private ITutorialService _tutorialService;

    private void Awake()
    {
        _tutorialService = tutorialManager;
    }
    public void OnLoginSuccess()
    {
        if (_tutorialService == null)
        {
            Debug.LogWarning("LoginTutorialTrigger: TutorialService belum diinisialisasi (cek TutorialManager).");
            return;
        }

        var hasPending = _tutorialService.HasAnyPending();

        if (hasPending)
        {
            var started = _tutorialService.TryStartNext();
            if (!started)
            {
                Debug.LogWarning("LoginTutorialTrigger: gagal start tutorial berikutnya. Cek konfigurasi TutorialManager.");
            }
        }
        else
        {
            Debug.Log("Semua tutorial sudah pernah diselesaikan, langsung lanjut gameplay.");
        }
    }
}
