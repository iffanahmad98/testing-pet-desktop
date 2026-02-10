using TMPro;
using UnityEngine;

public class FarmTutorialStepData : MonoBehaviour
{
    public string title;
    public string body;
    public Vector2 scrollTo;
    public bool isNextButtonActive = false;

    public void WriteInstruction(TextMeshProUGUI titleText, TextMeshProUGUI bodyText)
    {
        titleText.text = title;
        bodyText.text = body;
    }

    public void DeletePreviousStep()
    { }

    public void RunCurrentStep()
    { }
}
