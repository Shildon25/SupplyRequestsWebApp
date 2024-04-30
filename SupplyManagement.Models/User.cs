﻿namespace SupplyManagement.Models
{
    using Microsoft.AspNetCore.Identity;
    using SupplyManagement.Models.Enums;
    using System.ComponentModel.DataAnnotations;

    public class User: IdentityUser
    {
        [Required]
        [StringLength(100)]
		[Display(Name = "First Name")]
		public string Name { get; set; }
        [Required]
        [StringLength(100)]
		[Display(Name = "Last Name")]
		public string Surname { get; set; }
		[Display(Name = "Account Status")]
		public AccountStatuses AccountStatus {  get; set; }
    }
}
