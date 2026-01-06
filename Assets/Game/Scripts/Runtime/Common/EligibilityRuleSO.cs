using UnityEngine;


public abstract class EligibilityRuleSO : ScriptableObject
{
    public abstract bool IsEligible();
    public abstract string GetFailReason();
}
