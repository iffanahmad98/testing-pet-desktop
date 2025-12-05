using UnityEngine;
using System.Collections;
using System.Collections.Generic;
[CreateAssetMenu(fileName = "HotelFacilitiesDatabase", menuName = "BumiMobile/Npc Area Point Database SO")]
public class NPCAreaPointsDatabaseSO : ScriptableObject {
    [SerializeField] List <NPCAreaPointsSO> listNpcAreaPoints = new List <NPCAreaPointsSO> ();

    public NPCAreaPointsSO GetRandomNPCAreaPointsSO () {
        return listNpcAreaPoints[Random.Range (0, listNpcAreaPoints.Count)];
    }
}
