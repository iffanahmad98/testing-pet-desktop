using UnityEngine;

public  class DecorationClickableTrigger : MonoBehaviour
{
   [SerializeField] DecorationClickable decorationClickable;

   void OffDisalbe () {
      if (decorationClickable) {
         decorationClickable.DecorationTurnedOff ();
      }
   }
}
