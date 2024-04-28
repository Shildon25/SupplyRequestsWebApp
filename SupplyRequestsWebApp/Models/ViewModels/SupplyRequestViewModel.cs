using Microsoft.AspNetCore.Mvc.Rendering;
using SupplyManagement.WebApp.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace SupplyManagement.WebApp.Models.ViewModels
{
	public class SupplyRequestViewModel
	{
		public int Id { get; set; }
		[Display(Name = "Selected Items")]
		public List<SelectListItem> SelectedItems { get; set; }
		[Display(Name="Items")]
		public int[] ItemsIds { get; set; }
		[Display(Name = "Claims Text")]
		public string? ClaimsText { get; set; }
		public SupplyRequestStatuses Status { get; set; }
	}
}
