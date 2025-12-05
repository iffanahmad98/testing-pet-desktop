using UnityEngine;

// Attach with Hotel Controller
public class HotelGiftSpawner : MonoBehaviour
{
   [SerializeField] GameObject hotelGiftBubblePrefab;
   [SerializeField] GameObject hotelGiftReceivedPrefab;
   int totalGift = 1;
   GameObject cloneHotelGiftBubble;
   public void OnSpawnGift () { // HotelController (Call when check out)
   // Debug.Log ("Spawn Gift");
     cloneHotelGiftBubble = GameObject.Instantiate (hotelGiftBubblePrefab);
     cloneHotelGiftBubble.transform.SetParent (this.gameObject.transform);
     cloneHotelGiftBubble.SetActive (true); 
     cloneHotelGiftBubble.transform.localPosition = new Vector3 (0, 4.1f, 0);
   }

   public void ClaimGift () { // CickableObjectHotel (when click hotel object)
    // Debug.Log ("ClaimGift");
     GameObject cloneReceived = GameObject.Instantiate (hotelGiftReceivedPrefab);
     cloneReceived.transform.SetParent (this.gameObject.transform);
     cloneReceived.SetActive (true); 
     cloneReceived.transform.localPosition = new Vector3 (0, 4.1f, 0);

     Destroy (cloneHotelGiftBubble);
     cloneHotelGiftBubble = null;

     HotelGift.instance.GetLoot (1);
     HotelGiftHandler.instance.SpawnHotelGiftDisplay ();
   }

   public bool IsAnyGiftable () { // check is anything gift or not before claim.
    return cloneHotelGiftBubble;
   } 

}
