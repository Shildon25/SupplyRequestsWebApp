using SupplyManagement.Models.Enums;
using System.Data;
using System.Data.SqlClient;

namespace SupplyManagement.DocumentGeneratorService
{
    public class DocumentProcessingService : BackgroundService
    {
        private readonly string _filePathBase;
        private readonly string _connectionString;
        private readonly ILogger<DocumentProcessingService> _logger;

        public DocumentProcessingService(string filePathBase, string connectionString, ILogger<DocumentProcessingService>? logger)
        {
            _filePathBase = filePathBase;
            _connectionString = connectionString;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Retrieve documents from the database
                    List<SupplyDocument> supplyRequestDocuments = new List<SupplyDocument>();
                    List<ClaimsDocument> claimsDocuments = new List<ClaimsDocument>();

                    await GetRequestsFromDatabase(_connectionString, supplyRequestDocuments, claimsDocuments);

                    // Process requests in parallel
                    await ProcessRequests(supplyRequestDocuments, claimsDocuments, _filePathBase);

                    // Update document statuses in the database
                    foreach (var document in supplyRequestDocuments)
                    {
                        var filePath = Path.Combine(_filePathBase, $"SupplyDocument_{document.RequestId}.docx");
                        await CheckDocumentCreationAndChangeStatus(_connectionString, filePath, document.RequestId, SupplyRequestStatuses.DelailsDocumentGenerated);
                    }

                    foreach (var document in claimsDocuments)
                    {
                        var filePath = Path.Combine(_filePathBase, $"ClaimsDocument_{document.RequestId}.docx");
                        await CheckDocumentCreationAndChangeStatus(_connectionString, filePath, document.RequestId, SupplyRequestStatuses.ClaimsDocumentGenerated);
                    }

                    // Log information
                    _logger.LogInformation("Document processing completed.");

                    // Wait for 1 hour before checking the database again
                    await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
                }
                catch (Exception ex)
                {
                    // Log error
                    _logger.LogError(ex, "An error occurred while processing documents.");

                    // Handle the exception as needed
                    // For example, you can rethrow the exception, log it and continue, or perform other actions.
                }
            }
        }

        private async Task<(List<SupplyDocument>, List<ClaimsDocument>)> GetRequestsFromDatabase(string connectionString, List<SupplyDocument> supplyRequestDocuments, List<ClaimsDocument> claimsDocuments)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    // Query to retrieve documents from the database
                    string query = "SELECT sr.Id, sr.Status, " +
                        "STRING_AGG(CONCAT(i.Name, ' - ', v.Name), ', ') AS Items, " +
                        "CONCAT(cu.Name, ' ', cu.Surname) AS CreatedBy, " +
                        "CONCAT(au.Name, ' ', au.Surname) AS ApprovedBy, " +
                        "CONCAT(du.Name, ' ', du.Surname) AS DeliveredBy, " +
                        "sr.ClaimsText AS ClaimsText " +
                        "FROM SupplyRequests sr " +
                        "LEFT JOIN AspNetUsers cu ON cu.Id = sr.CreatedByUserId " +
                        "LEFT JOIN AspNetUsers au ON au.Id = sr.ApprovedByUserId " +
                        "LEFT JOIN AspNetUsers du ON du.Id = sr.DeliveredByUserId " +
                        "JOIN ItemSupplyRequests isr ON isr.SupplyRequestId = sr.Id " +
                        "JOIN Items i ON isr.ItemId = i.Id " +
                        "JOIN Vendors v ON v.Id = i.VendorId " +
                        "WHERE sr.Status = @ApprovedStatus OR sr.Status = @ClaimsStatus " +
                        "GROUP BY sr.Id, sr.Status, cu.Name, cu.Surname, au.Name, au.Surname, du.Name, du.Surname, sr.ClaimsText";
                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.Add("@ApprovedStatus", SqlDbType.Int);
                    command.Parameters["@ApprovedStatus"].Value = (int)SupplyRequestStatuses.Approved;
                    command.Parameters.Add("@ClaimsStatus", SqlDbType.Int);
                    command.Parameters["@ClaimsStatus"].Value = (int)SupplyRequestStatuses.DeliveredWithClaims;
                    SqlDataReader reader = await command.ExecuteReaderAsync();

                    while (await reader.ReadAsync())
                    {
                        // Read data from the database
                        // Process data and populate supplyRequestDocuments and claimsDocuments lists
                    }

                    reader.Close();
                }
            }
            catch (Exception ex)
            {
                // Log error
                _logger.LogError(ex, "An error occurred while retrieving documents from the database.");
            }

            return (supplyRequestDocuments, claimsDocuments);
        }

        private async Task ProcessRequests(List<SupplyDocument> supplyRequestDocuments, List<ClaimsDocument> claimsDocuments, string filePathBase)
        {
            try
            {

                // Process requests in parallel
                await Task.WhenAll(supplyRequestDocuments.Select(async document =>
                {
                    try
                    {
                        // Generate supply document
                        DocumentGenerator.GenerateSupplyDocument(document, Path.Combine(filePathBase, $"SupplyDocument_{document.RequestId}.docx"));
                    }
                    catch (Exception ex)
                    {
                        // Log error
                        _logger.LogError(ex, $"An error occurred while generating supply document for request ID: {document.RequestId}");
                    }
                }));

                await Task.WhenAll(claimsDocuments.Select(async document =>
                {
                    try
                    {
                        // Generate claims document
                        DocumentGenerator.GenerateClaimsDocument(document, Path.Combine(filePathBase, $"ClaimsDocument_{document.RequestId}.docx"));
                    }
                    catch (Exception ex)
                    {
                        // Log error
                        _logger.LogError(ex, $"An error occurred while generating claims document for request ID: {document.RequestId}");
                    }
                }));
            }
            catch (Exception ex)
            {
                // Log error
                _logger.LogError(ex, "An error occurred while processing requests.");
            }
        }

        private async Task CheckDocumentCreationAndChangeStatus(string connectionString, string filePath, int requestId, SupplyRequestStatuses status)
        {
            try
            {
                // Check if the document was created
                if (File.Exists(filePath))
                {
                    // Update status in the database
                    await UpdateStatusInDatabase(connectionString, requestId, status);
                }
                else
                {
                    // Log info
                    _logger.LogInformation($"Document not found for request ID: {requestId}. Status not updated.");
                }
            }
            catch (Exception ex)
            {
                // Log error
                _logger.LogError(ex, $"An error occurred while checking document creation and changing status for request ID: {requestId}");
            }
        }

        private async Task UpdateStatusInDatabase(string connectionString, int requestId, SupplyRequestStatuses newStatus)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    string query = "UPDATE SupplyRequests SET Status = @NewStatus WHERE Id = @RequestId";
                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@NewStatus", (int)newStatus);
                    command.Parameters.AddWithValue("@RequestId", requestId);

                    int rowsAffected = await command.ExecuteNonQueryAsync();
                    if (rowsAffected > 0)
                    {
                        // Log info
                        _logger.LogInformation($"Status updated successfully for request ID: {requestId}");
                    }
                    else
                    {
                        // Log error
                        _logger.LogError($"Failed to update status for request ID: {requestId}");
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error
                _logger.LogError(ex, $"An error occurred while updating status in the database for request ID: {requestId}");
            }
        }
    }
}
