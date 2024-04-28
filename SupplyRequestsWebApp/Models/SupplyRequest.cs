namespace SupplyManagement.WebApp.Models
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Diagnostics.CodeAnalysis;
    using SupplyManagement.WebApp.Models.Enums;

    public class SupplyRequest
    {
        [Display(Name = "Request Id")]
        public int Id { get; set; }
        public List<ItemSupplyRequest> RequestItems { get; set; } = [];
        public SupplyRequestStatuses Status { get; set; }
        [Display(Name = "Created At")]
        public DateTimeOffset CreatedAt { get; set; }
        [Display(Name = "Closed At")]
        public DateTimeOffset? ClosedAt { get; set; }
        [Required]
        [Display(Name = "Created By")]
        public string CreatedByUserId { get; set; }
        [ForeignKey("CreatedByUserId")]
        public User CreatedBy { get; set; }
        [Display(Name = "Approved By")]
        [AllowNull]
        public string? ApprovedByUserId { get; set; }
        [ForeignKey("ApprovedByUserId")]
        public User? ApprovedBy { get; set; }
        [Display(Name = "Delivered By")]
        public string? DeliveredByUserId { get; set; }
        [ForeignKey("DeliveredByUserId")]
        public User? DeliveredBy { get; set; }
		[Display(Name = "Claims Text")]
		public string? ClaimsText { get; set; }
    }
}
