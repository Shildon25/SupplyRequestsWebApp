using SupplyManagement.DocumentGeneratorService;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

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

    var storageConnectionString = configuration.GetSection("Azure")["StorageConnectionString"];
    var containerName = configuration.GetSection("Azure")["ContainerName"];
    var connectionString = configuration.GetConnectionString("DefaultConnection");

    // Inject the logger into the DocumentProcessingService constructor
    var logger = serviceProvider.GetRequiredService<ILogger<DocumentProcessingService>>();
    return new DocumentProcessingService(filePathBase, connectionString, storageConnectionString, containerName, logger);
});

var host = builder.Build();
host.Run();
