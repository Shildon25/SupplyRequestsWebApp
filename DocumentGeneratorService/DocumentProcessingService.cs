using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using SupplyManagement.Models.Enums;
using System.Data;
using System.Data.SqlClient;

namespace SupplyManagement.DocumentGeneratorService
{
    public class DocumentProcessingService : BackgroundService
    {
        private const string SupplyDocumentTemplateFilePath = "Templates//Supply Document.docx";
        private const string ClaimsDocumentTemplateFilePath = "Templates//Claims Document.docx";

        private readonly string _filePathBase;
        private readonly string _connectionString;
        private readonly string _storageConnectionString;
        private readonly string _containerName;
        private readonly ILogger<DocumentProcessingService> _logger;

        public DocumentProcessingService(string filePathBase, string connectionString, string storageConnectionString, string containerName, ILogger<DocumentProcessingService> logger)
        {
            _filePathBase = filePathBase;
            _connectionString = connectionString;
            _storageConnectionString = storageConnectionString;
            _containerName = containerName;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Retrieve documents from the database
                    List<SupplyDocument> supplyRequestDocuments = [];
                    List<ClaimsDocument> claimsDocuments = [];
                    BlobServiceClient blobServiceClient = new(_storageConnectionString);
                    BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(_containerName);

                    // Create the container if it doesn't exist
                    await containerClient.CreateIfNotExistsAsync(cancellationToken: stoppingToken);

                    await GetRequestsFromDatabase(_connectionString, supplyRequestDocuments, claimsDocuments);

                    // Process requests in parallel
                    await ProcessRequests(supplyRequestDocuments, claimsDocuments, _filePathBase, containerClient);

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
                        "STRING_AGG(CONCAT('Item: ', i.Name, '; Vendor: ', v.Name), ', ') AS Items, " +
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
                    SqlDataReader reader = await command.ExecuteReaderAsync() ?? throw new NullReferenceException("Failed to retrieve reader for the query.");

                    while (await reader.ReadAsync())
                    {
                        int requestId = reader.GetInt32(0);
                        int requestStatus = reader.GetInt32(1);
                        List<string> items = reader.GetString(2).Split(',').ToList();
                        string createdBy = reader.GetString(3) ?? "";
                        string approvedBy = reader.GetString(4) ?? "";
                        string deliveredBy = reader.GetString(5) ?? "";
                        string claimsText = reader.IsDBNull(6) ? "" : reader.GetString(6);

                        if (requestStatus == (int)SupplyRequestStatuses.Approved)
                        {
                            var approvedDoc = new SupplyDocument(requestId, createdBy, approvedBy, items);
                            supplyRequestDocuments.Add(approvedDoc);
                        }
                        else if (requestStatus == (int)SupplyRequestStatuses.DeliveredWithClaims)
                        {
                            var claimsDoc = new ClaimsDocument(requestId, createdBy, approvedBy, deliveredBy, claimsText, items);
                            claimsDocuments.Add(claimsDoc);
                        }
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

        private async Task ProcessRequests(List<SupplyDocument> supplyRequestDocuments, List<ClaimsDocument> claimsDocuments, string filePathBase, BlobContainerClient containerClient)
        {
            try
            {
                var tasks = new List<Task>();

                // Process supply request documents
                tasks.AddRange(supplyRequestDocuments.Select(async document =>
                {
                    try
                    {
                        // Generate supply document
                        string filePath = Path.Combine(filePathBase, $"SupplyDocument_{document.RequestId}.docx");


                        DocumentGenerator.GenerateSupplyDocument(document, filePath, SupplyDocumentTemplateFilePath);

                        await UploadFileToBlobStorage(containerClient, filePath);

                        await CheckDocumentCreationAndChangeStatus(_connectionString, filePath, document.RequestId, SupplyRequestStatuses.DelailsDocumentGenerated, containerClient);
                    }
                    catch (Exception ex)
                    {
                        // Log error
                        _logger.LogError(ex, "An error occurred while generating supply document for request ID: {id}", document.RequestId);
                    }
                }));

                // Process claims documents
                tasks.AddRange(claimsDocuments.Select(async document =>
                {
                    try
                    {
                        // Generate claims document
                        string filePath = Path.Combine(filePathBase, $"ClaimsDocument_{document.RequestId}.docx");

                        DocumentGenerator.GenerateClaimsDocument(document, filePath, ClaimsDocumentTemplateFilePath);

                        await UploadFileToBlobStorage(containerClient, filePath);

                        await CheckDocumentCreationAndChangeStatus(_connectionString, filePath, document.RequestId, SupplyRequestStatuses.ClaimsDocumentGenerated, containerClient);
                    }
                    catch (Exception ex)
                    {
                        // Log error
                        _logger.LogError(ex, "An error occurred while generating claims document for request ID: {id}", document.RequestId);
                    }
                }));

                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                // Log error
                _logger.LogError(ex, "An error occurred while processing requests.");
            }
        }

        private async Task CheckDocumentCreationAndChangeStatus(string connectionString, string filePath, int requestId, SupplyRequestStatuses status, BlobContainerClient containerClient)
        {
            try
            {
                // Check if the blob exists in the container
                if (await BlobExistsAsync(containerClient, Path.GetFileName(filePath)))
                {
                    // Update status in the database
                    await UpdateStatusInDatabase(connectionString, requestId, status);
                }
                else
                {
                    // Log info
                    _logger.LogInformation("Document not found for request ID: {id}. Status not updated.", requestId);
                }
            }
            catch (Exception ex)
            {
                // Log error
                _logger.LogError(ex, "An error occurred while checking document creation and changing status for request ID: {id}", requestId);
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
                    SqlCommand command = new(query, connection);
                    command.Parameters.AddWithValue("@NewStatus", (int)newStatus);
                    command.Parameters.AddWithValue("@RequestId", requestId);

                    int rowsAffected = await command.ExecuteNonQueryAsync();
                    if (rowsAffected > 0)
                    {
                        // Log info
                        _logger.LogInformation("Status updated successfully for request ID: {requestId}", requestId);
                    }
                    else
                    {
                        // Log error
                        _logger.LogError("Failed to update status for request ID: {requestId}", requestId);
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error
                _logger.LogError(ex, "An error occurred while updating status in the database for request ID: {requestId}", requestId);
            }
        }

        private async Task UploadFileToBlobStorage(BlobContainerClient containerClient, string filePath)
        {
            BlobClient blobClient = containerClient.GetBlobClient(Path.GetFileName(filePath));

            // Upload the file to blob storage
            await blobClient.UploadAsync(filePath, true);

            // Log information
            _logger.LogInformation("File {fileName} uploaded to Azure Storage.", Path.GetFileName(filePath));
        }

        private async Task<bool> BlobExistsAsync(BlobContainerClient containerClient, string blobName)
        {
            try
            {
                // Check if the blob exists in the container
                return await containerClient.GetBlobClient(blobName).ExistsAsync();
            }
            catch (RequestFailedException ex) when (ex.ErrorCode == BlobErrorCode.BlobNotFound)
            {
                // If the blob does not exist, return false
                return false;
            }
            catch (Exception ex)
            {
                // Log any other errors
                _logger.LogError(ex, "An error occurred while checking if blob {fileName} exists.", blobName);
                throw; // Rethrow the exception for the caller to handle
            }
        }
    }
}
