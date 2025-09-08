using Microsoft.EntityFrameworkCore;
using TPOR.Shared.Data;
using TPOR.Shared.Services;
using TPOR.Intranet.Worker.Services;
using TPOR.Shared.Configuration;
using Microsoft.Extensions.Options;

var builder = Host.CreateApplicationBuilder(args);

// Load .env file
DotNetEnv.Env.Load();

// Configure AppSettings
builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));

// Register services based on environment
var isProduction = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production";
var isDevelopmentLocal = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development" 
    && string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING"));

if (isDevelopmentLocal)
{
    // Development Local: No database dependency
    builder.Services.AddScoped<IFileService, WorkerFileService>();
    builder.Services.AddScoped<IMessageQueueService, WorkerMessageQueueService>();
    // No database service for local development
    builder.Services.AddScoped<IFileProcessingService, FileProcessingService>();
}
else
{
    // Development Docker and Production: Database required
    builder.Services.AddDbContext<TporDbContext>(options =>
    {
        var connectionString = Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING") 
            ?? builder.Configuration.GetConnectionString("DefaultConnection");
        // Use a fixed server version instead of AutoDetect to avoid immediate connection attempt
        options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 21)));
    });
    
    builder.Services.AddScoped<IFileService, WorkerFileService>();
    builder.Services.AddScoped<IMessageQueueService, WorkerMessageQueueService>();
    builder.Services.AddScoped<IDatabaseService, WorkerDatabaseService>();
    builder.Services.AddScoped<IFileProcessingService, FileProcessingService>();
}

// Register the worker service
builder.Services.AddHostedService<TPOR.Intranet.Worker.FileProcessingWorker>();

var host = builder.Build();

// Database setup only for environments that require it
if (!isDevelopmentLocal)
{
    // Ensure database is created with error handling
    using (var scope = host.Services.CreateScope())
    {
        try
        {
            var context = scope.ServiceProvider.GetRequiredService<TporDbContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            
            logger.LogInformation("Attempting to connect to database...");
            context.Database.EnsureCreated();
            logger.LogInformation("Database connection successful and database ensured.");
        }
        catch (Exception ex)
        {
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "Failed to connect to database. Worker service cannot start without database connection.");
            logger.LogError("Please ensure MySQL server is running and connection string is correct.");
            logger.LogError("Connection string: {ConnectionString}", 
                Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING") 
                ?? builder.Configuration.GetConnectionString("DefaultConnection"));
            
            // Exit the application with error code
            Environment.Exit(1);
        }
    }
}
else
{
    var logger = host.Services.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("Development Local mode: Worker service starting without database dependency.");
}

host.Run();
