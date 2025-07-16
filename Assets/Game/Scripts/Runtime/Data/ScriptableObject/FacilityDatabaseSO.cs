using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "FacilityDatabase", menuName = "Facility/FacilityDatabase")]
public class FacilityDatabaseSO : ScriptableObject
{
    public List<FacilityDataSO> allFacilities;

    public FacilityDataSO GetFacility(string id)
    {
        return allFacilities.Find(facility => facility.facilityID == id);
    }
}
