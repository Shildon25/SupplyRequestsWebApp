namespace SupplyManagement.WebApp.Models
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using SupplyManagement.WebApp.Models.Enums;

    public class SupplyRequest
    {
        public int Id { get; set; }
        public List<Item> Items { get; set; }
        
        public SupplyRequestStatuses Status { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? ClosedAt { get; set; }
        public DateTimeOffset LastUpdatedAt { get; set; }
        [Required]
        [Display(Name = "Created By")]
        public string CreatedByUserId { get; set; }
        [ForeignKey("CreatedByUserId")]
        public User CreatedBy { get; set; }
        [Required]
        [Display(Name = "Approved By")]
        public string? ApprovedByUserId { get; set; }
        [ForeignKey("ApprovedByUserId")]
        public User? ApprovedBy { get; set; }
        [Required]
        [Display(Name = "Delivered By")]
        public string? DeliveredByUserId { get; set; }
        [ForeignKey("DeliveredByUserId")]
        public User? DeliveredBy { get; set; }
        public string? ClaimsText { get; set; }
    }
}
