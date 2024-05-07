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
    var keyVaultUrl = builder.Configuration.GetSection("AzureKeyVault")["KeyVaultUrl"] ?? throw new KeyNotFoundException("Configuration key 'KeyVaultUrl' wasn't found.");
    var credential = new ManagedIdentityCredential();
    var client = new SecretClient(new Uri(keyVaultUrl), credential);

    var storageConnectionString = configuration.GetSection("Azure")["StorageConnectionString"] ?? throw new KeyNotFoundException("Configuration key 'StorageConnectionString' wasn't found.");
    var containerName = configuration.GetSection("Azure")["ContainerName"] ?? throw new KeyNotFoundException("Configuration key 'ContainerName' wasn't found.");
    var connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new KeyNotFoundException("Configuration key 'DefaultConnection' wasn't found.");

    // Inject the logger into the DocumentProcessingService constructor
    var logger = serviceProvider.GetRequiredService<ILogger<DocumentProcessingService>>();
    return new DocumentProcessingService(filePathBase, connectionString, storageConnectionString, containerName, logger);
});

var host = builder.Build();
host.Run();
