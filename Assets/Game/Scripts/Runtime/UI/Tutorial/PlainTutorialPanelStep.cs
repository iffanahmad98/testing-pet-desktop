using System;
using UnityEngine;

[Serializable]
public class PlainTutorialPanelStep
{
    [Header("Panel (Scene Object)")]
    public GameObject panelRoot;

    [Header("Step Config (Data)")]
    public PlainTutorialStepConfig config;
}
