using System;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using static CoinType;

public partial class TutorialManager
{
    private void Update()
    {
        if (!IsSimpleMode)
            return;

        if (_simplePanelIndex < 0 || simpleTutorialPanels == null || _simplePanelIndex >= simpleTutorialPanels.Count)
            return;

        TrySubscribePlacementManager();
        TrySubscribeMonsterPoopClean();

        var step = simpleTutorialPanels[_simplePanelIndex];
        if (step == null)
            return;

        if (step.usePointer)
        {
            UpdatePointerForSimpleStep(step);
        }
        UpdateSimpleStepNextButtonsInteractable();
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

        if (_tutorialMonsterController != null)
        {
            _tutorialMonsterController.SetInteractionsDisabledByTutorial(true);
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
            _foodDropCountForCurrentStep = 0;

            UpdateRightClickMouseHintForSimpleStep(firstStep);
            PlaySimpleStepEffectForIndex(_simplePanelIndex);

            UpdateTutorialMonsterMovementForSimpleStep(firstStep);

            ApplyTutorialMonsterPoopForSimpleStep(firstStep);

            ShowMonsterInfoForSimpleStep(firstStep);

            if (firstStep.handPointerSequence != null)
            {
                StartHandPointerSubTutorial(firstStep);
            }
            else
            {
                UpdateSimpleStepNextButtonsInteractable();
            }
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

            UpdateTutorialMonsterMovementForSimpleStep(null);
        }

        _simplePanelIndex++;
        Debug.Log($"TutorialManager: moving to simple panel index {_simplePanelIndex}");

        if (_simplePanelIndex >= simpleTutorialPanels.Count)
        {
            MarkSimpleTutorialCompleted();

            HidePointerIfAny();
            RestoreUIManagerButtonsInteractable();
            if (_tutorialMonsterController != null)
            {
                _tutorialMonsterController.SetInteractionsDisabledByTutorial(false);
            }
            gameObject.SetActive(false);
            return;
        }

        var nextStep = simpleTutorialPanels[_simplePanelIndex];
        if (nextStep != null && nextStep.panelRoot != null)
        {
            PlaySimplePanelShowAnimation(nextStep.panelRoot);
            UpdatePointerForSimpleStep(nextStep);
            _simpleStepShownTime = Time.time;
            _foodDropCountForCurrentStep = 0;

            UpdateRightClickMouseHintForSimpleStep(nextStep);
            PlaySimpleStepEffectForIndex(_simplePanelIndex);

            UpdateTutorialMonsterMovementForSimpleStep(nextStep);
            ApplyTutorialMonsterHungerForSimpleStep(nextStep);
            ApplyTutorialMonsterPoopForSimpleStep(nextStep);
            ShowMonsterInfoForSimpleStep(nextStep);

            if (nextStep.handPointerSequence != null)
            {
                StartHandPointerSubTutorial(nextStep);
            }
            else
            {
                UpdateSimpleStepNextButtonsInteractable();
            }
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

        float currentHunger = _tutorialMonsterController.StatsHandler?.CurrentHunger ?? 100f;
        float newHunger = currentHunger - step.hungryReduceAmount;
        _tutorialMonsterController.SetHunger(newHunger);
    }

    private void ApplyTutorialMonsterPoopForSimpleStep(SimpleTutorialPanelStep step)
    {
        if (_tutorialMonsterController == null || step == null)
            return;

        if (!step.dropPoopOnStepStart)
            return;

        _tutorialMonsterController.DropPoop(PoopType.Normal);
    }

    private void ShowMonsterInfoForSimpleStep(SimpleTutorialPanelStep step)
    {
        if (_tutorialMonsterController == null || step == null)
            return;

        if (!step.showMonsterInfoOnStepStart)
            return;

        _tutorialMonsterController.UI?.ShowMonsterInfo();
    }

    private void UpdatePointerForSimpleStep(SimpleTutorialPanelStep step)
    {
        var pointer = ServiceLocator.Get<ITutorialPointer>();
        if (pointer == null)
            return;

        bool wantsUIHand = step != null && step.useUIManagerButtonHandPointer;
        bool wantsPointer = step != null && (step.usePointer || wantsUIHand);

        if (!wantsPointer)
        {
            pointer.Hide();
            return;
        }

        if (step.useTutorialMonsterAsPointerTarget && _tutorialMonsterRect != null)
        {
            pointer.PointTo(_tutorialMonsterRect, step.pointerOffset);
            return;
        }

        if (step.usePoopCleanAsNext)
        {
            var monsterManager = ServiceLocator.Get<MonsterManager>();
            if (monsterManager != null &&
                monsterManager.activePoops != null &&
                monsterManager.activePoops.Count > 0)
            {
                var poop = monsterManager.activePoops[0];
                if (poop != null)
                {
                    var poopRect = poop.GetComponentInChildren<RectTransform>();
                    if (poopRect != null)
                    {
                        pointer.PointTo(poopRect, step.pointerOffset);
                        return;
                    }
                }
            }
        }

        if (step.useNextButtonAsPointerTarget || wantsUIHand)
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

        if (step.useFoodDropAsNext)
            return;

        if (step.useUIManagerButtonHandPointer)
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
            if (_tutorialMonsterController != null)
            {
                _tutorialMonsterController.DropCoin(Gold);
            }
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

        if (currentStep.useFoodDropAsNext || currentStep.usePoopCleanAsNext)
            return;

        if (currentStep.nextButtonIndex < 0 || currentStep.nextButtonIndex >= _uiButtonsCache.Length)
            return;

        var currentBtn = _uiButtonsCache[currentStep.nextButtonIndex];
        if (currentBtn == null)
            return;

        bool allowInteract = true;
        if (currentStep.minNextClickDelay > 0f)
        {
            if (Time.time - _simpleStepShownTime < currentStep.minNextClickDelay)
            {
                allowInteract = false;
            }
        }
        currentBtn.interactable = allowInteract;

        if (currentStep.useUIManagerButtonHandPointer)
        {
            if (!currentBtn.gameObject.activeSelf)
                currentBtn.gameObject.SetActive(true);

            currentBtn.interactable = true;
        }

        if (_tutorialNextButton != null)
        {
            bool currentUsesTutorialNext = (currentBtn == _tutorialNextButton);

            _tutorialNextButton.gameObject.SetActive(currentUsesTutorialNext);
            _tutorialNextButton.interactable = currentUsesTutorialNext && allowInteract;
        }
    }
}
