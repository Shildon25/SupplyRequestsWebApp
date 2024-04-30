using SupplyManagement.DocumentGeneratorService;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<DocumentProcessingService>(serviceProvider =>
{
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();
    var filePathBase = configuration["FilePathBase"];
    var connectionString = configuration.GetConnectionString("DefaultConnection");

    return new DocumentProcessingService(filePathBase, connectionString);
});

var host = builder.Build();
host.Run();
