using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PomodoroUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject pomodoroPanel;
    [SerializeField] private PomodoroPhaseManager phaseManager;
    [SerializeField] private PomodoroMiniUI miniUI;

    [Header("Display Elements")]
    [SerializeField] private TextMeshProUGUI phaseNameText;
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private TextMeshProUGUI phaseInfoText;
    [SerializeField] private Image phaseProgressFillImage;

    [Header("Control Buttons")]
    [SerializeField] private Button pumpkinButton;
    [SerializeField] private Button playButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button skipButton;
    [SerializeField] private Button minMaxButton;
    [SerializeField] private Button closeButton;

    [Header("Button Sprites")]
    [SerializeField] private Sprite playSprite;
    [SerializeField] private Sprite pauseSprite;

    private bool isMinimized = true;
    private bool isPlaying = false;

    private void Start()
    {
        SetupButtonListeners();

        // Hide panel by default
        if (pomodoroPanel != null)
        {
            pomodoroPanel.SetActive(false);
        }

        // Get phase manager if not assigned
        if (phaseManager == null)
        {
            phaseManager = GetComponent<PomodoroPhaseManager>();
            if (phaseManager == null)
            {
                phaseManager = pomodoroPanel?.GetComponentInChildren<PomodoroPhaseManager>();
            }
        }

        // Set initial play button sprite
        UpdatePlayButtonSprite();
        UpdateDisplay();
    }

    private void Update()
    {
        if (phaseManager != null)
        {
            // Always update display to show real-time countdown
            UpdateDisplay();
        }
    }

    private void OnDestroy()
    {
        RemoveButtonListeners();
    }

    private void SetupButtonListeners()
    {
        if (pumpkinButton != null)
            pumpkinButton.onClick.AddListener(OnPumpkinButtonClicked);

        if (playButton != null)
            playButton.onClick.AddListener(OnPlayButtonClicked);

        if (restartButton != null)
            restartButton.onClick.AddListener(OnRestartButtonClicked);

        if (skipButton != null)
            skipButton.onClick.AddListener(OnSkipButtonClicked);

        if (minMaxButton != null)
            minMaxButton.onClick.AddListener(OnMinMaxButtonClicked);

        if (closeButton != null)
            closeButton.onClick.AddListener(OnCloseButtonClicked);
    }

    private void RemoveButtonListeners()
    {
        if (pumpkinButton != null)
            pumpkinButton.onClick.RemoveListener(OnPumpkinButtonClicked);

        if (playButton != null)
            playButton.onClick.RemoveListener(OnPlayButtonClicked);

        if (restartButton != null)
            restartButton.onClick.RemoveListener(OnRestartButtonClicked);

        if (skipButton != null)
            skipButton.onClick.RemoveListener(OnSkipButtonClicked);

        if (minMaxButton != null)
            minMaxButton.onClick.RemoveListener(OnMinMaxButtonClicked);

        if (closeButton != null)
            closeButton.onClick.RemoveListener(OnCloseButtonClicked);
    }

    private void OnPumpkinButtonClicked()
    {
        if (pomodoroPanel != null)
        {
            pomodoroPanel.SetActive(true);
        }

        // Hide mini UI when main panel opens
        if (miniUI != null)
        {
            miniUI.Hide();
        }

        Debug.Log("Pumpkin button clicked - Opening Pomodoro Panel");
    }

    private void OnPlayButtonClicked()
    {
        if (phaseManager == null)
            return;

        isPlaying = !isPlaying;

        if (isPlaying)
        {
            phaseManager.StartTimer();
            Debug.Log("Timer started");
        }
        else
        {
            phaseManager.PauseTimer();
            Debug.Log("Timer paused");
        }

        UpdatePlayButtonSprite();
        UpdateDisplay();
    }

    private void UpdatePlayButtonSprite()
    {
        if (playButton == null)
            return;

        Image buttonImage = playButton.GetComponent<Image>();
        if (buttonImage == null)
            return;

        if (isPlaying)
        {
            // When playing, show pause sprite
            if (pauseSprite != null)
            {
                buttonImage.sprite = pauseSprite;
            }
        }
        else
        {
            // When paused, show play sprite
            if (playSprite != null)
            {
                buttonImage.sprite = playSprite;
            }
        }
    }

    private void OnRestartButtonClicked()
    {
        if (phaseManager != null)
        {
            phaseManager.StopTimer();
            isPlaying = false;
            UpdatePlayButtonSprite();
            UpdateDisplay();
        }

        Debug.Log("Restart button clicked - Reset to start");
    }

    private void OnSkipButtonClicked()
    {
        if (phaseManager != null)
        {
            phaseManager.AdvanceToNextPhase();
            UpdateDisplay();

            PomodoroPhase currentPhase = phaseManager.GetCurrentPhase();
            if (currentPhase != null)
            {
                int phaseIndex = phaseManager.GetCurrentPhaseIndex() + 1;
                int totalPhases = phaseManager.GetTotalPhases();
                Debug.Log($"Skip button clicked - Advanced to Phase {phaseIndex}/{totalPhases}: {currentPhase.phaseName} ({currentPhase.phaseType}) - {currentPhase.durationMinutes} minutes");
            }
            else
            {
                Debug.Log("Skip button clicked - Advanced to next phase");
            }
        }
    }

    private void OnMinMaxButtonClicked()
    {
        isMinimized = !isMinimized;

        if (pomodoroPanel != null)
        {
            RectTransform panelRect = pomodoroPanel.GetComponent<RectTransform>();
            if (panelRect != null)
            {
                if (isMinimized)
                {
                    // Minimize: set scale to 0.7:0.7
                    panelRect.localScale = new Vector3(0.7f, 0.7f, 1f);
                    Debug.Log("MinMax button clicked - Panel minimized to scale 0.7:0.7");
                }
                else
                {
                    // Maximize: set scale to 1:1 (full screen)
                    panelRect.localScale = new Vector3(1f, 1f, 1f);
                    Debug.Log("MinMax button clicked - Panel maximized to scale 1:1");
                }
            }
        }
    }

    private void OnCloseButtonClicked()
    {
        if (pomodoroPanel != null)
        {
            pomodoroPanel.SetActive(false);
        }

        // Show mini UI when main panel closes
        if (miniUI != null)
        {
            miniUI.Show();
        }

        isPlaying = false;

        Debug.Log("Close button clicked - Closing Pomodoro Panel");
    }
    
    private void UpdateDisplay()
    {
        if (phaseManager == null)
            return;

        PomodoroPhase currentPhase = phaseManager.GetCurrentPhase();
        if (currentPhase == null)
            return;

        if (phaseNameText != null)
        {
            phaseNameText.text = currentPhase.phaseName;
        }

        if (timeText != null)
        {
            // Get remaining time in seconds for accurate countdown
            int totalRemainingSeconds = phaseManager.GetRemainingSecondsInCurrentPhase();
            int minutes = totalRemainingSeconds / 60;
            int seconds = totalRemainingSeconds % 60;
            timeText.text = $"{minutes:D2}:{seconds:D2}";
        }

        // Update phase progress fill image (countdown from 0.75 to 0)
        if (phaseProgressFillImage != null)
        {
            float progress = phaseManager.GetCurrentPhaseProgress();
            // Invert progress: start at 0.75, end at 0
            phaseProgressFillImage.fillAmount = 0.75f * (1f - progress);
        }

        if (phaseInfoText != null)
        {
            float progress = phaseManager.GetCurrentPhaseProgress() * 100f;
            int phaseIndex = phaseManager.GetCurrentPhaseIndex() + 1;
            int totalPhases = phaseManager.GetTotalPhases();

            phaseInfoText.text = $"Phase {phaseIndex}/{totalPhases}\n" +
                               $"Progress: {progress:F1}%\n" +
                               $"Type: {currentPhase.phaseType}";
        }
    }
    

    public PomodoroPhaseManager GetPhaseManager()
    {
        return phaseManager;
    }

    public bool IsPlaying()
    {
        return isPlaying;
    }

    public void SetPlaying(bool playing)
    {
        isPlaying = playing;
    }
}
