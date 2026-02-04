using System;
using UnityEngine;

[Serializable]
public class SimpleTutorialPanelStep
{
    [Tooltip("Root panel untuk step tutorial sederhana ini.")]
    public GameObject panelRoot;

    [Tooltip("Index button Next di cache UIManager (TutorialManager). -1 = tidak pakai button.")]
    public int nextButtonIndex = -1;
}
