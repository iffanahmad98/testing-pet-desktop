using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "HotelClickableDatabase", menuName = "BumiMobile/Hotel Clickable Database")]
public class HotelClickableDatabaseSO : ScriptableObject
{
    public List<HotelClickableDataSO> allDatas = new List<HotelClickableDataSO>();
    public HotelClickableDataSO GetHotelClickableDataSO(string objectName)
    {
        return allDatas.Find(data => data.objectName == objectName);
    }
}
