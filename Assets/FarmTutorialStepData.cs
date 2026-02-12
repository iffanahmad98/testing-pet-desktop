using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FarmTutorialStepData : MonoBehaviour
{
    public string title;
    public string body;
    public Vector2 scrollTo;

    public bool isNextButtonActive = false;
    public bool showHandPointer = false;
    public Vector2 handPosition;
    public MenuBtn enabledButton = MenuBtn.None;

    public bool isBuySeeds = false;
    public string seedName = string.Empty;
    public int minimumCost = 0;
    public int seedBuyRequirement = 0;
    public string panelNameToClose = string.Empty;

    public bool isSelectSeed = false;
    public bool isPlantSeed = false;
    public int seedPlantRequirement = 0;
    public Vector3Int[] seedPlantingPos;

    public void WriteInstruction(TextMeshProUGUI titleText, TextMeshProUGUI bodyText)
    {
        titleText.text = title;
        bodyText.text = body;
    }

    public void DeletePreviousStep(Image handPointer)
    {
        handPointer.transform.localPosition = Vector3.zero;
        handPointer.gameObject.SetActive(false);
    }

    public void RunCurrentStep()
    { }
}
