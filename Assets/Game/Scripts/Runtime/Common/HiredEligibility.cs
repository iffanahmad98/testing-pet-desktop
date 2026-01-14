using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class HiredEligibility {
    public int hired = 0;
    public List<EligibilityRuleSO> rules = new();
}
