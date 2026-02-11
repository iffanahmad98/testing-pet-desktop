using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using MagicalGarden.Hotel;

public partial class TutorialManager
{
    #region Fields

    private bool _isRunningHandPointerSubTutorial;
    private int _handPointerSubStepIndex = -1;
    private HandPointerTutorialSequenceSO _activeHandPointerSubTutorial;
    private IUIButtonResolver _buttonResolver;

    // Targeting context and handlers
    private HandPointerTargetingContext _targetingContext;
    private List<HandPointerTargetHandler> _targetHandlers;

    #endregion

    #region Lifecycle & State Management

    private void InitializeHandPointerSystem()
    {
        if (_targetingContext == null)
        {
            _targetingContext = new HandPointerTargetingContext();
        }

        if (_buttonResolver == null)
        {
            _buttonResolver = Create(_currentMode);
        }

        if (_targetHandlers == null)
        {
            _targetHandlers = new List<HandPointerTargetHandler>
            {
                new HotelRoomTargetHandler(this, _targetingContext),
                new GuestItemCheckInTargetHandler(this, _targetingContext),
                new ClickableObjectTargetHandler(this, _targetingContext),
                new UIButtonTargetHandler(this, _targetingContext, _buttonResolver)
            };
        }
    }

    private void CancelHandPointerSubTutorial()
    {
        _isRunningHandPointerSubTutorial = false;

        if (_targetingContext != null)
        {
            if (_targetingContext.CurrentButton != null)
            {
                _targetingContext.CurrentButton.onClick.RemoveListener(OnHandPointerTargetClicked);
            }

            if (_targetingContext.CurrentClickable != null)
            {
                _targetingContext.CurrentClickable.OnClicked -= OnHandPointerClickableTargetClicked;
            }

            _targetingContext.HidePointer();
            _targetingContext.ClearCurrentTargets();
        }
    }

    #endregion

    #region Sequence Control

    private void SetHandPointerSequenceButtonsInteractable(bool interactable)
    {
        if (_activeHandPointerSubTutorial == null ||
            _activeHandPointerSubTutorial.steps == null ||
            _activeHandPointerSubTutorial.steps.Count == 0)
            return;

        InitializeHandPointerSystem();

        if (_buttonResolver == null)
            return;

        foreach (var subStep in _activeHandPointerSubTutorial.steps)
        {
            if (subStep == null)
                continue;

            var btn = _buttonResolver.Resolve(this, subStep);
            if (btn != null)
            {
                btn.interactable = interactable;
            }
        }
    }

    private void StartHandPointerPlainSubTutorial(PlainTutorialPanelStep plainStep)
    {
        if (!ValidateAndInitializeSequence(plainStep?.config?.handPointerSequence))
            return;

        StartSubTutorialSequence();

        if (!_isRunningHandPointerSubTutorial)
        {
            UpdatePlainStepNextButtonsInteractable();
        }
    }

    private void StartHandPointerHotelSubTutorial(HotelTutorialPanelStep hotelStep)
    {
        if (!ValidateAndInitializeSequence(hotelStep?.config?.handPointerSequence))
            return;

        StartSubTutorialSequence();

        if (!_isRunningHandPointerSubTutorial)
        {
            UpdateHotelStepNextButtonsInteractable();
        }
    }

    private bool ValidateAndInitializeSequence(HandPointerTutorialSequenceSO sequence)
    {
        if (sequence == null || sequence.steps == null || sequence.steps.Count == 0)
            return false;

        _activeHandPointerSubTutorial = sequence;
        _handPointerSubStepIndex = 0;
        _isRunningHandPointerSubTutorial = true;

        InitializeHandPointerSystem();

        return true;
    }

    private void StartSubTutorialSequence()
    {
        ApplyCurrentHandPointerSubStep();
    }

    #endregion

    #region Step Application

    private void ApplyCurrentHandPointerSubStep()
    {
        if (!ValidateSubTutorialState())
            return;

        var step = _activeHandPointerSubTutorial.steps[_handPointerSubStepIndex];
        if (step == null)
        {
            CancelHandPointerSubTutorial();
            return;
        }

        // Chain of Responsibility pattern: find first handler that can handle this step
        foreach (var handler in _targetHandlers)
        {
            if (handler.CanHandle(step))
            {
                bool success = handler.Apply(step, OnHandPointerTargetClicked);
                if (!success)
                {
                    CancelHandPointerSubTutorial();
                }
                return;
            }
        }

        // If no handler can handle the step, cancel
        Debug.LogWarning("[HotelTutorial] HandPointerSub: No handler found for current step configuration.");
        CancelHandPointerSubTutorial();
    }

    private bool ValidateSubTutorialState()
    {
        if (!_isRunningHandPointerSubTutorial || _activeHandPointerSubTutorial == null)
            return false;

        if (_handPointerSubStepIndex < 0 || _handPointerSubStepIndex >= _activeHandPointerSubTutorial.steps.Count)
            return false;

        return true;
    }

    #endregion

    #region Event Handlers

    private void OnHandPointerClickableTargetClicked(ClickableObject clickable)
    {
        OnHandPointerTargetClicked();
    }

    private void OnHandPointerTargetClicked()
    {
        if (!_isRunningHandPointerSubTutorial || _activeHandPointerSubTutorial == null)
            return;

        if (_targetingContext?.CurrentButton != null)
        {
            _targetingContext.CurrentButton.interactable = false;
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

    #endregion

    #region Completion & Cleanup

    private void EndHandPointerSubTutorial()
    {
        SetHandPointerSequenceButtonsInteractable(false);
        CancelHandPointerSubTutorial();

        if (_currentMode == TutorialMode.Plain)
        {
            HandlePlainModeCompletion();
        }
        else if (_currentMode == TutorialMode.Hotel)
        {
            ShowNextHotelPanel();
        }
    }

    private void HandlePlainModeCompletion()
    {
        if (plainTutorials != null && _plainPanelIndex >= 0 && _plainPanelIndex < plainTutorials.Count)
        {
            var currentPlainStep = plainTutorials[_plainPanelIndex];
            var config = currentPlainStep?.config;

            if (config != null)
            {
                // Don't proceed if waiting for food drop or poop clean
                if (config.useFoodDropAsNext || config.usePoopCleanAsNext)
                    return;
            }
        }

        ShowNextPlainPanel();
    }

    #endregion

    #region Utility Methods

    private void UpdateCurrentHandPointerOffsetRealtime()
    {
        if (!ValidateSubTutorialState())
            return;

        if (_targetingContext?.CurrentRect == null || _targetingContext?.Pointer == null)
            return;

        var step = _activeHandPointerSubTutorial.steps[_handPointerSubStepIndex];
        _targetingContext.Pointer.PointTo(_targetingContext.CurrentRect, step.pointerOffset);
    }

    #endregion
}
