using UnityEngine;

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
    
}
