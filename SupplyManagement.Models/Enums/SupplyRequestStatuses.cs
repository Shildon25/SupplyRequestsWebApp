using System.ComponentModel.DataAnnotations;

namespace SupplyManagement.Models.Enums
{
    public enum SupplyRequestStatuses
    {
        Created,
        Approved,
        Rejected,
        [Display(Name = "Details Document Generated")]
        DelailsDocumentGenerated,
        [Display(Name = "Pending Delivery")]
        PendingDelivery,
        Delivered,
        [Display(Name = "Delivered With Claims")]
        DeliveredWithClaims,
        [Display(Name = "Claims Document Generated")]
        ClaimsDocumentGenerated,
        [Display(Name = "Claims Eliminated")]
        ClaimsEliminated,
        [Display(Name = "Money Returned")]
        MoneyRetured,
    }
}
