using UnityEngine;
using UnityEngine.UI;

public partial class TutorialManager
{
    [Header("Hand Pointer Sub-Tutorial")]
    [SerializeField] private Button handPointerNextButton;

    private bool _isRunningHandPointerSubTutorial;
    private int _handPointerSubStepIndex = -1;
    private HandPointerTutorialSequenceSO _activeHandPointerSubTutorial;
    private Button _currentHandPointerTargetButton;
    private void StartHandPointerSubTutorial(SimpleTutorialPanelStep simpleStep)
    {
        if (simpleStep == null || simpleStep.handPointerSequence == null)
            return;

        var sequence = simpleStep.handPointerSequence;
        if (sequence.steps == null || sequence.steps.Count == 0)
            return;

        _activeHandPointerSubTutorial = sequence;
        _handPointerSubStepIndex = 0;
        _isRunningHandPointerSubTutorial = true;

        ApplyCurrentHandPointerSubStep();
    }

    private void ApplyCurrentHandPointerSubStep()
    {
        if (!_isRunningHandPointerSubTutorial || _activeHandPointerSubTutorial == null)
            return;

        if (_handPointerSubStepIndex < 0 || _handPointerSubStepIndex >= _activeHandPointerSubTutorial.steps.Count)
            return;

        var pointer = ServiceLocator.Get<ITutorialPointer>();
        if (pointer == null)
            return;

        var step = _activeHandPointerSubTutorial.steps[_handPointerSubStepIndex];

        Button targetButton = null;

        if (step.uiButtonIndex >= 0)
        {
            if (_uiButtonsCache == null || _uiButtonsCache.Length == 0)
            {
                CacheUIButtonsFromUIManager();
            }

            if (_uiButtonsCache != null && step.uiButtonIndex < _uiButtonsCache.Length)
            {
                targetButton = _uiButtonsCache[step.uiButtonIndex];
            }
        }

        if (targetButton == null)
        {
            targetButton = handPointerNextButton;
        }

        if (targetButton == null)
            return;

        var rect = targetButton.transform as RectTransform;
        if (rect == null)
            return;

        if (!targetButton.gameObject.activeSelf)
            targetButton.gameObject.SetActive(true);

        targetButton.interactable = true;

        if (_currentHandPointerTargetButton != null)
        {
            _currentHandPointerTargetButton.onClick.RemoveListener(OnHandPointerTargetClicked);
        }

        _currentHandPointerTargetButton = targetButton;
        _currentHandPointerTargetButton.onClick.RemoveListener(OnHandPointerTargetClicked);
        _currentHandPointerTargetButton.onClick.AddListener(OnHandPointerTargetClicked);

        pointer.PointTo(rect, step.pointerOffset);
    }

    private void OnHandPointerTargetClicked()
    {
        if (!_isRunningHandPointerSubTutorial || _activeHandPointerSubTutorial == null)
            return;

        _handPointerSubStepIndex++;

        if (_handPointerSubStepIndex >= _activeHandPointerSubTutorial.steps.Count)
        {
            EndHandPointerSubTutorial();
        }
        else
        {
            ApplyCurrentHandPointerSubStep();
        }
    }

    private void EndHandPointerSubTutorial()
    {
        _isRunningHandPointerSubTutorial = false;

        var pointer = ServiceLocator.Get<ITutorialPointer>();
        if (pointer != null)
        {
            pointer.Hide();
        }

        if (_currentHandPointerTargetButton != null)
        {
            _currentHandPointerTargetButton.onClick.RemoveListener(OnHandPointerTargetClicked);
            _currentHandPointerTargetButton = null;
        }

        if (simpleTutorialPanels != null &&
            _simplePanelIndex >= 0 &&
            _simplePanelIndex < simpleTutorialPanels.Count)
        {
            var currentSimpleStep = simpleTutorialPanels[_simplePanelIndex];
            if (currentSimpleStep != null)
            {
                if (currentSimpleStep.useFoodDropAsNext)
                {
                    // For food-drop steps, defer progression to TryHandleFoodDropProgress
                    // so it can re-check count & delay instead of forcing Next here.
                    TryHandleFoodDropProgress(false);
                    return;
                }

                if (currentSimpleStep.usePoopCleanAsNext)
                {
                    // For poop-clean steps, delegate to TryHandlePoopCleanProgress
                    // so that progression still goes through the centralized logic
                    // (which will call RequestNextSimplePanel).
                    TryHandlePoopCleanProgress();
                    return;
                }
            }
        }

        // Default behaviour for steps that don't use special world interactions:
        // finish hand pointer, then just advance to next simple panel.
        ShowNextSimplePanel();
    }
}