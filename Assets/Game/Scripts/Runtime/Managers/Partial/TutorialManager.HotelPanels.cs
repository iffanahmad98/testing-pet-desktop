using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public partial class TutorialManager
{

    public void ShowNextHotelPanel()
    {
        if (hotelTutorials == null || hotelTutorials.Count == 0)
            return;

        if (_hotelPanelIndex < 0)
        {
            StartHotelTutorialSequence();
            return;
        }

        if (_hotelPanelIndex < hotelTutorials.Count)
        {
            var currentStep = hotelTutorials[_hotelPanelIndex];
            var currentConfig = currentStep != null ? currentStep.config : null;
            if (currentStep != null && currentStep.panelRoot != null)
                currentStep.panelRoot.SetActive(false);
        }

        _hotelPanelIndex++;
        Debug.Log($"TutorialManager: moving to hotel panel index {_hotelPanelIndex}");

        if (_hotelPanelIndex >= hotelTutorials.Count)
        {
            MarkHotelTutorialCompleted();
            gameObject.SetActive(false);
            return;
        }

        var nextStep = hotelTutorials[_hotelPanelIndex];
        var nextConfig = nextStep != null ? nextStep.config : null;
        if (nextStep != null && nextStep.panelRoot != null && nextConfig != null)
        {
            PlayHotelPanelShowAnimation(nextStep.panelRoot);
            _hotelStepShownTime = Time.time;
            if (nextConfig.handPointerSequence != null)
            {
                StartHandPointerHotelSubTutorial(nextStep);
            }
            else
            {
                UpdateHotelStepNextButtonsInteractable();
            }
        }
    }

    private void StartHotelTutorialSequence()
    {
        if (hotelTutorials == null || hotelTutorials.Count == 0)
        {
            Debug.LogWarning("TutorialManager: hotelTutorials kosong, tidak ada tutorial hotel yang ditampilkan.");
            return;
        }

        for (int i = 0; i < hotelTutorials.Count; i++)
        {
            var step = hotelTutorials[i];
            if (step == null)
                continue;

            if (step.panelRoot != null)
                step.panelRoot.SetActive(false);

            var nextButton = GetHotelStepNextButton(step);
            if (nextButton != null)
            {
                nextButton.onClick.RemoveListener(ShowNextHotelPanel);
                nextButton.onClick.AddListener(ShowNextHotelPanel);
            }
        }

        _hotelPanelIndex = 0;
        var firstStep = hotelTutorials[_hotelPanelIndex];
        var firstConfig = firstStep != null ? firstStep.config : null;
        if (firstStep != null && firstStep.panelRoot != null && firstConfig != null)
        {
            PlayHotelPanelShowAnimation(firstStep.panelRoot);
            _hotelStepShownTime = Time.time;
            if (firstConfig.handPointerSequence != null)
            {
                StartHandPointerHotelSubTutorial(firstStep);
            }
            else
            {
                UpdateHotelStepNextButtonsInteractable();
            }
        }
    }

    private void UpdatePointerForHotelStep(HotelTutorialPanelStep step)
    {
        var pointer = ServiceLocator.Get<ITutorialPointer>();
        if (pointer == null)
            return;
        var config = step != null ? step.config : null;

        bool wantsPointer = config != null && config.usePointer;
        if (!wantsPointer)
        {
            pointer.Hide();
            return;
        }

        if (config != null && config.useNextButtonAsPointerTarget)
        {
            var btn = GetHotelStepNextButton(step);
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

    private void PlayHotelPanelShowAnimation(GameObject panel)
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

    private void UpdateHotelStepNextButtonsInteractable()
    {
        if (hotelTutorials == null || hotelTutorials.Count == 0)
            return;

        if (_uiButtonsCache == null || _uiButtonsCache.Length == 0)
        {
            CacheUIButtonsFromUIManager();
        }

        if (_uiButtonsCache == null || _uiButtonsCache.Length == 0)
            return;

        for (int i = 0; i < hotelTutorials.Count; i++)
        {
            var step = hotelTutorials[i];
            var config = step != null ? step.config : null;
            if (step == null || config == null)
                continue;

            var btn = GetButtonByName(config.nextButtonName);
            if (btn == null)
                continue;

            btn.interactable = false;
        }

        if (_hotelPanelIndex < 0 || _hotelPanelIndex >= hotelTutorials.Count)
            return;

        var currentStep = hotelTutorials[_hotelPanelIndex];
        var currentConfig = currentStep != null ? currentStep.config : null;
        if (currentStep == null || currentConfig == null)
            return;

        var currentBtn = GetButtonByName(currentConfig.nextButtonName);
        if (currentBtn == null)
            return;

        currentBtn.interactable = true;
    }

    private Button GetHotelStepNextButton(HotelTutorialPanelStep step)
    {
        if (step == null)
            return null;

        var config = step.config;
        if (config == null)
            return null;

        if (_uiButtonsCache == null || _uiButtonsCache.Length == 0)
        {
            CacheUIButtonsFromUIManager();
        }

        if (_uiButtonsCache == null || _uiButtonsCache.Length == 0)
            return null;

        return GetButtonByName(config.nextButtonName);
    }
    public Button GetButtonByName(string buttonName)
    {
        if (string.IsNullOrEmpty(buttonName) || _uiButtonsCache == null)
            return null;
        for (int i = 0; i < _uiButtonsCache.Length; i++)
        {
            var btn = _uiButtonsCache[i];
            if (btn != null && btn.gameObject.name == buttonName)
                return btn;
        }
        return null;
    }
}
