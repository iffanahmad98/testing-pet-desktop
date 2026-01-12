using UnityEngine;


[CreateAssetMenu(menuName = "Eligibility/Golden Ticket Exchange")]
public class GoldenTicketsExchangeEntry : EligibilityRuleSO
{
    public Rewardable rewardable;
    public int ticketCost;

    public override bool IsEligible()
    {
        return  GoldenTicket.instance.TotalValue >= ticketCost;
    }

    public override string GetFailReason()
    {
        return $"Butuh golden ticket setidaknya {ticketCost} kali";
    }
}
