namespace SupplyManagement.Models
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using SupplyManagement.Models.Enums;

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
        public int CreatedByUserId { get; set; }
        [ForeignKey("UserId")]
        public User CreatedBy { get; set; }
        [Required]
        [Display(Name = "Approved By")]
        public int? ApprovedByUserId { get; set; }
        [ForeignKey("UserId")]
        public User? ApprovedBy { get; set; }
        [Required]
        [Display(Name = "Delivered By")]
        public int? DeliveredByUserId { get; set; }
        [ForeignKey("UserId")]
        public User? DeliveredBy { get; set; }
        public string? ClaimsText { get; set; }
    }
}
