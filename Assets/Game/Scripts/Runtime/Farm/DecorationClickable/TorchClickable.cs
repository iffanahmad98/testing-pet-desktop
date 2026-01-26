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
        string activeBiome = SaveSystem.GetActiveBiome();
        if (activeBiome != "night_biome") return;

        MonsterManager.instance.audio.PlaySFX("torch_ignite");
        foreach (GameObject torch in torchLights) {
            torch.SetActive (true);
        }

        // BGM malam dengan torch menyala harus ada di elemen 0
        MonsterManager.instance.audio.PlaySituationalBGM(0);
   }

   public void OffEvent () {
        foreach (GameObject torch in torchLights) {
            torch.SetActive (false);
        }

        string activeBiome = SaveSystem.GetActiveBiome();
        if (activeBiome == "night_biome")
        {
            MonsterManager.instance.audio.PlayMainMenuBGM();
        }
    }

}
