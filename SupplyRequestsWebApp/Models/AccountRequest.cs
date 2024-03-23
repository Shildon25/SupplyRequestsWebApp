namespace SupplyManagement.WebApp.Models
{
    using SupplyManagement.WebApp.Models.Enums;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class AccountRequest
    {
        public int Id { get; set; }
        [Required]
        [Display(Name = "Account to approve")]
        public string RequestedAccountId { get; set; }
        [ForeignKey("RequestedAccountId")]
        public User RequestedAccount { get; set; }
        public AccountStatuses Status { get; set; }
        [Display(Name = "Created At")]
        public DateTimeOffset CreatedAt { get; set; }
        [Display(Name = "Closed At")]
        public DateTimeOffset? ClosedAt { get; set; }
        [Display(Name = "Approved By")]
        public string? ApprovedByUserId { get; set; }
        [ForeignKey("ApprovedByUserId")]
        public User? ApprovedBy { get; set; }
    }
}
