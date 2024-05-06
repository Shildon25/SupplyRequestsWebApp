using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace SupplyManagement.DocumentGeneratorService
{
    public static class DocumentGenerator
	{
        public static void GenerateSupplyDocument(SupplyDocument supplyDocument, string filePath, string templateFilePath)
        {
            // Load the template document
            using (WordprocessingDocument templateDoc = WordprocessingDocument.Open(templateFilePath, false))
            {
                // Create a new document based on the template
                using (WordprocessingDocument doc = (WordprocessingDocument)templateDoc.Clone(filePath, true))
                {
                    // Access the main part of the document
                    MainDocumentPart mainPart = doc.MainDocumentPart;

                    // Find and replace placeholders in the document with actual values
                    ReplacePlaceholderWithText(mainPart, "[REQUEST_ID]", supplyDocument.RequestId.ToString());
                    ReplacePlaceholderWithText(mainPart, "[REQUEST_OWNER]", supplyDocument.RequestOwnerName);
                    ReplacePlaceholderWithText(mainPart, "[APPROVAL_MANAGER]", supplyDocument.ApprovalManagerName);

                    // Replace items list placeholder
                    string itemsList = string.Join("\n", supplyDocument.RequestItems);
                    ReplacePlaceholderWithText(mainPart, "[ITEMS_LIST]", itemsList);

                    // Save the modified document to the output file
                    doc.Save();
                }
            }
        }

        public static void GenerateClaimsDocument(ClaimsDocument claimsDocument, string filePath, string templateFilePath)
        {
            // Load the template document
            using (WordprocessingDocument templateDoc = WordprocessingDocument.Open(templateFilePath, false))
            {
                // Create a new document based on the template
                using (WordprocessingDocument doc = (WordprocessingDocument)templateDoc.Clone(filePath))
                {
                    // Access the main part of the document
                    MainDocumentPart mainPart = doc.MainDocumentPart;

                    // Find and replace placeholders in the document with actual values
                    ReplacePlaceholderWithText(mainPart, "[REQUEST_ID]", claimsDocument.RequestId.ToString());
                    ReplacePlaceholderWithText(mainPart, "[REQUEST_OWNER]", claimsDocument.RequestOwnerName);
                    ReplacePlaceholderWithText(mainPart, "[APPROVAL_MANAGER]", claimsDocument.ApprovalManagerName);
                    ReplacePlaceholderWithText(mainPart, "[REQUEST_COURIER]", claimsDocument.CourierName);
                    ReplacePlaceholderWithText(mainPart, "[CLAIMS_TEXT]", claimsDocument.ClaimsText);

                    // Replace items list placeholder
                    string itemsList = string.Join("\n", claimsDocument.RequestItems);
                    ReplacePlaceholderWithText(mainPart, "[ITEMS_LIST]", itemsList);

                    // Save the modified document to the output file
                    doc.Save();
                }
            }
        }

        // Helper method to replace placeholders in the document with text
        private static void ReplacePlaceholderWithText(MainDocumentPart mainPart, string placeholder, string newText)
        {
            // Iterate through all paragraphs in the document
            foreach (Paragraph paragraph in mainPart.Document.Body.Elements<Paragraph>())
            {
                // Iterate through all runs in the paragraph
                foreach (Run run in paragraph.Elements<Run>())
                {
                    // Find and replace the placeholder with the new text
                    foreach (Text text in run.Elements<Text>())
                    {
                        if (text.Text.Contains(placeholder))
                        {
                            text.Text = text.Text.Replace(placeholder, newText);
                        }
                    }
                }
            }
        }
    }
}
