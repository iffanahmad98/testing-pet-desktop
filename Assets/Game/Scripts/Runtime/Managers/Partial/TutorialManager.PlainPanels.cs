using System;
using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;
using static CoinType;
using System.Collections;

public partial class TutorialManager
{

    public void RefreshCurrentPlainPointer()
    {
        if (_currentMode != TutorialMode.Plain)
            return;

        if (plainTutorials == null || plainTutorials.Count == 0)
            return;

        if (_plainPanelIndex < 0 || _plainPanelIndex >= plainTutorials.Count)
            return;

        UpdatePointerForPlainStep(plainTutorials[_plainPanelIndex]);
    }

    public void Log()
    {
        Debug.Log("Clicked");
    }

    private void StartPlainTutorialSequence()
    {
        Debug.Log("TutorialManager: Memulai urutan tutorial plain.");
        if (plainTutorials == null || plainTutorials.Count == 0)
        {
            Debug.LogWarning("TutorialManager: plainTutorials kosong, tidak ada tutorial plain yang ditampilkan.");
            return;
        }

        Debug.Log($"[PlainTutorial] Jumlah step plain = {plainTutorials.Count}");

        if (_tutorialMonsterController != null)
        {
            _tutorialMonsterController.SetInteractionsDisabledByTutorial(true);
        }

        for (int i = 0; i < plainTutorials.Count; i++)
        {
            var step = plainTutorials[i];
            if (step == null)
            {
                Debug.LogWarning($"[PlainTutorial] Step index {i} null, dilewati.");
                continue;
            }

            if (step.panelRoot != null)
            {
                Debug.Log($"[PlainTutorial] Step index {i} panelRoot = {step.panelRoot.name}");
                step.panelRoot.SetActive(false);
            }
            else
            {
                Debug.LogWarning($"[PlainTutorial] Step index {i} tidak punya panelRoot.");
            }

            var nextButton = GetPlainStepNextButton(step);
            if (nextButton != null)
            {
                if (!_plainNextButtonsHooked.Contains(nextButton))
                {
                    _plainNextButtonsHooked.Add(nextButton);
                    nextButton.onClick.AddListener(RequestNextPlainPanel);
                    Debug.Log($"[PlainTutorial] Hook RequestNextPlainPanel ke nextButton '{nextButton.gameObject.name}' untuk step index {i}.");
                }
                else
                {
                    Debug.Log($"[PlainTutorial] Next button '{nextButton.gameObject.name}' untuk step index {i} sudah pernah di-hook, dilewati.");
                }
            }
            else
            {
                Debug.LogWarning($"[PlainTutorial] Next button untuk step index {i} tidak ditemukan (mungkin nextButtonIndex tidak valid).");
            }
        }

        _plainPanelIndex = 0;
        var firstStep = plainTutorials[_plainPanelIndex];
        var firstConfig = firstStep != null ? firstStep.config : null;
        if (firstStep != null && firstStep.panelRoot != null && firstConfig != null)
        {
            Debug.Log($"[PlainTutorial] Menampilkan firstStep index={_plainPanelIndex}, panel={firstStep.panelRoot.name}");
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

            EnsurePlainNextButtonListenerForStep(firstStep);

            if (firstConfig.handPointerSequence != null)
            {
                Debug.Log("[PlainTutorial] StartHandPointerPlainSubTutorial()");
                StartHandPointerPlainSubTutorial(firstStep);
            }
            else
            {
                Debug.Log("[PlainTutorial] UpdatePlainStepNextButtonsInteractable()");
                UpdatePlainStepNextButtonsInteractable();
                SetupPlainNextClickDelay(firstConfig);
            }
        }
        else
        {
            Debug.LogWarning($"[PlainTutorial] firstStep atau config atau panelRoot null (firstStep null = {firstStep == null}, panelRoot null = {firstStep?.panelRoot == null}, config null = {firstConfig == null}).");
        }
    }

    public void ShowNextPlainPanel()
    {
        Debug.Log(
            $"[PlainTutorial] ShowNextPlainPanel ENTER | " +
            $"index={_plainPanelIndex} | " +
            $"total={plainTutorials?.Count ?? 0} | " +
            $"mode={_currentMode}"
        );

        if (plainTutorials == null || plainTutorials.Count == 0)
        {
            Debug.LogWarning("[PlainTutorial] ABORT: plainTutorials null / empty");
            return;
        }

        if (_plainPanelIndex < 0)
        {
            Debug.Log("[PlainTutorial] index < 0 → StartPlainTutorialSequence()");
            StartPlainTutorialSequence();
            return;
        }

        if (_plainPanelIndex < plainTutorials.Count)
        {
            var currentStep = plainTutorials[_plainPanelIndex];
            var currentConfig = currentStep != null ? currentStep.config : null;

            Debug.Log(
                $"[PlainTutorial] Hiding current panel | " +
                $"index={_plainPanelIndex} | " +
                $"panel={(currentStep?.panelRoot != null ? currentStep.panelRoot.name : "NULL")}"
            );

            if (currentStep != null && currentStep.panelRoot != null)
                currentStep.panelRoot.SetActive(false);
            var previousNextButton = GetPlainStepNextButton(currentStep);
            if (previousNextButton != null)
            {
                previousNextButton.interactable = false;
                previousNextButton.gameObject.SetActive(false);
            }

            HideRightClickMouseHint();
            UpdateTutorialMonsterMovementForPlainStep(null);

            if (currentConfig != null && currentConfig.hideInventoryOnNext)
            {
                Debug.Log("[PlainTutorial] hideInventoryOnNext = TRUE");

                var inventory = ServiceLocator.Get<ItemInventoryUI>();
                if (inventory != null)
                {
                    inventory.HideInventory();
                    inventory.ResetInventoryGroupvisibility();
                    inventory.ExitDeleteMode();
                }
                else
                {
                    Debug.LogWarning("[PlainTutorial] ItemInventoryUI NOT FOUND");
                }
            }
        }

        _plainPanelIndex++;
        Debug.Log($"[PlainTutorial] Increment index → {_plainPanelIndex}");

        if (_plainPanelIndex >= plainTutorials.Count)
        {
            Debug.Log("[PlainTutorial] Tutorial FINISHED");

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

        Debug.Log(
            $"[PlainTutorial] Showing NEXT panel | " +
            $"index={_plainPanelIndex} | " +
            $"panel={(nextStep?.panelRoot != null ? nextStep.panelRoot.name : "NULL")} | " +
            $"handPointer={(nextConfig?.handPointerSequence != null)}"
        );

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

            EnsurePlainNextButtonListenerForStep(nextStep);

            if (nextConfig.handPointerSequence != null)
            {
                Debug.Log("[PlainTutorial] StartHandPointerPlainSubTutorial()");
                StartHandPointerPlainSubTutorial(nextStep);
            }
            else
            {
                Debug.Log("[PlainTutorial] UpdatePlainStepNextButtonsInteractable()");
                UpdatePlainStepNextButtonsInteractable();
                SetupPlainNextClickDelay(nextConfig);
            }
        }
        else
        {
            Debug.LogWarning("[PlainTutorial] NEXT STEP INVALID (step / panel / config null)");
        }
    }

    private void EnsurePlainNextButtonListenerForStep(PlainTutorialPanelStep step)
    {
        if (step == null)
            return;

        var btn = GetPlainStepNextButton(step);
        if (btn == null)
            return;

        btn.onClick.RemoveListener(RequestNextPlainPanel);
        btn.onClick.AddListener(RequestNextPlainPanel);
    }

    private IEnumerator EnablePlainNextButtonAfterDelay(float delay, int stepIndex)
    {
        if (delay <= 0f)
            yield break;

        yield return new WaitForSeconds(delay);

        if (_currentMode != TutorialMode.Plain)
            yield break;

        if (stepIndex < 0 || stepIndex >= plainTutorials.Count)
            yield break;

        if (_plainPanelIndex != stepIndex)
            yield break;

        var step = plainTutorials[stepIndex];
        if (step == null || step.config == null)
            yield break;

        var config = step.config;
        if (config.useFoodDropAsNext || config.usePoopCleanAsNext)
            yield break;

        var btn = GetPlainStepNextButton(step);
        if (btn == null)
            yield break;

        btn.interactable = true;

        if (_tutorialNextButton != null && btn == _tutorialNextButton)
        {
            _tutorialNextButton.gameObject.SetActive(true);
            _tutorialNextButton.interactable = true;
        }
    }

    private void SetupPlainNextClickDelay(PlainTutorialStepConfig config)
    {
        if (config == null)
            return;

        if (_plainNextClickDelayRoutine != null)
        {
            StopCoroutine(_plainNextClickDelayRoutine);
            _plainNextClickDelayRoutine = null;
        }

        if (config.minNextClickDelay <= 0f)
            return;

        if (config.useFoodDropAsNext || config.usePoopCleanAsNext)
            return;

        _plainNextClickDelayRoutine = StartCoroutine(EnablePlainNextButtonAfterDelay(config.minNextClickDelay, _plainPanelIndex));
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
        Debug.Log("ApplyTutorialMonsterPoopForPlainStep ENTER");
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
        if (_currentMode != TutorialMode.Plain)
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

            if (_isRunningHandPointerSubTutorial && _targetingContext?.CurrentButton == btn)
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

        if (_isRunningHandPointerSubTutorial && _targetingContext?.CurrentButton == currentBtn)
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
