using UnityEngine;

public class BackToPlains : MonoBehaviour
{
   // All services in hotel and farm must be registered here (If there's a bug) 
   [SerializeField] MagicalGarden.AI.NPCHotel npcHotel;
    // Attach to back button
   public void StartConfig () {
    StopNPCHotel ();
   }

   void StopNPCHotel () {
    npcHotel.StopMovement ();
   }

}
