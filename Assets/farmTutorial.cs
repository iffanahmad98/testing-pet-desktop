using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FarmTutorial : MonoBehaviour
{
    [SerializeField] private int tutorialStepIndex = 0;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI bodyText;
    [SerializeField] private Button nextButton;

    [SerializeField] private FarmTutorialStepData[] stepData;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ExecuteTutorialAtStep();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void ExecuteTutorialAtStep(int step = 0)
    {
        // step is an index number
        if (stepData.Length < tutorialStepIndex)
        {
            Debug.LogWarning("Step is bigger than the stepData array");
            return;
        }

        stepData[step].DeletePreviousStep();

        if (nextButton != null)
        {
            nextButton.gameObject.SetActive(stepData[step].isNextButtonActive);
            nextButton.onClick.RemoveAllListeners();

            if (stepData[step].isNextButtonActive)
                nextButton.onClick.AddListener(OnNextButtonClicked);
        }

        stepData[step].WriteInstruction(titleText, bodyText);
    }

    private void OnNextButtonClicked()
    {
        tutorialStepIndex++;
        ExecuteTutorialAtStep(tutorialStepIndex);
    }
}
