using UnityEngine;
using UnityEngine.UI;

public partial class TutorialManager
{
    private bool _isRunningHandPointerSubTutorial;
    private int _handPointerSubStepIndex = -1;
    private HandPointerTutorialSequenceSO _activeHandPointerSubTutorial;
    private IUIButtonResolver _buttonResolver;
    private Button _currentHandPointerTargetButton;
    private RectTransform _currentHandPointerTargetRect;

    private void CancelHandPointerSubTutorial()
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

        _currentHandPointerTargetRect = null;
    }

    private void InitHandPointerResolver()
    {
        _buttonResolver = Create(_currentMode);
    }
    private void SetHandPointerSequenceButtonsInteractable(bool interactable)
    {
        if (_activeHandPointerSubTutorial == null ||
            _activeHandPointerSubTutorial.steps == null ||
            _activeHandPointerSubTutorial.steps.Count == 0)
            return;

        if (_buttonResolver == null)
        {
            InitHandPointerResolver();
            if (_buttonResolver == null)
                return;
        }

        for (int i = 0; i < _activeHandPointerSubTutorial.steps.Count; i++)
        {
            var subStep = _activeHandPointerSubTutorial.steps[i];
            if (subStep == null)
                continue;

            var btn = _buttonResolver.Resolve(this, subStep);
            if (btn == null)
                continue;

            btn.interactable = interactable;
        }
    }

    private void StartHandPointerPlainSubTutorial(PlainTutorialPanelStep plainStep)
    {
        var config = plainStep != null ? plainStep.config : null;
        if (config == null || config.handPointerSequence == null)
            return;

        var sequence = config.handPointerSequence;
        if (sequence.steps == null || sequence.steps.Count == 0)
            return;

        _activeHandPointerSubTutorial = sequence;
        _handPointerSubStepIndex = 0;
        _isRunningHandPointerSubTutorial = true;

        if (_buttonResolver == null)
        {
            InitHandPointerResolver();
        }

        ApplyCurrentHandPointerSubStep();

        if (!_isRunningHandPointerSubTutorial)
        {
            UpdatePlainStepNextButtonsInteractable();
        }
    }

    private void StartHandPointerHotelSubTutorial(HotelTutorialPanelStep hotelStep)
    {
        var config = hotelStep != null ? hotelStep.config : null;
        if (config == null || config.handPointerSequence == null)
            return;

        var sequence = config.handPointerSequence;
        if (sequence.steps == null || sequence.steps.Count == 0)
            return;

        _activeHandPointerSubTutorial = sequence;
        _handPointerSubStepIndex = 0;
        _isRunningHandPointerSubTutorial = true;

        if (_buttonResolver == null)
        {
            InitHandPointerResolver();
        }

        ApplyCurrentHandPointerSubStep();

        if (!_isRunningHandPointerSubTutorial)
        {
            UpdateHotelStepNextButtonsInteractable();
        }
    }

    private void ApplyCurrentHandPointerSubStep()
    {
        if (!_isRunningHandPointerSubTutorial || _activeHandPointerSubTutorial == null)
            return;

        if (_handPointerSubStepIndex < 0 ||
            _handPointerSubStepIndex >= _activeHandPointerSubTutorial.steps.Count)
            return;

        var step = _activeHandPointerSubTutorial.steps[_handPointerSubStepIndex];
        if (step == null)
        {
            CancelHandPointerSubTutorial();
            return;
        }

        if (_buttonResolver == null)
        {
            InitHandPointerResolver();
            if (_buttonResolver == null)
            {
                CancelHandPointerSubTutorial();
                return;
            }
        }

        var targetButton = _buttonResolver.Resolve(this, step);
        if (targetButton == null)
        {
            CancelHandPointerSubTutorial();
            return;
        }

        var rect = targetButton.transform as RectTransform;
        if (rect == null)
        {
            CancelHandPointerSubTutorial();
            return;
        }

        targetButton.gameObject.SetActive(true);
        targetButton.interactable = true;

        if (_currentHandPointerTargetButton != null)
            _currentHandPointerTargetButton.onClick.RemoveListener(OnHandPointerTargetClicked);

        _currentHandPointerTargetButton = targetButton;
        _currentHandPointerTargetButton.onClick.AddListener(OnHandPointerTargetClicked);

        _currentHandPointerTargetRect = rect;

        var pointer = ServiceLocator.Get<ITutorialPointer>();
        if (pointer != null)
        {
            pointer.PointTo(rect, step.pointerOffset);
        }
    }

    private void OnHandPointerTargetClicked()
    {
        if (!_isRunningHandPointerSubTutorial || _activeHandPointerSubTutorial == null)
            return;

        if (_currentHandPointerTargetButton != null)
        {
            _currentHandPointerTargetButton.interactable = false;
        }

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
        SetHandPointerSequenceButtonsInteractable(false);

        CancelHandPointerSubTutorial();

        if (plainTutorials != null &&
            _plainPanelIndex >= 0 &&
            _plainPanelIndex < plainTutorials.Count)
        {
            var currentPlainStep = plainTutorials[_plainPanelIndex];
            var config = currentPlainStep != null ? currentPlainStep.config : null;
            if (config != null)
            {
                if (config.useFoodDropAsNext)
                {
                    TryHandleFoodDropProgress(false);
                    return;
                }

                if (config.usePoopCleanAsNext)
                {
                    TryHandlePoopCleanProgress();
                    return;
                }
            }
        }

        ShowNextPlainPanel();
    }

    private void UpdateCurrentHandPointerOffsetRealtime()
    {
        if (!_isRunningHandPointerSubTutorial || _activeHandPointerSubTutorial == null)
            return;

        if (_handPointerSubStepIndex < 0 ||
            _handPointerSubStepIndex >= _activeHandPointerSubTutorial.steps.Count)
            return;

        if (_currentHandPointerTargetRect == null)
            return;

        var pointer = ServiceLocator.Get<ITutorialPointer>();
        if (pointer == null)
            return;

        var step = _activeHandPointerSubTutorial.steps[_handPointerSubStepIndex];
        pointer.PointTo(_currentHandPointerTargetRect, step.pointerOffset);
    }
}
