using UnityEngine.UI;
using UnityEngine;

public sealed class IndexUIButtonResolver : IUIButtonResolver
{
    public Button Resolve(TutorialManager manager, HandPointerSubStep step)
    {
        if (step.uiButtonIndex < 0)
        {
            Debug.LogWarning("[HandPointerTutorial] IndexUIButtonResolver: uiButtonIndex is negative, cannot resolve button");
            return null;
        }

        Debug.Log($"[HandPointerTutorial] IndexUIButtonResolver: Trying to resolve button with index {step.uiButtonIndex}");

        if (manager.UIButtonsCacheEmpty)
        {
            Debug.Log("[HandPointerTutorial] IndexUIButtonResolver: UI button cache is empty, rebuilding cache");
            manager.CacheUIButtonsFromUIManager();
        }

        var button = manager.TryGetUIButtonByIndex(step.uiButtonIndex);
        if (button != null)
        {
            Debug.Log($"[HandPointerTutorial] IndexUIButtonResolver: Button found - name='{button.name}', index={step.uiButtonIndex}, interactable={button.interactable}, active={button.gameObject.activeSelf}");
        }
        else
        {
            Debug.LogWarning($"[HandPointerTutorial] IndexUIButtonResolver: Button NOT FOUND for index {step.uiButtonIndex}");
        }

        return button;
    }
}
