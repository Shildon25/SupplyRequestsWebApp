using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using SupplyManagement.DocumentGeneratorService;

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.AddHostedService<DocumentProcessingService>(serviceProvider =>
{
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();
    var filePathBase = $"C:\\Users\\{Environment.UserName}\\Desktop\\Documents";

    //Get Secrets from Azure Key Vault
    var keyVaultUrl = builder.Configuration.GetSection("AzureKeyVault")["KeyVaultUrl"];
    var credential = new ManagedIdentityCredential();
    var client = new SecretClient(new Uri(keyVaultUrl), credential);

    string connectionString = client.GetSecret("DatabaseConnectionString").Value.Value ?? throw new InvalidOperationException("Database Connection string not found.");
    var storageConnectionString = client.GetSecret("AzureStorageConnectionString").Value.Value ?? throw new InvalidOperationException("Storage Connection string not found.");
    var containerName = client.GetSecret("AzureStorageContainerName").Value.Value ?? throw new InvalidOperationException("Azure Container Name not found.");

    // Inject the logger into the DocumentProcessingService constructor
    var logger = serviceProvider.GetRequiredService<ILogger<DocumentProcessingService>>();
    return new DocumentProcessingService(filePathBase, connectionString, storageConnectionString, containerName, logger);
});

var host = builder.Build();
host.Run();
