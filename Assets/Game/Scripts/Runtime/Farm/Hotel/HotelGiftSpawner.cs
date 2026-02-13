using UnityEngine;
using MagicalGarden.AI;
using MagicalGarden.Farm;
using MagicalGarden.Hotel;
using System.Collections;
using System.Collections.Generic;
// Attach with Hotel Controller
public class HotelGiftSpawner : MonoBehaviour
{
  [SerializeField] GameObject hotelGift2dPrefab;
  int chanceGiftDrop = 400;
  GameObject cloneHotelGift;

  static bool tutorialForceGiftActive = false;
  static float tutorialForceGiftEndTime = 0f;

  public static void EnableTutorialForceGift(float duration)
  {
    if (duration <= 0f)
    {
      tutorialForceGiftActive = false;
      return;
    }

    tutorialForceGiftActive = true;
    tutorialForceGiftEndTime = Time.time + duration;
    Debug.Log($"[HotelGiftSpawner] Tutorial force-gift enabled for {duration:F1}s.");
  }
  public void OnSpawnGift(List<PetMonsterHotel> listPets)
  { // HotelController (Call when check out)
    bool boostActive = tutorialForceGiftActive && Time.time <= tutorialForceGiftEndTime;
    if (tutorialForceGiftActive && !boostActive)
    {
      tutorialForceGiftActive = false;
    }

    bool atLeastOneSpawned = false;
    bool tutorialBubbleSpawned = false;

    foreach (PetMonsterHotel pet in listPets)
    {
      int result = Random.Range(0, 1000);
      bool shouldSpawn = result < chanceGiftDrop;

      if (!shouldSpawn && boostActive && !atLeastOneSpawned)
      {
        shouldSpawn = true;
        Debug.Log("[HotelGiftSpawner] Tutorial boost: forcing gift spawn on clean.");
      }

      if (shouldSpawn)
      {
        cloneHotelGift = GameObject.Instantiate(hotelGift2dPrefab);
        cloneHotelGift.transform.SetParent(pet.transform);
        cloneHotelGift.SetActive(true);
        cloneHotelGift.transform.localPosition = new Vector3(0, -0.5f, -3.0f);
        cloneHotelGift.transform.SetParent(null);

        if (HotelGiftHandler.instance != null)
        {
          HotelGiftHandler.instance.AddHotelGift(cloneHotelGift);
        }

        atLeastOneSpawned = true;
        Debug.Log("Spawn Gift Reward");

        // Saat booster tutorial aktif, selain menjamin gift drop,
        // kita juga munculkan bubble gift di kamar terkait (sekali saja per OnSpawnGift).
        if (boostActive && !tutorialBubbleSpawned && pet != null && pet.hotelContrRef != null)
        {
          tutorialBubbleSpawned = true;
          pet.hotelContrRef.SpawnGiftBubbleForTutorial();
          Debug.Log("[HotelGiftSpawner] Tutorial boost: spawn gift bubble for tutorial.");
        }
      }
      else
      {
        Debug.Log("Not Spawn Gift Reward");
      }
    }
  }



  public GameObject ForceSpawnGiftForTutorial()
  {
    if (hotelGift2dPrefab == null)
    {
      Debug.LogWarning("[HotelGiftSpawner] ForceSpawnGiftForTutorial: hotelGift2dPrefab belum di-assign.");
      return null;
    }

    GameObject gift = GameObject.Instantiate(hotelGift2dPrefab);
    gift.SetActive(true);
    gift.transform.position = transform.position;

    if (HotelGiftHandler.instance != null)
    {
      HotelGiftHandler.instance.AddHotelGift(gift);
    }

    Debug.Log("[HotelGiftSpawner] Spawn Tutorial Gift Reward");
    return gift;
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
  bool IsAnyGiftable()
  { // check is anything gift or not before claim.
    return true;
  }

}
