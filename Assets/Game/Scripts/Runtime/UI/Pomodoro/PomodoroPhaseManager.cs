using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PomodoroPhaseManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Slider progressSlider;

    [Header("Phase Configuration")]
    [SerializeField] private List<PomodoroPhase> phases = new List<PomodoroPhase>();

    private PomodoroPhase currentPhase;
    private int currentPhaseIndex = 0;

    // Timer state
    private bool isRunning = false;
    private float elapsedTimeInPhase = 0f; // in seconds
    private float totalPhaseTimeInSeconds = 0f;

    // Events
    public event Action<PomodoroPhase> OnPhaseChangeEvent;
    public event Action OnPomodoroCompleted;

    // private void Awake()
    // {
    //     InitializeDefaultPhases();
    // }

    private void Start()
    {
        if (progressSlider != null)
        {
            progressSlider.minValue = 0f;
            progressSlider.maxValue = 1f;
            progressSlider.value = 0f;
            progressSlider.onValueChanged.AddListener(OnSliderValueChanged);
        }

        UpdateCurrentPhase();
        InitializePhaseTimer();
    }

    private void Update()
    {
        if (isRunning && currentPhase != null)
        {
            UpdateTimer();
        }
    }

    private void OnDestroy()
    {
        if (progressSlider != null)
        {
            progressSlider.onValueChanged.RemoveListener(OnSliderValueChanged);
        }
    }

    private void InitializeDefaultPhases()
    {
        if (phases.Count > 0)
            return;
        
        phases = new List<PomodoroPhase>
        {
            new PomodoroPhase("Focus 1", PhaseType.Focus, 0.00f, 0.125f, 25),
            
            new PomodoroPhase("Break 1", PhaseType.ShortBreak, 0.125f, 0.25f, 5),

            // Phase 3: Focus (25 min)
            new PomodoroPhase("Focus 2", PhaseType.Focus, 0.25f, 0.375f, 25),

            // Phase 4: Short Break (5 min)
            new PomodoroPhase("Break 2", PhaseType.ShortBreak, 0.375f, 0.5f, 5),

            // Phase 5: Focus (25 min)
            new PomodoroPhase("Focus 3", PhaseType.Focus, 0.5f, 0.625f, 25),

            // Phase 6: Short Break (5 min)
            new PomodoroPhase("Break 3", PhaseType.ShortBreak, 0.625f, 0.75f, 5),

            // Phase 7: Focus (25 min)
            new PomodoroPhase("Focus 4", PhaseType.Focus, 0.75f, 0.875f, 25),

            // Phase 8: Long Break (30 min)
            new PomodoroPhase("Long Break", PhaseType.LongBreak, 0.875f, 1.0f, 30)
        };
    }

    private void OnSliderValueChanged(float value)
    {
        // Only update if slider is changed manually (not by timer)
        if (!isRunning)
        {
            UpdateCurrentPhase();
            InitializePhaseTimer();
        }
    }

    private void UpdateCurrentPhase()
    {
        if (progressSlider == null)
            return;

        float sliderValue = progressSlider.value;
        PomodoroPhase newPhase = GetPhaseAtSliderValue(sliderValue);

        if (newPhase != currentPhase)
        {
            currentPhase = newPhase;
            currentPhaseIndex = phases.IndexOf(currentPhase);
            OnPhaseChanged(currentPhase);
        }
    }

    private void InitializePhaseTimer()
    {
        if (currentPhase == null)
            return;

        // Calculate total time for current phase in seconds
        totalPhaseTimeInSeconds = currentPhase.durationMinutes * 60f;

        // Calculate elapsed time based on current slider position within the phase
        float phaseProgress = currentPhase.GetPhaseProgress(GetSliderValue());
        elapsedTimeInPhase = phaseProgress * totalPhaseTimeInSeconds;
    }

    private void UpdateTimer()
    {
        // Increment elapsed time
        elapsedTimeInPhase += Time.deltaTime;

        // Check if phase is completed
        if (elapsedTimeInPhase >= totalPhaseTimeInSeconds)
        {
            OnPhaseCompleted();
            return;
        }

        // Calculate progress within current phase (0-1)
        float phaseProgress = Mathf.Clamp01(elapsedTimeInPhase / totalPhaseTimeInSeconds);

        // Calculate slider value based on phase progress
        float phaseRange = currentPhase.GetSliderRange();
        float newSliderValue = currentPhase.sliderStart + (phaseProgress * phaseRange);

        // Update slider (this won't trigger OnSliderValueChanged because isRunning is true)
        if (progressSlider != null)
        {
            progressSlider.value = newSliderValue;
        }
    }

    private void OnPhaseCompleted()
    {
        // Move to next phase
        if (currentPhaseIndex < phases.Count - 1)
        {
            currentPhaseIndex++;
            PomodoroPhase nextPhase = phases[currentPhaseIndex];

            // Update current phase
            currentPhase = nextPhase;

            // Set slider to start of next phase
            if (progressSlider != null)
            {
                progressSlider.value = nextPhase.sliderStart;
            }

            // Reset timer for new phase
            elapsedTimeInPhase = 0f;
            totalPhaseTimeInSeconds = nextPhase.durationMinutes * 60f;

            OnPhaseChanged(nextPhase);
        }
        else
        {
            // All phases completed
            isRunning = false;
            OnPomodoroCompleted?.Invoke();
            Debug.Log("Pomodoro cycle completed!");
        }
    }

    public PomodoroPhase GetPhaseAtSliderValue(float sliderValue)
    {
        foreach (var phase in phases)
        {
            if (phase.IsInRange(sliderValue))
                return phase;
        }

        // Fallback to first phase
        return phases.Count > 0 ? phases[0] : null;
    }

    public PomodoroPhase GetCurrentPhase()
    {
        return currentPhase;
    }

    public int GetCurrentPhaseIndex()
    {
        return currentPhaseIndex;
    }

    public int GetTotalPhases()
    {
        return phases.Count;
    }

    public List<PomodoroPhase> GetAllPhases()
    {
        return phases;
    }

    public void SetSliderValue(float value)
    {
        if (progressSlider != null)
        {
            progressSlider.value = Mathf.Clamp01(value);
        }
    }

    public float GetSliderValue()
    {
        return progressSlider != null ? progressSlider.value : 0f;
    }

    public void AdvanceToNextPhase()
    {
        if (currentPhaseIndex < phases.Count - 1)
        {
            currentPhaseIndex++;
            PomodoroPhase nextPhase = phases[currentPhaseIndex];
            currentPhase = nextPhase;
            SetSliderValue(nextPhase.sliderStart);
            InitializePhaseTimer();
            OnPhaseChanged(nextPhase);
        }
        else
        {
            // Reset to beginning
            currentPhaseIndex = 0;
            currentPhase = phases.Count > 0 ? phases[0] : null;
            SetSliderValue(0f);
            InitializePhaseTimer();
            if (currentPhase != null)
            {
                OnPhaseChanged(currentPhase);
            }
        }
    }

    public void ResetToStart()
    {
        currentPhaseIndex = 0;
        SetSliderValue(0f);
        isRunning = false;
        InitializePhaseTimer();
    }

    // Timer control methods
    public void StartTimer()
    {
        isRunning = true;
    }

    public void PauseTimer()
    {
        isRunning = false;
    }

    public void StopTimer()
    {
        isRunning = false;
        ResetToStart();
    }

    public bool IsRunning()
    {
        return isRunning;
    }

    public string GetCurrentPhaseInfo()
    {
        if (currentPhase == null)
            return "No phase active";

        float sliderValue = GetSliderValue();
        int remainingMinutes = currentPhase.GetRemainingMinutes(sliderValue);
        int elapsedMinutes = currentPhase.GetElapsedMinutes(sliderValue);
        float progress = currentPhase.GetPhaseProgress(sliderValue) * 100f;

        return $"{currentPhase.phaseName}\n" +
               $"Progress: {progress:F1}%\n" +
               $"Elapsed: {elapsedMinutes} min\n" +
               $"Remaining: {remainingMinutes} min";
    }

    public int GetRemainingMinutesInCurrentPhase()
    {
        if (currentPhase == null)
            return 0;

        return currentPhase.GetRemainingMinutes(GetSliderValue());
    }

    public int GetElapsedMinutesInCurrentPhase()
    {
        if (currentPhase == null)
            return 0;

        return currentPhase.GetElapsedMinutes(GetSliderValue());
    }

    public int GetRemainingSecondsInCurrentPhase()
    {
        if (currentPhase == null)
            return 0;

        float remainingTime = totalPhaseTimeInSeconds - elapsedTimeInPhase;
        return Mathf.CeilToInt(Mathf.Max(0, remainingTime));
    }

    public int GetElapsedSecondsInCurrentPhase()
    {
        if (currentPhase == null)
            return 0;

        return Mathf.FloorToInt(elapsedTimeInPhase);
    }

    public float GetCurrentPhaseProgress()
    {
        if (currentPhase == null)
            return 0f;

        return currentPhase.GetPhaseProgress(GetSliderValue());
    }

    private void OnPhaseChanged(PomodoroPhase newPhase)
    {
        Debug.Log($"Phase changed to: {newPhase.phaseName} ({newPhase.phaseType})");

        // TODO: Add event or callback for phase change
        // You can trigger UI updates, sound effects, notifications, etc.
    }

    // Custom phase configuration methods
    public void SetPhaseSliderRange(int phaseIndex, float start, float end)
    {
        if (phaseIndex >= 0 && phaseIndex < phases.Count)
        {
            phases[phaseIndex].sliderStart = Mathf.Clamp01(start);
            phases[phaseIndex].sliderEnd = Mathf.Clamp01(end);
        }
    }

    public void SetPhaseDuration(int phaseIndex, int durationMinutes)
    {
        if (phaseIndex >= 0 && phaseIndex < phases.Count)
        {
            phases[phaseIndex].durationMinutes = Mathf.Max(1, durationMinutes);
        }
    }

    public void ClearPhases()
    {
        phases.Clear();
    }

    public void AddPhase(string name, PhaseType type, float start, float end, int duration)
    {
        phases.Add(new PomodoroPhase(name, type, start, end, duration));
    }
}
