using System;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public partial class TutorialManager
{
    private void Update()
    {
        if (!IsSimpleMode)
            return;

        if (_simplePanelIndex < 0 || simpleTutorialPanels == null || _simplePanelIndex >= simpleTutorialPanels.Count)
            return;

        var step = simpleTutorialPanels[_simplePanelIndex];
        if (step == null)
            return;

        if (step.usePointer)
        {
            UpdatePointerForSimpleStep(step);
        }
    }

    public void RefreshCurrentSimplePointer()
    {
        if (!IsSimpleMode)
            return;

        if (simpleTutorialPanels == null || simpleTutorialPanels.Count == 0)
            return;

        if (_simplePanelIndex < 0 || _simplePanelIndex >= simpleTutorialPanels.Count)
            return;

        UpdatePointerForSimpleStep(simpleTutorialPanels[_simplePanelIndex]);
    }

    private void StartSimpleTutorialSequence()
    {
        if (simpleTutorialPanels == null || simpleTutorialPanels.Count == 0)
        {
            Debug.LogWarning("TutorialManager: simpleTutorialPanels kosong, tidak ada tutorial sederhana yang ditampilkan.");
            return;
        }
        for (int i = 0; i < simpleTutorialPanels.Count; i++)
        {
            var step = simpleTutorialPanels[i];
            if (step == null)
                continue;

            if (step.panelRoot != null)
                step.panelRoot.SetActive(false);

            var nextButton = GetSimpleStepNextButton(step);
            if (nextButton != null)
            {
                if (!_simpleNextButtonsHooked.Contains(nextButton))
                {
                    _simpleNextButtonsHooked.Add(nextButton);
                    nextButton.onClick.AddListener(RequestNextSimplePanel);
                }
            }
        }

        _simplePanelIndex = 0;
        var firstStep = simpleTutorialPanels[_simplePanelIndex];
        if (firstStep != null && firstStep.panelRoot != null)
        {
            PlaySimplePanelShowAnimation(firstStep.panelRoot);
            UpdatePointerForSimpleStep(firstStep);
            _simpleStepShownTime = Time.time;

            UpdateRightClickMouseHintForSimpleStep(firstStep);
            PlaySimpleStepEffectForIndex(_simplePanelIndex);

            UpdateTutorialMonsterMovementForSimpleStep(firstStep);

            UpdateSimpleStepNextButtonsInteractable();
        }
    }

    public void ShowNextSimplePanel()
    {
        if (useTutorialSteps)
            return;

        if (simpleTutorialPanels == null || simpleTutorialPanels.Count == 0)
            return;

        if (_simplePanelIndex < 0)
        {
            StartSimpleTutorialSequence();
            return;
        }

        if (_simplePanelIndex < simpleTutorialPanels.Count)
        {
            var currentStep = simpleTutorialPanels[_simplePanelIndex];
            if (currentStep != null && currentStep.panelRoot != null)
                currentStep.panelRoot.SetActive(false);
            HideRightClickMouseHint();

            // Saat meninggalkan step ini, lepas efek freeze movement kalau ada.
            UpdateTutorialMonsterMovementForSimpleStep(null);
        }

        _simplePanelIndex++;

        if (_simplePanelIndex >= simpleTutorialPanels.Count)
        {
            HidePointerIfAny();
            RestoreUIManagerButtonsInteractable();
            gameObject.SetActive(false);
            return;
        }

        var nextStep = simpleTutorialPanels[_simplePanelIndex];
        if (nextStep != null && nextStep.panelRoot != null)
        {
            PlaySimplePanelShowAnimation(nextStep.panelRoot);
            UpdatePointerForSimpleStep(nextStep);
            _simpleStepShownTime = Time.time;

            UpdateRightClickMouseHintForSimpleStep(nextStep);
            PlaySimpleStepEffectForIndex(_simplePanelIndex);

            UpdateTutorialMonsterMovementForSimpleStep(nextStep);
            ApplyTutorialMonsterHungerForSimpleStep(nextStep);

            UpdateSimpleStepNextButtonsInteractable();
        }
    }

    private void UpdateTutorialMonsterMovementForSimpleStep(SimpleTutorialPanelStep step)
    {
        if (_tutorialMonsterController == null)
            return;

        bool shouldFreeze = step != null && step.freezeTutorialMonsterMovement;
        _tutorialMonsterController.SetMovementFrozenByTutorial(shouldFreeze);
    }

    private void ApplyTutorialMonsterHungerForSimpleStep(SimpleTutorialPanelStep step)
    {
        if (_tutorialMonsterController == null || step == null)
            return;

        if (!step.makeTutorialMonsterHungry)
            return;

        _tutorialMonsterController.SetHunger(20f);
    }

    private void UpdatePointerForSimpleStep(SimpleTutorialPanelStep step)
    {
        var pointer = ServiceLocator.Get<ITutorialPointer>();
        if (pointer == null)
            return;

        if (step == null || !step.usePointer)
        {
            pointer.Hide();
            return;
        }

        if (step.useTutorialMonsterAsPointerTarget && _tutorialMonsterRect != null)
        {
            pointer.PointTo(_tutorialMonsterRect, step.pointerOffset);
            return;
        }

        if (step.useNextButtonAsPointerTarget)
        {
            var btn = GetSimpleStepNextButton(step);
            if (btn != null)
            {
                var rect = btn.transform as RectTransform;
                if (rect != null)
                {
                    pointer.PointTo(rect, step.pointerOffset);
                    return;
                }
            }
        }

        pointer.Hide();
    }

    private void PlaySimplePanelShowAnimation(GameObject panel)
    {
        if (panel == null)
            return;

        panel.SetActive(true);

        var rect = panel.GetComponent<RectTransform>();
        var canvasGroup = panel.GetComponent<CanvasGroup>();

        if (rect == null || canvasGroup == null)
            return;

        rect.DOKill();
        canvasGroup.DOKill();

        var targetPos = rect.anchoredPosition;
        var startPos = targetPos;
        startPos.y -= rect.rect.height;
        rect.anchoredPosition = startPos;

        canvasGroup.alpha = 0f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        rect.DOAnchorPos(targetPos, simplePanelShowDuration).SetEase(simplePanelShowEase);
        canvasGroup.DOFade(1f, simplePanelShowDuration).SetEase(Ease.OutQuad);
    }

    private void UpdateRightClickMouseHintForSimpleStep(SimpleTutorialPanelStep step)
    {
        if (step == null || !step.showRightClickMouseHint)
        {
            HideRightClickMouseHint();
            return;
        }

        if (_tutorialMonsterRect == null || rightClickMouseHintPrefab == null)
        {
            HideRightClickMouseHint();
            return;
        }

        var canvas = _tutorialMonsterRect.GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            HideRightClickMouseHint();
            return;
        }

        if (_rightClickMouseHintInstance == null)
        {
            _rightClickMouseHintInstance = UnityEngine.Object.Instantiate(rightClickMouseHintPrefab, canvas.transform);
        }

        _rightClickMouseHintInstance.gameObject.SetActive(true);

        var petPos = _tutorialMonsterRect.anchoredPosition;
        var baseOffset = new Vector2(0f, 120f);
        _rightClickMouseHintInstance.anchoredPosition = petPos + baseOffset + step.rightClickMouseHintOffset;

        PlaySimplePanelShowAnimation(_rightClickMouseHintInstance.gameObject);
    }

    private void HideRightClickMouseHint()
    {
        if (_rightClickMouseHintInstance != null)
        {
            _rightClickMouseHintInstance.gameObject.SetActive(false);
        }
    }

    private void OnTutorialMonsterClicked(PointerEventData eventData)
    {
        if (!IsSimpleMode)
            return;

        if (simpleTutorialPanels == null || simpleTutorialPanels.Count == 0)
            return;

        if (_simplePanelIndex < 0 || _simplePanelIndex >= simpleTutorialPanels.Count)
            return;

        var step = simpleTutorialPanels[_simplePanelIndex];
        if (step == null)
            return;

        bool wantsLeft = step.useLeftClickPetAsNext;
        bool wantsRight = step.useRightClickPetAsNext;
        if (!wantsLeft && !wantsRight)
            return;

        const float minClickDelay = 0.15f;
        if (Time.time - _simpleStepShownTime < minClickDelay)
            return;

        var button = eventData.button;
        bool isLeft = button == PointerEventData.InputButton.Left;
        bool isRight = button == PointerEventData.InputButton.Right;

        if ((isLeft && wantsLeft) || (isRight && wantsRight))
        {
            RequestNextSimplePanel();
        }
    }

    private void UpdateSimpleStepNextButtonsInteractable()
    {
        if (simpleTutorialPanels == null || simpleTutorialPanels.Count == 0)
            return;

        if (_uiButtonsCache == null || _uiButtonsCache.Length == 0)
        {
            CacheUIButtonsFromUIManager();
        }

        if (_uiButtonsCache == null || _uiButtonsCache.Length == 0)
            return;

        for (int i = 0; i < simpleTutorialPanels.Count; i++)
        {
            var step = simpleTutorialPanels[i];
            if (step == null)
                continue;

            if (step.nextButtonIndex < 0 || step.nextButtonIndex >= _uiButtonsCache.Length)
                continue;

            var btn = _uiButtonsCache[step.nextButtonIndex];
            if (btn == null)
                continue;

            btn.interactable = false;
        }

        if (_simplePanelIndex < 0 || _simplePanelIndex >= simpleTutorialPanels.Count)
            return;

        var currentStep = simpleTutorialPanels[_simplePanelIndex];
        if (currentStep == null)
            return;

        if (currentStep.nextButtonIndex < 0 || currentStep.nextButtonIndex >= _uiButtonsCache.Length)
            return;

        var currentBtn = _uiButtonsCache[currentStep.nextButtonIndex];
        if (currentBtn == null)
            return;

        currentBtn.interactable = true;

        if (_tutorialNextButton != null)
        {
            bool currentUsesTutorialNext = (currentBtn == _tutorialNextButton);

            _tutorialNextButton.gameObject.SetActive(currentUsesTutorialNext);
            _tutorialNextButton.interactable = currentUsesTutorialNext;
        }
    }
}
