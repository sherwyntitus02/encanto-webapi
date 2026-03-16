using EncantoWebAPI.Hubs;
using EncantoWebAPI.Accessors; // Ensure this matches your namespace
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// --- 1. CONFIGURATION & LOGGING ---
// These will pull from appsettings.json OR Azure Environment Variables (Azure wins!)
string environmentTest = builder.Configuration["EnvironmentTest"] ?? "Not Set";
Console.WriteLine($"DEBUG: Effective EnvironmentTest value: {environmentTest}");

// --- 2. REGISTER SERVICES ---
builder.Services.AddControllers();
builder.Services.AddSignalR();

// IMPORTANT: Register your Accessor so .NET can inject the correct Configuration into it
builder.Services.AddSingleton<MongoDBAccessor>();

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

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("sessionKey", new OpenApiSecurityScheme
    {
        Description = "Enter the session key as received from /auth/login.",
        Name = "session-key",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "sessionKey"
    });

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

// --- 3. CONFIGURE PIPELINE ---

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");

// --- 4. MIDDLEWARE SETUP ---
// We pull directly from app.Configuration to ensure we have the Azure overrides
var mongoConnection = app.Configuration["MongoDBSettings:ConnectionURI"];
var mongoDbName = app.Configuration["MongoDBSettings:DatabaseName"];
var sessionsCollection = app.Configuration["MongoDBSettings:SessionsCollectionName"];

app.UseMiddleware<EncantoWebAPI.Middlewares.SessionValidationMiddleware>(
    mongoConnection!,
    mongoDbName!,
    sessionsCollection!
);

app.UseAuthorization();

app.MapControllers();
app.MapHub<NotificationHub>("/notificationHub"); // socket endpoint

app.Run();