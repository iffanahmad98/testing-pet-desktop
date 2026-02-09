using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public partial class TutorialManager
{
    private void ShowNextHotelPanel()
    {
        if (_currentMode != TutorialMode.Hotel)
            return;

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
            if (currentStep != null && currentStep.panelRoot != null)
                currentStep.panelRoot.SetActive(false);
        }

        _hotelPanelIndex++;
        Debug.Log($"TutorialManager: moving to hotel panel index {_hotelPanelIndex}");

        if (_hotelPanelIndex >= hotelTutorials.Count)
        {
            gameObject.SetActive(false);
            return;
        }

        var nextStep = hotelTutorials[_hotelPanelIndex];
        if (nextStep != null && nextStep.panelRoot != null)
        {
            nextStep.panelRoot.SetActive(true);
            _hotelStepShownTime = Time.time;
        }
    }

    private void StartHotelTutorialSequence()
    {
        if (_currentMode != TutorialMode.Hotel)
            return;

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
        }

        _hotelPanelIndex = 0;
        var firstStep = hotelTutorials[_hotelPanelIndex];
        if (firstStep != null && firstStep.panelRoot != null)
        {
            firstStep.panelRoot.SetActive(true);
            _hotelStepShownTime = Time.time;
        }
    }
}
