using UnityEngine;

[CreateAssetMenu(menuName = "Eligibility/Coin")]
public class EligibleCoin : EligibilityRuleSO
{
    public int minCoin;

    public override bool IsEligible()
    {
        return  SaveSystem.PlayerConfig.coins >= minCoin;
    }

    public override string GetFailReason()
    {
        return $"Butuh coin setidkanya {minCoin} kali";
    }
}
