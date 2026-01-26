using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Spine.Unity;

public class PomodoroUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject pomodoroPanel;
    [SerializeField] private PomodoroPhaseManager phaseManager;
    [SerializeField] private PomodoroMiniUI miniUI;
    [SerializeField] private PumpkinCarUIAnimator pumpkinAnim;

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

    [Header("Mushroom Animation")]
    [SerializeField] private SkeletonGraphic mushroomSkeletonGraphic;
    [SerializeField] private string runningAnimationName = "running";
    [SerializeField] private string idleAnimationName = "idle";

    [Header("Reward System")]
    [SerializeField] private Animator rewardBoxAnimator;
    [SerializeField] private GameObject rewardBoxPanel;
    [SerializeField] private TextMeshProUGUI rewardAmountText;
    [SerializeField] private int minGoldReward = 20;
    [SerializeField] private int maxGoldReward = 100;

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

        // Subscribe to Pomodoro completion event
        if (phaseManager != null)
        {
            phaseManager.OnPomodoroCompleted += OnPomodoroCompleted;
        }

        // Hide reward box panel initially
        if (rewardBoxPanel != null)
        {
            rewardBoxPanel.SetActive(false);
        }

        // Set initial play button sprite
        UpdatePlayButtonSprite();
        UpdateDisplay();
        UpdateMushroomAnimation();
    }

    private void Update()
    {
        if (phaseManager != null)
        {
            // Always update display to show real-time countdown
            UpdateDisplay();
            UpdateMushroomAnimation();
        }
    }

    private void OnDestroy()
    {
        RemoveButtonListeners();

        // Unsubscribe from events
        if (phaseManager != null)
        {
            phaseManager.OnPomodoroCompleted -= OnPomodoroCompleted;
        }
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
            // miniUI.Hide();
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

            // start pumpkin car animation
            pumpkinAnim.Play();
        }
        else
        {
            phaseManager.PauseTimer();
            Debug.Log("Timer paused");

            // stop pumpkin car animation
            pumpkinAnim.Pause();
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
            pumpkinAnim.ResetAnim();
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

    private void UpdateMushroomAnimation()
    {
        if (mushroomSkeletonGraphic == null || phaseManager == null)
            return;

        // Play "running" animation ONLY if timer is running AND in Focus phase
        // Play "idle" animation if timer is idle/paused OR in break phase
        bool shouldRun = false;

        if (phaseManager.IsRunning())
        {
            PomodoroPhase currentPhase = phaseManager.GetCurrentPhase();
            if (currentPhase != null && currentPhase.phaseType == PhaseType.Focus)
            {
                shouldRun = true;
            }
        }

        // Get current animation name
        var currentTrack = mushroomSkeletonGraphic.AnimationState.GetCurrent(0);
        string currentAnimName = currentTrack != null ? currentTrack.Animation.Name : "";

        if (shouldRun)
        {
            // Play running animation if not already playing
            if (currentAnimName != runningAnimationName)
            {
                mushroomSkeletonGraphic.AnimationState.SetAnimation(0, runningAnimationName, true);
            }
        }
        else
        {
            // Play idle animation if not already playing
            if (currentAnimName != idleAnimationName)
            {
                mushroomSkeletonGraphic.AnimationState.SetAnimation(0, idleAnimationName, true);
            }
        }
    }

    private void OnPomodoroCompleted()
    {
        Debug.Log("Pomodoro cycle completed! Showing reward...");

        // Stop the timer and reset playing state
        isPlaying = false;
        UpdatePlayButtonSprite();

        // Generate random gold reward
        int goldReward = Random.Range(minGoldReward, maxGoldReward + 1);

        // Add gold to player's inventory
        CoinManager.AddCoins(goldReward);

        Debug.Log($"Rewarded {goldReward} gold coins!");

        // Show reward box with animation
        ShowRewardBox(goldReward);
    }

    private void ShowRewardBox(int goldAmount)
    {
        if (rewardBoxPanel == null)
        {
            Debug.LogWarning("Reward box panel is not assigned!");
            return;
        }

        // Show reward panel
        rewardBoxPanel.SetActive(true);

        // Update reward amount text
        if (rewardAmountText != null)
        {
            rewardAmountText.text = goldAmount.ToString() + " GOLD";
        }

        // Trigger reward box animation
        if (rewardBoxAnimator != null)
        {
            rewardBoxAnimator.SetTrigger("Show");
        }

        OnRestartButtonClicked();
        Debug.Log($"Showing reward box with {goldAmount} gold");
    }

    public void CloseRewardBox()
    {
        if (rewardBoxPanel != null)
        {
            // Start coroutine to wait for Open animation to complete before hiding
            StartCoroutine(CloseRewardBoxCoroutine());
        }
    }

    private IEnumerator CloseRewardBoxCoroutine()
    {
        // Trigger Open animation
        if (rewardBoxAnimator != null)
        {
            rewardBoxAnimator.SetTrigger("Open");

            // Get the current animation state info
            AnimatorStateInfo stateInfo = rewardBoxAnimator.GetCurrentAnimatorStateInfo(0);

            // Wait for the animator to transition to the new state
            yield return null;

            // Get the updated state info after transition
            stateInfo = rewardBoxAnimator.GetCurrentAnimatorStateInfo(0);

            // Wait for the duration of the Open animation + 1 extra seconds
            float animationLength = stateInfo.length;
            yield return new WaitForSeconds(animationLength + 1f);
        }

        // Trigger hide animation if available
        if (rewardBoxAnimator != null)
        {
            rewardBoxAnimator.SetTrigger("Hide");
        }

        // Hide panel after animation (or immediately if no animator)
        rewardBoxPanel.SetActive(false);
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
