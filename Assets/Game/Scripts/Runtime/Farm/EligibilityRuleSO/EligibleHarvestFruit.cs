using UnityEngine;

[CreateAssetMenu(menuName = "Eligibility/Harvest Fruit")]
public class EligibleHarvestFruit : EligibilityRuleSO
{
    public int minHarvest;

    public override bool IsEligible()
    {
        return  SaveSystem.PlayerConfig.harvestFruit >= minHarvest;
    }

    public override string GetFailReason()
    {
        return $"Butuh harvest fruit setidkanya {minHarvest} kali";
    }
}
