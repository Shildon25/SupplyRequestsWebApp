using Microsoft.AspNetCore.Http;
using System.Diagnostics;

namespace SupplyManagement.Models.ViewModels
{
    public class ErrorViewModel
    {
        public ErrorViewModel()
        {
        }

        public ErrorViewModel(string errorMessage)
		{
            ErrorMessage = errorMessage;
		}

		public ErrorViewModel(string requestId, string errorMessage)
        {
            RequestId = requestId;
            ErrorMessage = errorMessage;
        }
        public string? RequestId { get; set; }
        public string? ErrorMessage { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
