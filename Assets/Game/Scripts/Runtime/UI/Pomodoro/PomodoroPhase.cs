using System;
using UnityEngine;

[Serializable]
public class PomodoroPhase
{
    [Header("Phase Info")]
    public string phaseName;
    public PhaseType phaseType;

    [Header("Slider Range")]
    [Range(0f, 1f)]
    public float sliderStart;
    [Range(0f, 1f)]
    public float sliderEnd;

    [Header("Duration")]
    public int durationMinutes;

    public PomodoroPhase(string name, PhaseType type, float start, float end, int duration)
    {
        phaseName = name;
        phaseType = type;
        sliderStart = start;
        sliderEnd = end;
        durationMinutes = duration;
    }

    public float GetSliderRange()
    {
        return sliderEnd - sliderStart;
    }

    public bool IsInRange(float sliderValue)
    {
        return sliderValue >= sliderStart && sliderValue <= sliderEnd;
    }

    public float GetPhaseProgress(float sliderValue)
    {
        if (!IsInRange(sliderValue))
            return 0f;

        float range = GetSliderRange();
        if (range <= 0)
            return 0f;

        return (sliderValue - sliderStart) / range;
    }

    public int GetRemainingMinutes(float sliderValue)
    {
        float progress = GetPhaseProgress(sliderValue);
        return Mathf.CeilToInt(durationMinutes * (1f - progress));
    }

    public int GetElapsedMinutes(float sliderValue)
    {
        float progress = GetPhaseProgress(sliderValue);
        return Mathf.FloorToInt(durationMinutes * progress);
    }
}

public enum PhaseType
{
    Focus,
    ShortBreak,
    LongBreak
}
