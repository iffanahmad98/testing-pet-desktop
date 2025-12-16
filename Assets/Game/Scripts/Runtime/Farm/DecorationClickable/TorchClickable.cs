using UnityEngine;
using UnityEngine.UI;

public class TorchClickable : DecorationClickable
{
   public bool isActived = false;
   [SerializeField] Button [] torchButtons;
   [SerializeField] GameObject [] torchLights;
    
    void Start () {
        foreach (Button button in torchButtons) {
            button.onClick.AddListener (OnClick);
        }
    }
   public override void OnClick () {
     if (isActived) {
        isActived = false;
        OffEvent ();
     } else {
        isActived = true;
        OnEvent ();
     } 
   }

   public override void DecorationTurnedOff () {
    isActived = false;
    OffEvent ();
   }

   public void OnEvent () {
        foreach (GameObject torch in torchLights) {
            torch.SetActive (true);
        }
   }

   public void OffEvent () {
        foreach (GameObject torch in torchLights) {
            torch.SetActive (false);
        }
   }

}
