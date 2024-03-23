namespace SupplyManagement.WebApp.Models.Enums
{
    public enum SupplyRequestStatuses
    {
        Created,
        ApprovedByManager,
        RejectedByManager,
        DelailsDocumentGenerated,
        PendingDelivery,
        Delivered,
        DeliveredWithClaims,
        ClaimsDocumentGenerated,
        ClaimsEliminated,
        MoneyRetured,
    }
}
