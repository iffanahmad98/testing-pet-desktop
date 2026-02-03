using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TutorialDialogView : MonoBehaviour, ITutorialDialogView
{
    [Header("UI References")]
    [Tooltip("Text untuk nama speaker / karakter (opsional).")]
    [SerializeField] private TMP_Text speakerText;

    [Tooltip("Text utama isi dialog.")]
    [SerializeField] private TMP_Text bodyText;

    [Tooltip("Tombol untuk lanjut / close dialog.")]
    [SerializeField] private Button nextButton;

    [Tooltip("Text label tombol (opsional, bisa diubah antara 'Next' / 'OK').")]
    [SerializeField] private TMP_Text nextButtonLabel;

    public Button NextButton => nextButton;

    public void SetDialog(string speakerName, string text, bool isLastStep)
    {
        if (speakerText != null)
            speakerText.text = speakerName;

        if (bodyText != null)
            bodyText.text = text;

        if (nextButtonLabel != null)
            nextButtonLabel.text = isLastStep ? "OK" : "Next";
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
