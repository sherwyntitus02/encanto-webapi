using EncantoWebAPI.Hubs;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Read from appsettings.json
string mongoConnection = builder.Configuration["MongoDBSettings:ConnectionURI"]!;
string mongoDbName = builder.Configuration["MongoDbSettings:DatabaseName"]!;
string sessionsCollectionName = builder.Configuration["MongoDBSettings:SessionsCollectionName"]!;

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

// Adds session validation middleware BEFORE controllers
app.UseMiddleware<EncantoWebAPI.Middlewares.SessionValidationMiddleware>(
    mongoConnection,
    mongoDbName,
    sessionsCollectionName
);

app.UseAuthorization();

// Map both Controllers and your Hub endpoint
app.MapControllers();
app.MapHub<NotificationHub>("/notificationHub"); // socket endpoint

app.Run();
