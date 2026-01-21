using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
[Serializable]
public class EligibleMaterial {
    public Rewardable rewardable;
    public int minMaterial;
}

[CreateAssetMenu(menuName = "Eligibility/Materials")]
public class EligibleMaterials : EligibilityRuleSO
{
    public List <EligibleMaterial> listEligibleMaterial = new List <EligibleMaterial> (); 

    public override bool IsEligible()
    {
        PlayerConfig playerConfig = SaveSystem.PlayerConfig;
        foreach (EligibleMaterial eligible in listEligibleMaterial) {
            if (eligible.rewardable is MagicalGarden.Inventory.ItemData) {
                if ( playerConfig.GetItemFarmAmount (eligible.rewardable.ItemId) < eligible.minMaterial) {
                    return false;
                }
            }
           
        }
        return true;
    }

    public override string GetFailReason()
    {
        return $"Material tidak cukup";
    }

    #region EligibleMaterials
    public void UsingAllItems () { // FertilizerManager
        PlayerConfig playerConfig = SaveSystem.PlayerConfig;
        foreach (EligibleMaterial eligible in listEligibleMaterial) {
            if (eligible.rewardable is MagicalGarden.Inventory.ItemData) {
                if ( playerConfig.GetItemFarmAmount (eligible.rewardable.ItemId) >= eligible.minMaterial) {
                    playerConfig.RemoveItemFarm (eligible.rewardable.ItemId, eligible.minMaterial, true);
                }
            }
           
        }
        SaveSystem.SaveAll ();
    }
    #endregion
}


