using UnityEngine;
using System.Collections.Generic;
using System.Collections;
[CreateAssetMenu(fileName = "FarmFacilitiesDatabaseSO", menuName = "BumiMobile/Farm Facilities Database")]
public class FarmFacilitiesDatabaseSO : ScriptableObject
{
    public List<FarmFacilitiesDataSO> allDatas = new List<FarmFacilitiesDataSO>();
    public FarmFacilitiesDataSO GetFarmFacilitiesDataSO(string idValue)
    {
        return allDatas.Find(data => data.id == idValue);
    }

    public FarmFacilitiesDataSO GetFarmFacilitiesDataSO (int element) {
        return allDatas[element];
    }

    public List<FarmFacilitiesDataSO> GetListFarmFacilitiesDataSO () {
        return allDatas;
    }
}
