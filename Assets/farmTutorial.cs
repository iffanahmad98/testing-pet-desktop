using TMPro;
using UnityEngine;

public class farmTutorial : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI bodyText;
    [SerializeField] private int tutorialStep = 1;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

public class FarmTutorialStepData
{
    public string titleText;
    public string bodyText;
    public Vector2 scrollTo;
}