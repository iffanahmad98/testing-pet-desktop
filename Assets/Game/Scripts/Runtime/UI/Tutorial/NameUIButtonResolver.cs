using UnityEngine.UI;


public sealed class NameUIButtonResolver : IUIButtonResolver
{
    public Button Resolve(TutorialManager manager, HandPointerSubStep step)
    {
        return manager.GetButtonByName(step.ButtonKey);
    }
}
