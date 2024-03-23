namespace SupplyManagement.Models
{
    using SupplyManagement.Models.Enums;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class AccountRequest
    {
        public int Id { get; set; }
        [Required]
        [Display(Name = "Account to approve")]
        public int RequestedAccountId { get; set; }
        [ForeignKey("UserId")]
        public User RequestedAccount { get; set; }
        public AccountStatuses Status { get; set; }
        [Display(Name = "Created At")]
        public DateTimeOffset CreatedAt { get; set; }
        [Display(Name = "Closed At")]
        public DateTimeOffset? ClosedAt { get; set; }
        [Display(Name = "Approved By")]
        public int? ApprovedByUserId { get; set; }
        [ForeignKey("UserId")]
        public User? ApprovedBy { get; set; }
    }
}
