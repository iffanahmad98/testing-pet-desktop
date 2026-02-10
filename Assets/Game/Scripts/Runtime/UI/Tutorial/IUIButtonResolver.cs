using System;
using UnityEngine.UI;

public interface IUIButtonResolver
{
    Button Resolve(TutorialManager manager, HandPointerSubStep step);
}
