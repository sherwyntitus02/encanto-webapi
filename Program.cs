using EncantoWebAPI.Accessors;
using EncantoWebAPI.Config;
using EncantoWebAPI.Hubs;
using EncantoWebAPI.Managers;
using EncantoWebAPI.Services;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

// Configure MongoDB Settings
var mongoDbSettings = new MongoDbSettings
{
    ConnectionString = builder.Configuration["MongoDbConnectionString"] 
        ?? throw new InvalidOperationException("MongoDB connection string 'MongoDbConnectionString' not found."),
    DatabaseName = builder.Configuration["MongoDbSettings:DatabaseName"] 
        ?? throw new InvalidOperationException("MongoDB database name not found in configuration.")
};

// Register MongoDB Services
builder.Services.Configure<MongoDbSettings>(options =>
{
    options.ConnectionString = mongoDbSettings.ConnectionString;
    options.DatabaseName = mongoDbSettings.DatabaseName;
});

// Register MongoClient as Singleton
builder.Services.AddSingleton<MongoClient>(sp =>
{
    var settings = MongoClientSettings.FromConnectionString(mongoDbSettings.ConnectionString);
    settings.ServerSelectionTimeout = TimeSpan.FromSeconds(30);
    settings.ConnectTimeout = TimeSpan.FromSeconds(30);
    settings.SocketTimeout = TimeSpan.FromSeconds(30);
    return new MongoClient(settings);
});

// Register MongoDbService as Singleton
builder.Services.AddSingleton<MongoDbService>(provider =>
{
    var mongoClient = provider.GetRequiredService<MongoClient>();
    return new MongoDbService(mongoClient, mongoDbSettings.DatabaseName);
});

// Register Data Accessors with Dependency Injection
builder.Services.AddScoped<MongoDBAccessor>();
builder.Services.AddScoped<AuthenticationAccessor>();
builder.Services.AddScoped<UserDetailsAccessor>();
builder.Services.AddScoped<EventDetailsAccessor>();

// Register Business Logic Managers with Dependency Injection
builder.Services.AddScoped<AuthenticationManager>();
builder.Services.AddScoped<UserDetailsManager>();
builder.Services.AddScoped<EventDetailsManager>();

// Add services to the container.
builder.Services.AddControllers();

// Add SignalR service
builder.Services.AddSignalR();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .SetIsOriginAllowed(origin =>
            {
                var uri = new Uri(origin);
                return uri.Host == "localhost"
                       || uri.Host == "127.0.0.1"
                       || origin == "https://delightful-wave-0f9aba300.4.azurestaticapps.net";
            })
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// Add Swagger/OpenAPI services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // Define the security scheme for session-key header
    options.AddSecurityDefinition("sessionKey", new OpenApiSecurityScheme
    {
        Description = "Enter the session key as received from /auth/login. Example: abcdef12345",
        Name = "session-key",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "sessionKey"
    });

    // Make the session-key required for all endpoints in Swagger UI (you can override per-operation if needed)
    var securityRequirement = new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "sessionKey" }
            },
            new string[] { }
        }
    };

    options.AddSecurityRequirement(securityRequirement);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
// Enable Swagger in ALL environments (not just Development)
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseCors("AllowFrontend"); // Enables CORS (before authorization & controllers)

// Get MongoDB settings from DI container for middleware
var mongoDbSettingsForMiddleware = app.Services.GetRequiredService<IOptions<MongoDbSettings>>().Value;

// Adds session validation middleware BEFORE controllers
app.UseMiddleware<EncantoWebAPI.Middlewares.SessionValidationMiddleware>(
    mongoDbSettingsForMiddleware.ConnectionString,
    mongoDbSettingsForMiddleware.DatabaseName,
    builder.Configuration["MongoDBSettings:SessionsCollectionName"]!
);

app.UseAuthorization();

// Map both Controllers and your Hub endpoint
app.MapControllers();
app.MapHub<NotificationHub>("/notificationHub"); // socket endpoint

app.Run();
