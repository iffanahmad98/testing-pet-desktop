using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Hotel Eligible", menuName = "Hotel/Hotel Eligible Data")]
public class HotelControllerEligibleDataSO : ScriptableObject
{
    [Header ("Eligibility (Hotel Facilities Menu)")]
    public List<EligibilityRuleSO> rules = new();
    public RequirementTipDataSO requirementDataSO; // HotelController.cs
    #region Eligibility
        
    public bool IsEligible() // untuk yang tidak ada tingkat
    { // HotelController.cs
        foreach (var rule in rules)
        {
            if (!rule.IsEligible())
                return false;
        }
        return true;
    }

    public bool IsEligibleWithoutCoin ()
    { // // HotelController.cs
        foreach (var rule in rules)
        {
            if (rule is not EligibleCoin)
            {
                if (!rule.IsEligible())
                    return false;
            }
        }
        return true;
    }

    public int GetPrice ()
    { // HotelController.cs
        foreach (var rule in rules)
        {
            if (rule is EligibleCoin coinRule)
            {
                return coinRule.minCoin;
            }
        }

        return 0;
    }
    #endregion
}
