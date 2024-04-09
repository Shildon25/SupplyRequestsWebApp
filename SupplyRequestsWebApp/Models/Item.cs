namespace SupplyManagement.WebApp.Models
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
        public string CreatedByUserId { get; set; }
        [ForeignKey("CreatedByUserId")]
        public User CreatedBy { get; set; }

        public List<SupplyRequest> SupplyRequests { get; set; } = [];
	}
}
