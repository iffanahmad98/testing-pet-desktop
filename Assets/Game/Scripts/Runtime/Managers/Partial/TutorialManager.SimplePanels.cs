using System;
using UnityEngine;
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

        if (!step.useLeftClickPetAsNext && !step.useRightClickPetAsNext)
            return;

        const float minClickDelay = 0.15f;
        if (Time.time - _simpleStepShownTime < minClickDelay)
            return;

        int wantedButton = -1;
        if (step.useLeftClickPetAsNext && Input.GetMouseButtonDown(0))
            wantedButton = 0;
        else if (step.useRightClickPetAsNext && Input.GetMouseButtonDown(1))
            wantedButton = 1;

        if (wantedButton == -1)
            return;

        if (IsClickOnTutorialMonster())
        {
            RequestNextSimplePanel();
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
            if (nextButton != null && !step.useLeftClickPetAsNext && !step.useRightClickPetAsNext)
            {
                if (!_simpleNextButtonsHooked.Contains(nextButton))
                {
                    _simpleNextButtonsHooked.Add(nextButton);
                    nextButton.onClick.AddListener(RequestNextSimplePanel);
                }
            }
        }

        _simplePanelIndex = 0;
        if (simpleTutorialPanels[_simplePanelIndex] != null && simpleTutorialPanels[_simplePanelIndex].panelRoot != null)
        {
            PlaySimplePanelShowAnimation(simpleTutorialPanels[_simplePanelIndex].panelRoot);
            UpdatePointerForSimpleStep(simpleTutorialPanels[_simplePanelIndex]);
            _simpleStepShownTime = Time.time;

            UpdateRightClickMouseHintForSimpleStep(simpleTutorialPanels[_simplePanelIndex]);
            PlaySimpleStepEffectForIndex(_simplePanelIndex);
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

            // Sembunyikan hint mouse ketika meninggalkan step ini
            HideRightClickMouseHint();
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
        }
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

        // Jika diminta menunjuk ke monster tutorial (mis. Briabit) dan referensinya ada,
        // gunakan itu sebagai target utama.
        if (step.useTutorialMonsterAsPointerTarget && _tutorialMonsterRect != null)
        {
            pointer.PointTo(_tutorialMonsterRect, step.pointerOffset);
            return;
        }

        // Fallback: gunakan tombol Next yang terdaftar lewat nextButtonIndex.
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

        // Posisi dasar: di atas pet, lalu ditambah offset dari konfigurasi step.
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
}
