using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Linq;

namespace SupplyManagement.DocumentGeneratorService.Tests
{
    [TestClass]
    public class DocumentGeneratorTests
    {
        private const string SupplyDocumentTemplateFilePath = "Templates//Supply Document.docx";
        private const string ClaimsDocumentTemplateFilePath = "Templates//Claims Document.docx";

        [TestMethod]
        public void GenerateSupplyDocument_ReplacesPlaceholders()
        {
            // Arrange
            var supplyDocument = new SupplyDocument
            (
                123,
                "John Doe",
                "Jane Smith",
                new List<string> { "Item 1", "Item 2" }
            );

			// Convert the template document to a memory stream
			MemoryStream templateStream = new MemoryStream();
			using (FileStream fileStream = File.OpenRead(SupplyDocumentTemplateFilePath))
			{
				fileStream.CopyTo(templateStream);
			}

			// Act
			DocumentGenerator.GenerateSupplyDocument(supplyDocument, templateStream);

			// Reset the memory stream position to read from the beginning
			templateStream.Position = 0;

            // Assert
            using (WordprocessingDocument outputDoc = WordprocessingDocument.Open(templateStream, false))
            {
				Body outputBody = outputDoc.MainDocumentPart.Document.Body;

				Assert.IsTrue(outputBody.InnerText.Contains(supplyDocument.RequestId.ToString()));
                Assert.IsTrue(outputBody.InnerText.Contains(supplyDocument.RequestOwnerName));
                Assert.IsTrue(outputBody.InnerText.Contains(supplyDocument.ApprovalManagerName));
                Assert.IsTrue(outputBody.InnerText.Contains(supplyDocument.RequestItems[0]));
                Assert.IsTrue(outputBody.InnerText.Contains(supplyDocument.RequestItems[1]));
            }
		}

        [TestMethod]
        public void GenerateClaimsDocument_ReplacesPlaceholders()
        {
            // Arrange
            var claimsDocument = new ClaimsDocument
            (
                123,
                "John Doe",
                "Jane Smith",
                "Charlie",
                 "Defective items received.",
                new List<string> { "Item 1", "Item 2" }
            );

			// Convert the template document to a memory stream
			MemoryStream templateStream = new MemoryStream();
			using (FileStream fileStream = File.OpenRead(ClaimsDocumentTemplateFilePath))
			{
				fileStream.CopyTo(templateStream);
			}

			// Act
			DocumentGenerator.GenerateClaimsDocument(claimsDocument, templateStream);

			// Reset the memory stream position to read from the beginning
			templateStream.Position = 0;

            // Assert
            using (WordprocessingDocument outputDoc = WordprocessingDocument.Open(templateStream, false))
            {
                Body outputBody = outputDoc.MainDocumentPart.Document.Body;

                Assert.IsTrue(outputBody.InnerText.Contains(claimsDocument.RequestId.ToString()));
                Assert.IsTrue(outputBody.InnerText.Contains(claimsDocument.RequestOwnerName));
                Assert.IsTrue(outputBody.InnerText.Contains(claimsDocument.ApprovalManagerName));
                Assert.IsTrue(outputBody.InnerText.Contains(claimsDocument.CourierName));
                Assert.IsTrue(outputBody.InnerText.Contains(claimsDocument.ClaimsText));
                Assert.IsTrue(outputBody.InnerText.Contains(claimsDocument.RequestItems[0]));
                Assert.IsTrue(outputBody.InnerText.Contains(claimsDocument.RequestItems[1]));
            }
        }
    }
}
