using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class HiredEligibility {
    public int hired = 0;
    public List<EligibilityRuleSO> rules = new();
}

[CreateAssetMenu(fileName = "New Hotel Facilities", menuName = "Hotel/Hotel Facilities Data")]
public class HotelFacilitiesDataSO : ScriptableObject {
    public string id = "";
    public string facilityName = "";
    public int price = 0;
    public string detailText = "";

    [Header ("Facilities Motion Config")]
    public GameObject facilityPrefab;
    public Vector3 facilityLocalPosition;
    public Vector3 facilityLocalScale;

    [Header ("Facilities World")]
    public Vector3 facilitySpawnPosition;
    public Vector3 [] facilitySpawnPositions;

    [Header ("Hotel Facilities Menu")]
    public List<EligibilityRuleSO> rules = new();
    public List <HiredEligibility> rulesHiredEligibility = new List <HiredEligibility> ();

    public bool IsEligible()
    { // HotelFacilitiesMenu.cs
        foreach (var rule in rules)
        {
            if (!rule.IsEligible())
                return false;
        }
        return true;
    }

    public bool IsHiredEligible (int target)
    {
        foreach (var rule in rulesHiredEligibility[target].rules)
        {
            if (!rule.IsEligible())
                return false;
        }
        return true;
    }

    
}


