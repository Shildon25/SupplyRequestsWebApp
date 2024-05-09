using DocumentFormat.OpenXml.Packaging;

namespace SupplyManagement.DocumentGeneratorService.Tests
{
    [TestClass]
    public class DocumentGeneratorTests
    {
        private const string SupplyDocumentTemplateFilePath = "Templates//Supply Document.docx";
        private const string ClaimsDocumentTemplateFilePath = "Templates//Claims Document.docx";
        private const string OutputFilePath = "Output.docx";

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

            // Act
            DocumentGenerator.GenerateSupplyDocument(supplyDocument, OutputFilePath, SupplyDocumentTemplateFilePath);

            // Assert
            using (var outputDoc = WordprocessingDocument.Open(OutputFilePath, true))
            {
                var body = outputDoc.MainDocumentPart.Document.Body;

                Assert.IsNotNull(body);
                Assert.IsTrue(body.InnerText.Contains(supplyDocument.RequestId.ToString()));
                Assert.IsTrue(body.InnerText.Contains(supplyDocument.RequestOwnerName));
                Assert.IsTrue(body.InnerText.Contains(supplyDocument.ApprovalManagerName));
                Assert.IsTrue(body.InnerText.Contains(supplyDocument.RequestItems[0]));
                Assert.IsTrue(body.InnerText.Contains(supplyDocument.RequestItems[1]));
            }

            // Clean up
            File.Delete(OutputFilePath);
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

            // Act
            DocumentGenerator.GenerateClaimsDocument(claimsDocument, OutputFilePath, ClaimsDocumentTemplateFilePath);

            // Assert
            using (var outputDoc = WordprocessingDocument.Open(OutputFilePath, true))
            {
                var body = outputDoc.MainDocumentPart.Document.Body;

                Assert.IsNotNull(body);
                Assert.IsTrue(body.InnerText.Contains(claimsDocument.RequestId.ToString()));
                Assert.IsTrue(body.InnerText.Contains(claimsDocument.RequestOwnerName));
                Assert.IsTrue(body.InnerText.Contains(claimsDocument.ApprovalManagerName));
                Assert.IsTrue(body.InnerText.Contains(claimsDocument.CourierName));
                Assert.IsTrue(body.InnerText.Contains(claimsDocument.ClaimsText));
                Assert.IsTrue(body.InnerText.Contains(claimsDocument.RequestItems[0]));
                Assert.IsTrue(body.InnerText.Contains(claimsDocument.RequestItems[1]));
            }

            // Clean up
            File.Delete(OutputFilePath);
        }
    }
}
