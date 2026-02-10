using UnityEngine.UI;


public sealed class NameUIButtonResolver : IUIButtonResolver
{
    public Button Resolve(TutorialManager manager, HandPointerSubStep step)
    {
        if (manager == null || step == null)
            return null;

        return manager.ResolveHotelButtonByName(step.ButtonKey);
    }
}
