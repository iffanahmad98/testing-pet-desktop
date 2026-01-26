using UnityEngine;

[CreateAssetMenu(menuName = "Eligibility/Harvest Egg")]
public class EligibleHarvestEgg : EligibilityRuleSO
{
    public int minHarvest;

    public override bool IsEligible()
    {
        return  SaveSystem.PlayerConfig.harvestEggMonsters >= minHarvest;
    }

    public override string GetFailReason()
    {
        return $"Butuh harvest fruit setidkanya {minHarvest} kali";
    }
}
