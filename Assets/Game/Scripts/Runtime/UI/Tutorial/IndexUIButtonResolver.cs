using UnityEngine.UI;

public sealed class IndexUIButtonResolver : IUIButtonResolver
{
    public Button Resolve(TutorialManager manager, HandPointerSubStep step)
    {
        if (step.uiButtonIndex < 0)
            return null;

        if (manager.UIButtonsCacheEmpty)
            manager.CacheUIButtonsFromUIManager();

        return manager.TryGetUIButtonByIndex(step.uiButtonIndex);
    }
}
