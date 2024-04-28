using SupplyManagement.WebApp.Models.Enums;
using System.Data;
using System.Data.SqlClient;

namespace SupplyManagement.DocumentGeneratorApp
{
	class Program
	{
		static async Task Main(string[] args)
		{
			Console.WriteLine("Waiting for trigger. Press any key to start processing requests...");
			Console.ReadKey();
			List<SupplyDocument> supplyRequestDocuments = new List<SupplyDocument>();
			List<ClaimsDocument> claimsDocuments = new List<ClaimsDocument>();
			string filePathBase = "C:\\Users\\xdrul\\Desktop\\Documents\\";

			string connectionString = "Server=(localdb)\\mssqllocaldb;Database=SupplyRequestsWebApp;Trusted_Connection=True;MultipleActiveResultSets=true"; // Replace this with your actual connection string
			await GetRequestsFromDatabase(connectionString, supplyRequestDocuments, claimsDocuments);

			// Process requests in parallel
			await ProcessRequests(supplyRequestDocuments, claimsDocuments, filePathBase);

			Console.WriteLine("Request processing completed.");

			foreach (var document in supplyRequestDocuments) {
				var filePath = Path.Combine(filePathBase, String.Format("SupplyDocument_{0}.docx", document.RequestId));
				await CheckDocumentCreationAndChangeStatus(connectionString, filePath, document.RequestId, SupplyRequestStatuses.DelailsDocumentGenerated);
			}

			foreach (var document in claimsDocuments)
			{
				var filePath = Path.Combine(filePathBase, String.Format("ClaimsDocument_{0}.docx", document.RequestId));
				await CheckDocumentCreationAndChangeStatus(connectionString, filePath, document.RequestId, SupplyRequestStatuses.ClaimsDocumentGenerated);
			}

			Console.WriteLine("Requests statuses updated.");

			Console.ReadLine();
		}
		
		static async Task<(List<SupplyDocument>, List<ClaimsDocument>)> GetRequestsFromDatabase(string connectionString, List<SupplyDocument> supplyRequestDocuments, List<ClaimsDocument> claimsDocuments)
		{
			using (SqlConnection connection = new SqlConnection(connectionString))
			{
				await connection.OpenAsync();

				// To fix
				// Should validate that item name and vendor name don't have special char.
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
					int requestId = reader.GetInt32(0);
					int requestStatus = reader.GetInt32(1);
					List<string> items = reader.GetString(2).Split(',').ToList();
					string createdBy = reader.GetString(3) ?? "";
					string approvedBy = reader.GetString(4) ?? "";
					string deliveredBy = reader?.GetString(5) ?? "";
					string claimsText = reader.IsDBNull(6) ? "" : reader?.GetString(6);

					if (requestStatus == (int)SupplyRequestStatuses.Approved)
					{
						var approvedDoc = new SupplyDocument(requestId,createdBy, approvedBy, items);
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

			return (supplyRequestDocuments, claimsDocuments);
		}

		static async Task ProcessRequests(List<SupplyDocument> supplyRequestDocuments, List<ClaimsDocument> claimsDocuments, string filePathBase)
		{
			// Process requests in parallel
			await Task.WhenAll((supplyRequestDocuments.ConvertAll(async document =>
			{
				await Task.Run(() => DocumentGenerator.GenerateSupplyDocument(document, Path.Combine(filePathBase, String.Format("SupplyDocument_{0}.docx", document.RequestId)))).ConfigureAwait(false);
			}).ToArray())
			.Union(claimsDocuments.ConvertAll(async document =>
			{
				await Task.Run(() => DocumentGenerator.GenerateClaimsDocument(document, Path.Combine(filePathBase, String.Format("ClaimsDocument_{0}.docx", document.RequestId)))).ConfigureAwait(false);
			}).ToArray()));
		}


		static async Task CheckDocumentCreationAndChangeStatus(string connectionString, string filePath, int requestId, SupplyRequestStatuses status)
		{
			// Check if the document was created
			if (File.Exists(filePath))
			{
				// Update status in the database
				await UpdateStatusInDatabase(connectionString, requestId, status);
			}
			else
			{
				Console.WriteLine("Document not found. Status not updated.");
			}
		}

		static async Task UpdateStatusInDatabase(string connectionString, int requestId, SupplyRequestStatuses newStatus)
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
					Console.WriteLine($"Status updated successfully for request ID: {requestId}");
				}
				else
				{
					Console.WriteLine($"Failed to update status for request ID: {requestId}");
				}
			}
		}
	}
}