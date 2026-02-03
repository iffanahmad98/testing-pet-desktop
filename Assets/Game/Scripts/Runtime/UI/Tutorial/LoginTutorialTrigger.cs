using UnityEngine;
public class LoginTutorialTrigger : MonoBehaviour
{
    [Header("References")]
    public TutorialManager tutorialManager;
    private ITutorialService _tutorialService;

    [Header("Config")]
    [Tooltip("ID tutorial yang mau dicek waktu login. Harus sama dengan ID di TutorialManager.")]
    public string tutorialId = "Login";

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

        var alreadyDone = _tutorialService.HasCompleted(tutorialId);

        if (!alreadyDone)
        {
            var started = _tutorialService.TryStart(tutorialId);
            if (!started)
            {
                Debug.LogWarning($"LoginTutorialTrigger: gagal start tutorial dengan ID '{tutorialId}'. Cek konfigurasi TutorialManager.");
            }
        }
        else
        {
            Debug.Log($"Tutorial '{tutorialId}' sudah pernah diselesaikan, langsung lanjut gameplay.");
        }
    }
}
