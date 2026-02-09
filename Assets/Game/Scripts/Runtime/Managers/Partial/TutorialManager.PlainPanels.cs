using System;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using static CoinType;

public partial class TutorialManager
{
    private void Update()
    {
        if (!IsPlainMode)
            return;

        if (_plainPanelIndex < 0 || plainTutorials == null || _plainPanelIndex >= plainTutorials.Count)
            return;

        TrySubscribePlacementManager();
        TrySubscribeMonsterPoopClean();

        var step = plainTutorials[_plainPanelIndex];
        var config = step != null ? step.config : null;
        if (config == null)
            return;

        if (_tutorialMonsterRect == null)
        {
            var monsterManager = ServiceLocator.Get<MonsterManager>();
            if (monsterManager != null &&
                monsterManager.activeMonsters != null &&
                monsterManager.activeMonsters.Count > 0)
            {
                var controller = monsterManager.activeMonsters[0];
                if (controller != null)
                {
                    _tutorialMonsterRect = controller.GetComponent<RectTransform>();
                    SetupTutorialMonsterController(controller);

                    MoveTutorialMonsterForPlainStep(step);
                    UpdateRightClickMouseHintForPlainStep(step);
                }
            }
        }

        if (config.usePointer)
        {
            UpdatePointerForPlainStep(step);
        }

        UpdateCurrentHandPointerOffsetRealtime();

        UpdatePlainStepNextButtonsInteractable();
    }

    public void RefreshCurrentPlainPointer()
    {
        if (!IsPlainMode)
            return;

        if (plainTutorials == null || plainTutorials.Count == 0)
            return;

        if (_plainPanelIndex < 0 || _plainPanelIndex >= plainTutorials.Count)
            return;

        UpdatePointerForPlainStep(plainTutorials[_plainPanelIndex]);
    }

    private void StartPlainTutorialSequence()
    {
        if (plainTutorials == null || plainTutorials.Count == 0)
        {
            Debug.LogWarning("TutorialManager: plainTutorials kosong, tidak ada tutorial plain yang ditampilkan.");
            return;
        }

        if (_tutorialMonsterController != null)
        {
            _tutorialMonsterController.SetInteractionsDisabledByTutorial(true);
        }

        for (int i = 0; i < plainTutorials.Count; i++)
        {
            var step = plainTutorials[i];
            if (step == null)
                continue;

            if (step.panelRoot != null)
                step.panelRoot.SetActive(false);

            var nextButton = GetPlainStepNextButton(step);
            if (nextButton != null)
            {
                if (!_plainNextButtonsHooked.Contains(nextButton))
                {
                    _plainNextButtonsHooked.Add(nextButton);
                    nextButton.onClick.AddListener(RequestNextPlainPanel);
                }
            }
        }

        _plainPanelIndex = 0;
        var firstStep = plainTutorials[_plainPanelIndex];
        var firstConfig = firstStep != null ? firstStep.config : null;
        if (firstStep != null && firstStep.panelRoot != null && firstConfig != null)
        {
            PlayPlainPanelShowAnimation(firstStep.panelRoot);
            _plainStepShownTime = Time.time;
            _foodDropCountForCurrentStep = 0;

            UpdateTutorialMonsterMovementForPlainStep(firstStep);
            MoveTutorialMonsterForPlainStep(firstStep);

            UpdatePointerForPlainStep(firstStep);
            UpdateRightClickMouseHintForPlainStep(firstStep);
            PlayPlainStepEffectForIndex(_plainPanelIndex);

            ApplyTutorialMonsterPoopForPlainStep(firstStep);
            ShowMonsterInfoForPlainStep(firstStep);

            if (firstConfig.handPointerSequence != null)
            {
                StartHandPointerSubTutorial(firstStep);
            }
            else
            {
                UpdatePlainStepNextButtonsInteractable();
            }
        }
    }

    public void ShowNextPlainPanel()
    {
        if (plainTutorials == null || plainTutorials.Count == 0)
            return;

        if (_plainPanelIndex < 0)
        {
            StartPlainTutorialSequence();
            return;
        }

        if (_plainPanelIndex < plainTutorials.Count)
        {
            var currentStep = plainTutorials[_plainPanelIndex];
            var currentConfig = currentStep != null ? currentStep.config : null;

            if (currentStep != null && currentStep.panelRoot != null)
                currentStep.panelRoot.SetActive(false);

            HideRightClickMouseHint();
            UpdateTutorialMonsterMovementForPlainStep(null);

            if (currentConfig != null && currentConfig.hideInventoryOnNext)
            {
                var inventory = ServiceLocator.Get<ItemInventoryUI>();
                if (inventory != null)
                {
                    inventory.HideInventory();
                    inventory.ResetInventoryGroupvisibility();
                    inventory.ExitDeleteMode();
                }
            }
        }

        _plainPanelIndex++;
        Debug.Log($"TutorialManager: moving to plain panel index {_plainPanelIndex}");

        if (_plainPanelIndex >= plainTutorials.Count)
        {
            MarkPlainTutorialCompleted();

            HidePointerIfAny();
            RestoreUIManagerButtonsInteractable();
            if (_tutorialMonsterController != null)
            {
                _tutorialMonsterController.SetInteractionsDisabledByTutorial(false);
            }
            gameObject.SetActive(false);
            return;
        }

        var nextStep = plainTutorials[_plainPanelIndex];
        var nextConfig = nextStep != null ? nextStep.config : null;
        if (nextStep != null && nextStep.panelRoot != null && nextConfig != null)
        {
            PlayPlainPanelShowAnimation(nextStep.panelRoot);
            _plainStepShownTime = Time.time;
            _foodDropCountForCurrentStep = 0;

            UpdateTutorialMonsterMovementForPlainStep(nextStep);
            ApplyTutorialMonsterHungerForPlainStep(nextStep);
            MoveTutorialMonsterForPlainStep(nextStep);

            UpdatePointerForPlainStep(nextStep);
            UpdateRightClickMouseHintForPlainStep(nextStep);
            PlayPlainStepEffectForIndex(_plainPanelIndex);

            ApplyTutorialMonsterPoopForPlainStep(nextStep);
            ShowMonsterInfoForPlainStep(nextStep);

            if (nextConfig.handPointerSequence != null)
            {
                StartHandPointerSubTutorial(nextStep);
            }
            else
            {
                UpdatePlainStepNextButtonsInteractable();
            }
        }
    }

    private void UpdateTutorialMonsterMovementForPlainStep(PlainTutorialPanelStep step)
    {
        if (_tutorialMonsterController == null)
            return;

        var config = step != null ? step.config : null;
        bool shouldFreeze = config != null && config.freezeTutorialMonsterMovement;
        _tutorialMonsterController.SetMovementFrozenByTutorial(shouldFreeze);
    }

    private void ApplyTutorialMonsterHungerForPlainStep(PlainTutorialPanelStep step)
    {
        var config = step != null ? step.config : null;
        if (_tutorialMonsterController == null || config == null)
            return;

        if (!config.makeTutorialMonsterHungry)
            return;

        float currentHunger = _tutorialMonsterController.StatsHandler?.CurrentHunger ?? 100f;
        float newHunger = currentHunger - config.hungryReduceAmount;
        _tutorialMonsterController.SetHunger(newHunger);
    }

    private void ApplyTutorialMonsterPoopForPlainStep(PlainTutorialPanelStep step)
    {
        var config = step != null ? step.config : null;
        if (_tutorialMonsterController == null || config == null)
            return;

        if (!config.dropPoopOnStepStart)
            return;

        _tutorialMonsterController.DropPoop(PoopType.Normal);
    }

    private void MoveTutorialMonsterForPlainStep(PlainTutorialPanelStep step)
    {
        if (_tutorialMonsterController == null || _tutorialMonsterRect == null || step == null)
            return;

        var config = step.config;
        if (config == null || !config.moveTutorialMonsterToTarget)
            return;

        if (string.IsNullOrEmpty(config.monsterTargetId))
            return;
        TutorialMonsterTargetMarker[] markers = FindObjectsOfType<TutorialMonsterTargetMarker>(true);
        TutorialMonsterTargetMarker targetMarker = null;

        for (int i = 0; i < markers.Length; i++)
        {
            var marker = markers[i];
            if (marker != null && marker.isActiveAndEnabled && string.Equals(marker.id, config.monsterTargetId, StringComparison.Ordinal))
            {
                targetMarker = marker;
                break;
            }
        }

        if (targetMarker == null)
            return;

        var targetRect = targetMarker.transform as RectTransform;
        if (targetRect == null)
            return;

        var canvas = _tutorialMonsterRect.GetComponentInParent<Canvas>();
        if (canvas == null)
            return;

        var cam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
        var parentRect = _tutorialMonsterRect.parent as RectTransform;
        if (parentRect == null)
            return;

        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(cam, targetRect.position);
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, screenPos, cam, out var localPos))
        {
            _tutorialMonsterRect.anchoredPosition = localPos;
        }
    }

    private void ShowMonsterInfoForPlainStep(PlainTutorialPanelStep step)
    {
        var config = step != null ? step.config : null;
        if (_tutorialMonsterController == null || config == null)
            return;

        if (!config.showMonsterInfoOnStepStart)
            return;

        _tutorialMonsterController.UI?.ShowMonsterInfo();
    }

    private void UpdatePointerForPlainStep(PlainTutorialPanelStep step)
    {
        var pointer = ServiceLocator.Get<ITutorialPointer>();
        if (pointer == null)
            return;
        var config = step != null ? step.config : null;

        bool wantsUIHand = config != null && config.useUIManagerButtonHandPointer;
        bool wantsPointer = config != null && (config.usePointer || wantsUIHand);

        if (!wantsPointer)
        {
            pointer.Hide();
            return;
        }

        if (config != null && config.useTutorialMonsterAsPointerTarget && _tutorialMonsterRect != null)
        {
            pointer.PointTo(_tutorialMonsterRect, config.pointerOffset);
            return;
        }

        if (config != null && config.usePoopCleanAsNext)
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
                        pointer.PointTo(poopRect, config.pointerOffset);
                        return;
                    }
                }
            }
        }

        if ((config != null && config.useNextButtonAsPointerTarget) || wantsUIHand)
        {
            var btn = GetPlainStepNextButton(step);
            if (btn != null)
            {
                var rect = btn.transform as RectTransform;
                if (rect != null)
                {
                    pointer.PointTo(rect, config.pointerOffset);
                    return;
                }
            }
        }

        pointer.Hide();
    }

    private void PlayPlainPanelShowAnimation(GameObject panel)
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

        rect.DOAnchorPos(targetPos, plainPanelShowDuration).SetEase(plainPanelShowEase);
        canvasGroup.DOFade(1f, plainPanelShowDuration).SetEase(Ease.OutQuad);
    }

    private void UpdateRightClickMouseHintForPlainStep(PlainTutorialPanelStep step)
    {
        var config = step != null ? step.config : null;
        if (config == null || !config.showRightClickMouseHint)
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
        _rightClickMouseHintInstance.anchoredPosition = petPos + baseOffset + config.rightClickMouseHintOffset;

        PlayPlainPanelShowAnimation(_rightClickMouseHintInstance.gameObject);
    }

    private void HideRightClickMouseHint()
    {
        if (_rightClickMouseHintInstance != null)
        {
            _rightClickMouseHintInstance.gameObject.SetActive(false);
        }
    }

    public void ToggleInventoryFromPlainTutorial()
    {
        var inventory = ServiceLocator.Get<ItemInventoryUI>();
        if (inventory == null)
        {
            Debug.LogWarning("TutorialManager: ItemInventoryUI tidak ditemukan saat ToggleInventoryFromPlainTutorial.");
            return;
        }

        var canvasGroup = inventory.GetComponent<CanvasGroup>();
        bool isVisible = canvasGroup != null && canvasGroup.interactable && canvasGroup.alpha > 0.5f;

        if (isVisible)
        {
            inventory.HideInventory();
            inventory.ResetInventoryGroupvisibility();
            inventory.ExitDeleteMode();
        }
        else
        {
            inventory.ShowInventory();
            inventory.ResetInventoryGroupvisibility();
        }

        RequestNextPlainPanel();
    }

    private void OnTutorialMonsterClicked(PointerEventData eventData)
    {
        if (!IsPlainMode)
            return;

        if (plainTutorials == null || plainTutorials.Count == 0)
            return;

        if (_plainPanelIndex < 0 || _plainPanelIndex >= plainTutorials.Count)
            return;

        var step = plainTutorials[_plainPanelIndex];
        var config = step != null ? step.config : null;
        if (step == null || config == null)
            return;

        if (config.useFoodDropAsNext)
            return;

        if (config.useUIManagerButtonHandPointer)
            return;

        bool wantsLeft = config.useLeftClickPetAsNext;
        bool wantsRight = config.useRightClickPetAsNext;
        if (!wantsLeft && !wantsRight)
            return;

        const float minClickDelay = 0.15f;
        if (Time.time - _plainStepShownTime < minClickDelay)
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
            RequestNextPlainPanel();
        }
    }

    private void UpdatePlainStepNextButtonsInteractable()
    {
        if (plainTutorials == null || plainTutorials.Count == 0)
            return;

        if (_uiButtonsCache == null || _uiButtonsCache.Length == 0)
        {
            CacheUIButtonsFromUIManager();
        }

        if (_uiButtonsCache == null || _uiButtonsCache.Length == 0)
            return;

        for (int i = 0; i < plainTutorials.Count; i++)
        {
            var step = plainTutorials[i];
            var config = step != null ? step.config : null;
            if (step == null || config == null)
                continue;

            if (config.nextButtonIndex < 0 || config.nextButtonIndex >= _uiButtonsCache.Length)
                continue;

            var btn = _uiButtonsCache[config.nextButtonIndex];
            if (btn == null)
                continue;

            if (_isRunningHandPointerSubTutorial && _currentHandPointerTargetButton == btn)
                continue;

            btn.interactable = false;
        }

        if (_plainPanelIndex < 0 || _plainPanelIndex >= plainTutorials.Count)
            return;

        var currentStep = plainTutorials[_plainPanelIndex];
        var currentConfig = currentStep != null ? currentStep.config : null;
        if (currentStep == null || currentConfig == null)
            return;

        if (currentConfig.useFoodDropAsNext || currentConfig.usePoopCleanAsNext)
            return;

        if (currentConfig.nextButtonIndex < 0 || currentConfig.nextButtonIndex >= _uiButtonsCache.Length)
            return;

        var currentBtn = _uiButtonsCache[currentConfig.nextButtonIndex];
        if (currentBtn == null)
            return;

        bool allowInteract = true;
        if (currentConfig.minNextClickDelay > 0f)
        {
            if (Time.time - _plainStepShownTime < currentConfig.minNextClickDelay)
            {
                allowInteract = false;
            }
        }

        if (_isRunningHandPointerSubTutorial && _currentHandPointerTargetButton == currentBtn)
        {
            currentBtn.interactable = true;
        }
        else
        {
            currentBtn.interactable = allowInteract;
        }

        if (currentConfig.useUIManagerButtonHandPointer)
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
