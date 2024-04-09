using SupplyManagement.WebApp.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace SupplyManagement.WebApp.Models.ViewModels
{
	public class ApproveAccountViewModel
	{
        public string Id { get; set; }
		[Display(Name = "First Name")]
		public string Name { get; set; }
		[Display(Name = "Last Name")]
		public string Surname { get; set; }

        public string Email { get; set; }

        [Display(Name = "Roles")]
        public string Roles { get; set; }

        [Required(ErrorMessage = "Account Status is required.")]
		[Display(Name = "Account Status")]
		public AccountStatuses AccountStatus { get; set; }
    }
}
