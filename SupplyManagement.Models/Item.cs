namespace SupplyManagement.Models
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class Item
    {
        public int Id { get; set; }
        public string Name { get; set; }
        [ForeignKey("VendorId")]
        public Vendor Vendor { get; set; }
        [Required]
        [Display(Name = "Vendor")]
        public int VendorId { get; set; }
        [Required]
        [Display(Name = "Created By")]
        public int CreatedByUserId { get; set; }
        [ForeignKey("UserId")]
        public User CreatedBy { get; set; }

    }
}
