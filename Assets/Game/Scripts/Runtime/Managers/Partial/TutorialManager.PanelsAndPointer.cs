using UnityEngine;

public partial class TutorialManager
{
    private void HideAllTutorialPanels()
    {
        if (plainTutorials != null)
        {
            for (int i = 0; i < plainTutorials.Count; i++)
            {
                var step = plainTutorials[i];
                if (step != null && step.panelRoot != null)
                {
                    step.panelRoot.SetActive(false);
                }
            }
        }
        if (hotelTutorials != null)
        {
            for (int i = 0; i < hotelTutorials.Count; i++)
            {
                var step = hotelTutorials[i];
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
