using UnityEngine;

public partial class TutorialManager
{
    private void HideAllTutorialPanels()
    {
        if (simpleTutorialPanels != null)
        {
            for (int i = 0; i < simpleTutorialPanels.Count; i++)
            {
                var step = simpleTutorialPanels[i];
                if (step != null && step.panelRoot != null)
                {
                    step.panelRoot.SetActive(false);
                }
            }
        }

        HidePointerIfAny();
    }

    private void HidePointerIfAny()
    {
        var pointer = ServiceLocator.Get<ITutorialPointer>();
        if (pointer != null)
        {
            pointer.Hide();
        }
    }
}
