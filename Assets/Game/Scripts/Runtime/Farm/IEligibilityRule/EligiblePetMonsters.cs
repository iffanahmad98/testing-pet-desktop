using UnityEngine;

[CreateAssetMenu(menuName = "Eligibility/Pet Monster")]
public class EligiblePetMonster : EligibilityRuleSO
{
    public int minPetMonster;

    public override bool IsEligible()
    {
        return  PlayerHistoryManager.instance.hotelRoomCompleted >= minPetMonster;
    }

    public override string GetFailReason()
    {
        return $"Butuh pet setidaknya {minPetMonster}";
    }
}
