using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TPOR.Shared.Data;
using TPOR.Shared.Services;
using TPOR.Intranet.API.Services;
using TPOR.Intranet.API.Middleware;
using TPOR.Shared.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Load .env file
DotNetEnv.Env.Load();

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure AppSettings
builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));

// Database - API doesn't need database connection
// builder.Services.AddDbContext<TporDbContext>(options =>
// {
//     var connectionString = Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING") 
//         ?? builder.Configuration.GetConnectionString("DefaultConnection");
//     options.UseMySql(connectionString, new MySqlServerVersion(new Version(8, 0, 21)));
// });

// JWT Authentication
var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET") 
    ?? builder.Configuration["AppSettings:Authentication:JwtSecret"];

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret ?? "default-secret-key")),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// Register services
builder.Services.AddScoped<IFileService, FileService>();
// builder.Services.AddScoped<IDatabaseService, DatabaseService>(); // API doesn't need database

// Register services based on environment
var isProduction = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Production";
if (isProduction)
{
    // Production: use Google Cloud services
    builder.Services.AddScoped<IMessageQueueService, MessageQueueService>();
    builder.Services.AddScoped<IAuthenticationService, ProductionAuthenticationService>();
}
else
{
    // Development mode: use simplified implementations without Google Cloud
    builder.Services.AddScoped<IMessageQueueService, DevMessageQueueService>();
    builder.Services.AddScoped<IAuthenticationService, DevAuthenticationService>();
}

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
// Enable Swagger for development environments
var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
if (app.Environment.IsDevelopment() || environment == "Development")
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "TPOR Intranet API V1");
        c.RoutePrefix = "swagger";
        c.DocumentTitle = "TPOR Intranet API";
        c.DisplayRequestDuration();
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

// Custom middleware
app.UseMiddleware<ErrorHandlingMiddleware>();

app.MapControllers();

// Add a simple route for testing
app.MapGet("/", () => "TPOR Intranet API is running!");

// Ensure database is created (disabled for development without database)
// using (var scope = app.Services.CreateScope())
// {
//     var context = scope.ServiceProvider.GetRequiredService<TporDbContext>();
//     context.Database.EnsureCreated();
// }

app.Run();
