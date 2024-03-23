namespace SupplyManagement.Models
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class Vendor
    {
        public int Id { get; set; }
        public string Name { get; set; }
        [Required]
        [Display(Name = "Created By")]
        public int CreatedByUserId { get; set; }
        [ForeignKey("UserId")]
        public User CreatedBy { get; set; }
    }
}
