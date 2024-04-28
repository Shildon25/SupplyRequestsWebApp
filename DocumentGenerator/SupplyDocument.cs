using SupplyManagement.WebApp.Models;

namespace SupplyManagement.DocumentGeneratorApp
{
	public class SupplyDocument
	{
		public SupplyDocument(int requestId, string requestOwnerName, string approvalManagerName, List<string> itemsList)
		{
			RequestId = requestId;
			RequestOwnerName = requestOwnerName;
			ApprovalManagerName = approvalManagerName;
			RequestItems = itemsList;
		}

		public int RequestId { get; set; }
        public string RequestOwnerName { get; set; }
        public string ApprovalManagerName{ get; set; }
        public List<string> RequestItems { get; set; }
    }
}
