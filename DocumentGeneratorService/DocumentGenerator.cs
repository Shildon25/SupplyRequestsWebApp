using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace SupplyManagement.DocumentGeneratorService
{
    public static class DocumentGenerator
	{
        public static void GenerateSupplyDocument(SupplyDocument supplyDocument, string filePath)
        {
            using (WordprocessingDocument doc = WordprocessingDocument.Create(filePath, WordprocessingDocumentType.Document))
            {
                MainDocumentPart mainPart = doc.AddMainDocumentPart();
                mainPart.Document = new Document();
                Body body = new Body();

                CreateParagraph(body, String.Format("Request id: {0}", supplyDocument.RequestId.ToString()));
                CreateEmptyParagraph(body);
                CreateParagraph(body, String.Format("Request owner: {0};  Signature:", supplyDocument.RequestOwnerName));
                CreateEmptyParagraph(body);
                CreateParagraph(body, String.Format("Request manager: {0};  Signature:", supplyDocument.ApprovalManagerName));

                CreateEmptyParagraph(body);
                CreateParagraph(body, "Items list:");

                foreach (string item in supplyDocument.RequestItems)
                {
                    CreateParagraph(body, item);
                }

                mainPart.Document.Append(body);
            }
        }

        public static void GenerateClaimsDocument(ClaimsDocument claimsDocument, string filePath)
        {
            using (WordprocessingDocument doc = WordprocessingDocument.Create(filePath, WordprocessingDocumentType.Document))
            {
                MainDocumentPart mainPart = doc.AddMainDocumentPart();
                mainPart.Document = new Document();
                Body body = new Body();
                CreateParagraph(body, String.Format("Request id: {0}", claimsDocument.RequestId.ToString()));
                CreateEmptyParagraph(body);
                CreateParagraph(body, String.Format("Request owner: {0};  Signature:", claimsDocument.RequestOwnerName));
                CreateEmptyParagraph(body);
                CreateParagraph(body, String.Format("Request manager: {0};  Signature:", claimsDocument.ApprovalManagerName));
                CreateEmptyParagraph(body);
                CreateParagraph(body, String.Format("Request courier: {0};  Signature:", claimsDocument.CourierName));

                CreateEmptyParagraph(body);
                CreateParagraph(body, "Items list:");

                foreach (string item in claimsDocument.RequestItems)
                {
                    CreateParagraph(body, item);
                }

                CreateEmptyParagraph(body);
                CreateParagraph(body, "Claims text:");
                CreateParagraph(body, claimsDocument.ClaimsText);

                mainPart.Document.Append(body);
            }
        }

        static void CreateParagraph(Body body, string text)
        {
            Paragraph paragraph = new Paragraph();
            Run run = new Run();
            Text textElement = new Text(text);

            // Apply formatting
            RunProperties runProperties = new RunProperties();
            FontSize fontSize = new FontSize() { Val = "24" }; // Font size 24
            Color color = new Color() { Val = "0000FF" }; // Blue color
            Bold bold = new Bold(); // Bold text

            runProperties.Append(fontSize);
            runProperties.Append(color);
            runProperties.Append(bold);

            run.Append(runProperties);
            run.Append(textElement);
            paragraph.Append(run);

            body.Append(paragraph);
        }

        static void CreateEmptyParagraph(Body body)
        {
            Paragraph paragraph = new Paragraph();
            ParagraphProperties paragraphProperties = new ParagraphProperties();
            SpacingBetweenLines spacingBetweenLines = new SpacingBetweenLines() { Before = "240", After = "240" }; // 240 = 1/3 inch

            paragraphProperties.Append(spacingBetweenLines);
            paragraph.Append(paragraphProperties);

            body.Append(paragraph);
        }
    }
}
