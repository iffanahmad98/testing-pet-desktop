using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "HotelFacilitiesDatabase", menuName = "BumiMobile/Hotel Facilities Database")]
public class HotelFacilitiesDatabaseSO : ScriptableObject
{
    public List<HotelFacilitiesDataSO> allDatas = new List<HotelFacilitiesDataSO>();
    public HotelFacilitiesDataSO GetHotelFacilitiesDataSO(string idValue)
    {
        return allDatas.Find(data => data.id == idValue);
    }

    public HotelFacilitiesDataSO GetHotelFacilitiesDataSO (int element) {
        return allDatas[element];
    }
}
