using UnityEngine;
using MagicalGarden.AI;
using MagicalGarden.Farm;
using System.Collections;
using System.Collections.Generic;
// Attach with Hotel Controller
public class HotelGiftSpawner : MonoBehaviour
{
   [SerializeField] GameObject hotelGift2dPrefab;
   int chanceGiftDrop = 400;
   GameObject cloneHotelGift;
   public void OnSpawnGift (List<PetMonsterHotel> listPets) { // HotelController (Call when check out)
   // Debug.Log ("Spawn Gift");
     foreach (PetMonsterHotel pet in listPets) { 
      int result = Random.Range (0,1000);
      if (result < chanceGiftDrop) {
        cloneHotelGift = GameObject.Instantiate (hotelGift2dPrefab);
        cloneHotelGift.transform.SetParent (pet.transform);
        cloneHotelGift.SetActive (true); 
        cloneHotelGift.transform.localPosition = new Vector3 (0, -0.5f, 1.5f);
        cloneHotelGift.transform.SetParent (null);

        HotelGiftHandler.instance.AddHotelGift (cloneHotelGift);
        Debug.Log ("Spawn Gift Reward");
      } else {
        Debug.Log ("Not Spawn Gift Reward");
      }
     }
   }

  /*
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
  */
   bool IsAnyGiftable () { // check is anything gift or not before claim.
    return true;
   } 

}
