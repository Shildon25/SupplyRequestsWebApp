using SupplyManagement.WebApp.Models;

namespace SupplyManagement.DocumentGeneratorApp
{
	public class ClaimsDocument: SupplyDocument
	{
		public ClaimsDocument(int requestId, string requestOwnerName, string approvalManagerName, string courierName, string claimsText, List<string> itemsList)
		: base(requestId, requestOwnerName, approvalManagerName, itemsList)
		{
			CourierName = courierName;
			ClaimsText = claimsText;
		}

        public string CourierName { get; set; }
        public string ClaimsText{ get; set; }
    }
}
