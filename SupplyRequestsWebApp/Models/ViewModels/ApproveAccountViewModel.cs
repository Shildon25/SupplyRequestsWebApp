using SupplyManagement.WebApp.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace SupplyManagement.WebApp.Models.ViewModels
{
	public class ApproveAccountViewModel
	{
		public string Id { get; set; }

		[Display(Name = "First Name")]
		[Required(ErrorMessage = "First Name is required.")]
		[StringLength(50, ErrorMessage = "First Name must be between {2} and {1} characters long.", MinimumLength = 2)]
		public string Name { get; set; }

		[Display(Name = "Last Name")]
		[Required(ErrorMessage = "Last Name is required.")]
		[StringLength(50, ErrorMessage = "Last Name must be between {2} and {1} characters long.", MinimumLength = 2)]
		public string Surname { get; set; }

		[Required(ErrorMessage = "Email is required.")]
		[EmailAddress(ErrorMessage = "Invalid Email Address.")]
		public string Email { get; set; }

		[Display(Name = "Roles")]
		public string Roles { get; set; }

		[Required(ErrorMessage = "Account Status is required.")]
		[Display(Name = "Account Status")]
		public AccountStatuses AccountStatus { get; set; }
	}
}
