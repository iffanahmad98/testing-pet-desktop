using UnityEngine;

public partial class TutorialManager
{
    private void HideAllTutorialPanels()
    {
        foreach (var step in tutorialSteps)
        {
            if (step.panelRoot != null)
                step.panelRoot.SetActive(false);
        }

        HidePointerIfAny();
    }

    private void ShowOnly(TutorialStep stepToShow)
    {
        foreach (var step in tutorialSteps)
        {
            if (step.panelRoot == null)
                continue;

            step.panelRoot.SetActive(step == stepToShow);
        }

        UpdatePointerForStep(stepToShow);
    }

    private void UpdatePointerForStep(TutorialStep step)
    {
        var pointer = ServiceLocator.Get<ITutorialPointer>();
        if (pointer == null)
            return;

        if (step == null)
        {
            pointer.Hide();
            return;
        }

        if (!step.usePointer)
        {
            pointer.Hide();
            return;
        }

        if (step.useTutorialMonsterAsPointerTarget && _tutorialMonsterRect != null)
        {
            pointer.PointTo(_tutorialMonsterRect, step.pointerOffset);
            return;
        }

        RectTransform target = step.pointerTarget;
        if (target == null && step.nextButton != null)
        {
            target = step.nextButton.transform as RectTransform;
        }

        if (target != null)
        {
            pointer.PointTo(target, step.pointerOffset);
        }
        else
        {
            pointer.Hide();
        }
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
