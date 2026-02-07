using System;
using UnityEngine;

[Serializable]
public class SimpleTutorialPanelStep
{
    [Header("Panel (Scene Object)")]
    public GameObject panelRoot;

    [Header("Step Config (Data)")]
    public SimpleTutorialStepConfig config;
}
