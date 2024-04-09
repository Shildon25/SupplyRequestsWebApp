using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace SupplyManagement.WebApp.Models.ViewModels
{
	public class SupplyRequestViewModel
	{
		public int Id { get; set; }
		public List<SelectListItem> SelectedItems { get; set; }
		[Display(Name="Items")]
		public int[] ItemsIds { get; set; }
		public string? ClaimsText { get; set; }
	}
}
