using UnityEngine;

[CreateAssetMenu(menuName = "Eligibility/Material")]
public class EligibleMaterial : EligibilityRuleSO
{
    public EligibleItem eligibleItem; 
    public int minMaterials;

    public override bool IsEligible()
    {
        if (eligibleItem is MagicalGarden.Inventory.ItemData) {
            PlayerConfig playerConfig = SaveSystem.PlayerConfig;
            return playerConfig.GetItemAmount (eligibleItem.ItemId) >= minMaterials;
        }
        else {
            Debug.LogError ("Item belum terdaftar di EligibleItem");
            return false;
        }
        return  SaveSystem.PlayerConfig.coins >= minMaterials;
    }

    public override string GetFailReason()
    {
        return $"Butuh material {eligibleItem.ItemId} setidaknya {minMaterials}";
    }
}
