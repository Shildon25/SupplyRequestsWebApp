namespace SupplyManagement.Models
{
    using Microsoft.AspNet.Identity.EntityFramework;
    using SupplyManagement.Models.Enums;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class User: IdentityUser
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }
        [Required]
        [StringLength(100)]
        public string Surname { get; set; }
        [Required]
        public AccountRoles Role { get; set; }
        [Required]
        [Display(Name = "Approved By")]
        public int? ApprovedByUserId { get; set; }
        [ForeignKey("UserId")]
        public User? ApprovedBy { get; set; }
        public AccountStatuses AccountStatus {  get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}
