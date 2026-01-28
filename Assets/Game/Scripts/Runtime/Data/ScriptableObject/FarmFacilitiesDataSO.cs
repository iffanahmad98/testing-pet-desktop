using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Farm Facilities", menuName = "Farm/Farm Facilities Data")]
public class FarmFacilitiesDataSO : ScriptableObject {
    public string id = "";
    public string facilityName = "";
    public int price = 0;
    [TextArea (3,10)]
    public string detailText = "";
    public int maxHired = 0;
    [Header ("Facilities Motion Config (World)")]
    public GameObject facilityPrefab;
    
    [Header ("Facilities Motion Config (UI)")]
    public GameObject facilityUIPrefab;
    public Vector3 facilityCardUILocalPosition;
    public Vector3 facilityCardUILocalScale;

    public Vector3 facilityPodiumUILocalPosition;
    public Vector3 facilityPodiumUILocalScale;

    [Header ("Facilities World")]
    public Vector3 facilitySpawnPosition;
    public Vector3 [] facilitySpawnPositions;

    [Header ("Eligibility (Hotel Facilities Menu)")]
    public List<EligibilityRuleSO> rules = new();
    public List <HiredEligibility> rulesHiredEligibility = new List <HiredEligibility> ();

    #region Eligibility
    public bool IsEligible() // untuk yang tidak ada tingkat
    { // HotelFacilitiesMenu.cs
        foreach (var rule in rules)
        {
            if (!rule.IsEligible())
                return false;
        }
        return true;
    }

    public bool IsHiredEligible (int target) // untuk yang ada tingkat (HiredEligibility)
    {
        if (target >= maxHired) return false;
        
        foreach (var rule in rulesHiredEligibility[target].rules)
        {
            if (!rule.IsEligible())
                return false;
        }
        return true;
    }

    public int GetHiredPrice (int target) {
        foreach (var rule in rulesHiredEligibility[target].rules)
        {
            if (rule is EligibleCoin targetRule)
                return targetRule.minCoin;
        }
        return 0;
    }

    #endregion
    
}


