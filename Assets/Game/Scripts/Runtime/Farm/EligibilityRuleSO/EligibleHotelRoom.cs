using UnityEngine;

[CreateAssetMenu(menuName = "Eligibility/Hotel Room")]
public class EligibleHotelRoom : EligibilityRuleSO
{
    public int minHotel;

    public override bool IsEligible()
    {
        return  PlayerHistoryManager.instance.hotelRoomCompleted >= minHotel;
    }

    public override string GetFailReason()
    {
        return $"Butuh Hotel complete setidkanya {minHotel} kali";
    }
}
