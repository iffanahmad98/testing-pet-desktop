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
    private RectTransform _currentHandPointerTargetRect;
    private void SetHandPointerSequenceButtonsInteractable(bool interactable)
    {
        if (_activeHandPointerSubTutorial == null ||
            _activeHandPointerSubTutorial.steps == null ||
            _activeHandPointerSubTutorial.steps.Count == 0)
            return;

        if (_uiButtonsCache == null || _uiButtonsCache.Length == 0)
        {
            CacheUIButtonsFromUIManager();
        }

        if (_uiButtonsCache == null || _uiButtonsCache.Length == 0)
            return;

        for (int i = 0; i < _activeHandPointerSubTutorial.steps.Count; i++)
        {
            var subStep = _activeHandPointerSubTutorial.steps[i];
            if (subStep == null || subStep.uiButtonIndex < 0 || subStep.uiButtonIndex >= _uiButtonsCache.Length)
                continue;

            var btn = _uiButtonsCache[subStep.uiButtonIndex];
            if (btn == null)
                continue;

            btn.interactable = interactable;
        }
    }
    private void StartHandPointerSubTutorial(PlainTutorialPanelStep plainStep)
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

        _currentHandPointerTargetRect = rect;
        pointer.PointTo(rect, step.pointerOffset);
    }

    private void OnHandPointerTargetClicked()
    {
        if (!_isRunningHandPointerSubTutorial || _activeHandPointerSubTutorial == null)
            return;

        // Saat pindah ke sub-step berikutnya, matikan interaksi
        // pada tombol target sebelumnya.
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
        // Setelah sequence selesai, semua tombol yang pernah
        // dipakai sebagai target hand pointer dibuat non-interactable.
        SetHandPointerSequenceButtonsInteractable(false);

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
                    // For food-drop steps, defer progression to TryHandleFoodDropProgress
                    // so it can re-check count & delay instead of forcing Next here.
                    TryHandleFoodDropProgress(false);
                    return;
                }

                if (config.usePoopCleanAsNext)
                {
                    // For poop-clean steps, delegate to TryHandlePoopCleanProgress
                    // so that progression still goes through the centralized logic
                    // (which will call RequestNextPlainPanel).
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