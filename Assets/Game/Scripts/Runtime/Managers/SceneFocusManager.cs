using UnityEngine;
using System.Collections;
using System.Collections.Generic;
/// <summary>
/// Manager to handle camera focus target when transitioning between scenes
/// </summary>
public static class SceneFocusManager
{
    public enum FocusTarget
    {
        None,
        Farm,
        Hotel
    }

    // Static variable to persist across scenes
    private static FocusTarget _targetFocus = FocusTarget.None;

    /// <summary>
    /// Set the target focus for the next scene
    /// </summary>
    public static void SetFocusTarget(FocusTarget target)
    {
        _targetFocus = target;
        Debug.Log($"SceneFocusManager: Focus target set to {target}");
    }

    /// <summary>
    /// Get and clear the target focus (should be called once when scene loads)
    /// </summary>
    public static FocusTarget GetAndClearFocusTarget()
    {
        FocusTarget target = _targetFocus;
        _targetFocus = FocusTarget.None; // Clear after reading
        Debug.Log($"SceneFocusManager: Retrieved focus target {target}, cleared to None");
        return target;
    }

    /// <summary>
    /// Get the current target focus without clearing
    /// </summary>
    public static FocusTarget GetFocusTarget()
    {
        return _targetFocus;
    }

    /// <summary>
    /// Clear the focus target manually
    /// </summary>
    public static void ClearFocusTarget()
    {
        _targetFocus = FocusTarget.None;
        Debug.Log("SceneFocusManager: Focus target cleared");
    }
}
