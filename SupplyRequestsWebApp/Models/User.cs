namespace SupplyManagement.WebApp.Models
{
    using Microsoft.AspNetCore.Identity;
    using SupplyManagement.WebApp.Models.Enums;
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
        public AccountStatuses AccountStatus {  get; set; }
    }
}
