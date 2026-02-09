using System;
using UnityEngine;

[Serializable]
public class HotelTutorialPanelStep
{
    [Header("Panel (Scene Object)")]
    public GameObject panelRoot;

    [Header("Step Config (Data)")]
    public HotelTutorialStepConfig config;
}
