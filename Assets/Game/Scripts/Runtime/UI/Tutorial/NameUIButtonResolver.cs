using UnityEngine.UI;
using UnityEngine;

public sealed class NameUIButtonResolver : IUIButtonResolver
{
    public Button Resolve(TutorialManager manager, HandPointerSubStep step)
    {
        if (manager == null || step == null)
        {
            Debug.LogWarning("[HandPointerTutorial] NameUIButtonResolver: manager or step is null");
            return null;
        }

        if (string.IsNullOrEmpty(step.ButtonKey))
        {
            Debug.LogWarning("[HandPointerTutorial] NameUIButtonResolver: ButtonKey is null or empty");
            return null;
        }

        Debug.Log($"[HandPointerTutorial] NameUIButtonResolver: Trying to resolve button with name '{step.ButtonKey}'");

        var button = manager.ResolveHotelButtonByName(step.ButtonKey);
        if (button != null)
        {
            Debug.Log($"[HandPointerTutorial] NameUIButtonResolver: Button found - name='{button.name}', ButtonKey='{step.ButtonKey}', interactable={button.interactable}, active={button.gameObject.activeSelf}");
        }
        else
        {
            Debug.LogWarning($"[HandPointerTutorial] NameUIButtonResolver: Button NOT FOUND for ButtonKey '{step.ButtonKey}'");
        }

        return button;
    }
}
