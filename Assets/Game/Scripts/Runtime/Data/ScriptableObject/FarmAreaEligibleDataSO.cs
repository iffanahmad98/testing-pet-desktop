using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Farm Eligible", menuName = "Hotel/Farm Area Data SO")]
public class FarmAreaEligibleDataSO : ScriptableObject
{
    [Header ("Eligibility (Hotel Facilities Menu)")]
    public List<EligibilityRuleSO> rules = new();

    #region Eligibility
        
    public bool IsEligible() // untuk yang tidak ada tingkat
    { // UnlockBubbleUI.cs
        foreach (var rule in rules)
        {
            if (!rule.IsEligible())
                return false;
        }
        return true;
    }

    public bool IsEligibleEnoughPetMonsterOnly ()
    {
        foreach (var rule in rules)
        {
            if (rule is EligiblePetMonster targetRule)
            {
                if (targetRule.IsEligible ())
                return true;
            }
        }
        return false;
    }


    public int GetPrice ()
    { // UnlockBubbleUI.cs
        foreach (var rule in rules)
        {
            if (rule is EligibleCoin coinRule)
            {
                return coinRule.minCoin;
            }
        }

        return 0;
    }

    public int GetHarvestFruit ()
    {
        foreach (var rule in rules)
        {
            if (rule is EligibleHarvestFruit targetRule)
            {
                return targetRule.minHarvest;
            }
        }

        return 0;
    }

    public int GetHarvestEgg ()
    {
        foreach (var rule in rules)
        {
            if (rule is EligibleHarvestEgg targetRule)
            {
                return targetRule.minHarvest;
            }
        }

        return 0;
    }
    #endregion
}
