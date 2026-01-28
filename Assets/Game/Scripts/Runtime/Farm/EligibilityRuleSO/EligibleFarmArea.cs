using UnityEngine;

[CreateAssetMenu(menuName = "Eligibility/Farm Area")]
public class EligibleFarmArea : EligibilityRuleSO
{
    public int minFarmArea;

    public override bool IsEligible()
    {
        return  MagicalGarden.Farm.PlantManager.Instance.GetTotalFarmArea () >= minFarmArea;
    }

    public override string GetFailReason()
    {
        return $"Butuh Farm area setidkanya {minFarmArea}";
    }
}
