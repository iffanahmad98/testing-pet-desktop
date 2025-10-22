using UnityEngine;
using UnityEngine.UI;

public class PomodoroMiniUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject miniPanel;
    [SerializeField] private PomodoroPhaseManager phaseManager;

    [Header("Display Elements")]
    [SerializeField] private Image phaseIconImage;
    [SerializeField] private Image progressFillImage;
    [SerializeField] private Image phaseTypeImage;

    [Header("Phase Icons")]
    [SerializeField] private Sprite focusIcon;
    [SerializeField] private Sprite breakIcon;

    [Header("Phase Type Images")]
    [SerializeField] private Sprite focusTypeImage;
    [SerializeField] private Sprite breakTypeImage;

    [Header("Settings")]
    [SerializeField] private bool showOnStart = false;

    [Header("Animation")]
    [SerializeField] private Animator miniUIAnimator;

    private void Start()
    {
        // Get phase manager if not assigned
        if (phaseManager == null)
        {
            phaseManager = FindObjectOfType<PomodoroPhaseManager>();
        }

        // Subscribe to phase manager timer state
        if (phaseManager != null)
        {
            // Check initial state
            UpdateAnimatorState();
        }

        // Show or hide panel based on settings
        if (miniPanel != null)
        {
            miniPanel.SetActive(showOnStart);
        }

        UpdateDisplay();
    }

    private void Update()
    {
        if (miniPanel != null && miniPanel.activeSelf && phaseManager != null)
        {
            UpdateDisplay();
            UpdateAnimatorState();
        }
    }

    private void UpdateDisplay()
    {
        if (phaseManager == null)
            return;

        PomodoroPhase currentPhase = phaseManager.GetCurrentPhase();
        if (currentPhase == null)
            return;

        // Update progress fill (countdown from 1 to 0)
        if (progressFillImage != null)
        {
            float progress = phaseManager.GetCurrentPhaseProgress();
            progressFillImage.fillAmount = 1f - progress;
        }

        // Update phase icon based on phase type
        if (phaseIconImage != null)
        {
            UpdatePhaseIcon(currentPhase.phaseType);
        }

        // Update phase type image based on phase type
        if (phaseTypeImage != null)
        {
            UpdatePhaseTypeImage(currentPhase.phaseType);
        }
    }

    private void UpdatePhaseIcon(PhaseType phaseType)
    {
        if (phaseIconImage == null)
            return;

        switch (phaseType)
        {
            case PhaseType.Focus:
                if (focusIcon != null)
                {
                    phaseIconImage.sprite = focusIcon;
                }
                break;

            case PhaseType.ShortBreak:
            case PhaseType.LongBreak:
                if (breakIcon != null)
                {
                    phaseIconImage.sprite = breakIcon;
                }
                break;
        }
    }

    private void UpdatePhaseTypeImage(PhaseType phaseType)
    {
        if (phaseTypeImage == null)
            return;

        switch (phaseType)
        {
            case PhaseType.Focus:
                if (focusTypeImage != null)
                {
                    phaseTypeImage.sprite = focusTypeImage;
                }
                break;

            case PhaseType.ShortBreak:
            case PhaseType.LongBreak:
                if (breakTypeImage != null)
                {
                    phaseTypeImage.sprite = breakTypeImage;
                }
                break;
        }
    }

    private void UpdateAnimatorState()
    {
        if (miniUIAnimator == null || phaseManager == null)
            return;

        // Set "Action" parameter to true ONLY if timer is running AND in Focus phase
        // Set to false if timer is idle/paused OR in break phase
        bool isActive = false;

        if (phaseManager.IsRunning())
        {
            PomodoroPhase currentPhase = phaseManager.GetCurrentPhase();
            if (currentPhase != null && currentPhase.phaseType == PhaseType.Focus)
            {
                isActive = true;
            }
        }

        miniUIAnimator.SetBool("Action", isActive);
    }

    // Public methods to show/hide mini UI
    public void Show()
    {
        if (miniPanel != null)
        {
            miniPanel.SetActive(true);
            UpdateDisplay();
        }
    }

    public void Hide()
    {
        if (miniPanel != null)
        {
            miniPanel.SetActive(false);
        }
    }

    public void Toggle()
    {
        if (miniPanel != null)
        {
            miniPanel.SetActive(!miniPanel.activeSelf);
            if (miniPanel.activeSelf)
            {
                UpdateDisplay();
            }
        }
    }

    public bool IsShowing()
    {
        return miniPanel != null && miniPanel.activeSelf;
    }

    public void SetPhaseManager(PomodoroPhaseManager manager)
    {
        phaseManager = manager;
        UpdateDisplay();
    }
}
