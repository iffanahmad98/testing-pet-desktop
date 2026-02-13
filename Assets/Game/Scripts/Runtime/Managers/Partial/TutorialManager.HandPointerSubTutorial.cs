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
                new LastAssignedHotelRoomTargetHandler(this, _targetingContext),
                new HotelGiftTargetHandler(this, _targetingContext),
                new HotelRandomLootTargetHandler(this, _targetingContext),
                new HotelShopTargetHandler(this, _targetingContext),
                new HotelFacilitiesHireButtonTargetHandler(this, _targetingContext),
                new HotelFacilitiesApplyButtonTargetHandler(this, _targetingContext),
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

        UnlockGuestScrollForTutorial();
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
        LockGuestScrollForTutorial();

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
        {
            Debug.LogWarning($"[HandPointerTutorial] Failed validation - cancelling tutorial");
            return;
        }

        var step = _activeHandPointerSubTutorial.steps[_handPointerSubStepIndex];
        if (step == null)
        {
            Debug.LogWarning($"[HandPointerTutorial] Step at index {_handPointerSubStepIndex} is null");
            CancelHandPointerSubTutorial();
            return;
        }

        // Find handler that can process this step
        foreach (var handler in _targetHandlers)
        {
            if (handler.CanHandle(step))
            {
                bool success = handler.Apply(step, OnHandPointerTargetClicked);
                if (!success)
                {
                    Debug.LogWarning($"[HandPointerTutorial] Handler '{handler.GetType().Name}' failed to apply step");
                    CancelHandPointerSubTutorial();
                }
                return;
            }
        }

        Debug.LogWarning($"[HandPointerTutorial] No handler found for step - {GetStepInfo(step)}");
        CancelHandPointerSubTutorial();
    }

    private string GetStepInfo(HandPointerSubStep step)
    {
        if (step.useGuestItemCheckInButton)
            return "Type=GuestItemCheckIn";
        if (step.useLastAssignedHotelRoomTarget)
            return "Type=LastAssignedHotelRoom";
        if (step.useHotelGiftTarget)
            return "Type=HotelGift";
        if (step.useHotelShopTarget)
            return "Type=HotelShop";
        if (step.useHotelFacilitiesHireButtonTarget)
            return "Type=HotelFacilitiesHireButton";
        if (step.useHotelFacilitiesApplyButtonTarget)
            return "Type=HotelFacilitiesApplyButton";
        if (step.useHotelRoomTarget)
            return $"Type=HotelRoom, GuestTypeFilter='{step.hotelRoomGuestTypeFilter}'";
        if (step.useClickableObjectTarget)
            return $"Type=ClickableObject, ID='{step.clickableObjectId}'";
        if (!string.IsNullOrEmpty(step.ButtonKey))
            return $"Type=UIButton, ButtonKey='{step.ButtonKey}'";
        if (step.uiButtonIndex >= 0)
            return $"Type=UIButton, Index={step.uiButtonIndex}";
        return "Type=Unknown";
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

    // Dipanggil eksplisit dari GuestItem ketika tombol check-in ditekan,
    // supaya step tutorial tetap maju walaupun listener UI tidak terpasang dengan benar.
    public void NotifyGuestItemCheckInClicked()
    {
        if (!_isRunningHandPointerSubTutorial || _activeHandPointerSubTutorial == null)
            return;

        if (_handPointerSubStepIndex < 0 || _handPointerSubStepIndex >= _activeHandPointerSubTutorial.steps.Count)
            return;

        var currentStep = _activeHandPointerSubTutorial.steps[_handPointerSubStepIndex];
        if (currentStep == null || !currentStep.useGuestItemCheckInButton)
            return;

        OnHandPointerTargetClicked();
    }

    private void OnHandPointerTargetClicked()
    {
        Debug.Log($"[HandPointerTutorial] OnHandPointerTargetClicked - Advancing tutorial");

        if (!_isRunningHandPointerSubTutorial || _activeHandPointerSubTutorial == null)
        {
            Debug.Log("[HandPointerTutorial] Tutorial is not running - ignoring click");
            return;
        }

        if (_targetingContext?.CurrentButton != null)
        {
            _targetingContext.CurrentButton.interactable = false;
        }

        if (_targetingContext?.PostClickAction != null)
        {
            Debug.Log("[HandPointerTutorial] Executing PostClickAction");
            var postAction = _targetingContext.PostClickAction;
            _targetingContext.PostClickAction = null;
            postAction.Invoke();
        }

        _handPointerSubStepIndex++;

        if (_handPointerSubStepIndex >= _activeHandPointerSubTutorial.steps.Count)
        {
            EndHandPointerSubTutorial();
            Debug.Log("[HandPointerTutorial] Sub-tutorial completed");
        }
        else
        {
            Debug.Log("applycurrent");
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
        else
        {
            Debug.LogWarning($"[HandPointerTutorial] Unknown mode {_currentMode}");
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

        var step = _activeHandPointerSubTutorial.steps[_handPointerSubStepIndex];
        if (_targetingContext?.Pointer == null)
            return;

        if (_targetingContext.CurrentRect != null)
        {
            _targetingContext.Pointer.PointTo(_targetingContext.CurrentRect, step.pointerOffset);
            return;
        }

        if (_targetingContext.CurrentWorldTarget != null)
        {
            _targetingContext.Pointer.PointToWorld(_targetingContext.CurrentWorldTarget, step.pointerOffset);
        }
    }

    #endregion
}
