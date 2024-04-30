namespace SupplyManagement.Models.ViewModels
{
	using System.ComponentModel.DataAnnotations;

	public class ChangePasswordViewModel
	{
		[Required(ErrorMessage = "Please enter your old password")]
		[DataType(DataType.Password)]
		[Display(Name = "Old Password")]
		public string OldPassword { get; set; }

		[Required(ErrorMessage = "Please enter your new password")]
		[DataType(DataType.Password)]
		[Display(Name = "New Password")]
		public string NewPassword { get; set; }

		[Required(ErrorMessage = "Please confirm your new password")]
		[DataType(DataType.Password)]
		[Display(Name = "Confirm Password")]
		[Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
		public string ConfirmPassword { get; set; }
	}
}
