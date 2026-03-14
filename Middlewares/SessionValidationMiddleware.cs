using EncantoWebAPI.Models.Auth;
using MongoDB.Driver;

// This middleware pipeline intercepts incoming HTTP requests (API calls) before they reach the controllers(endpoints).
// It checks the session-key in the request headers, validates it against the MongoDB "Sessions" collection, and ensures it hasn’t expired.
// Valid sessions are attached to HttpContext. Items for downstream controllers (success scenario), while invalid or missing sessions return a 401 Unauthorized (failure scenario).

namespace EncantoWebAPI.Middlewares
{
    public class SessionValidationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IMongoCollection<SessionDetails> _sessionCollection;

        public SessionValidationMiddleware(RequestDelegate next, string mongoConnection, string databaseName, string sessionsCollectionName)
        {
            _next = next;

            var client = new MongoClient(mongoConnection);
            var database = client.GetDatabase(databaseName);
            _sessionCollection = database.GetCollection<SessionDetails>(sessionsCollectionName);
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Skip validation for health checks and auth endpoints
            var path = context.Request.Path.Value?.ToLower();
            if (path != null && (path.Contains("/auth/login") 
                || path.Contains("/auth/signup") 
                || path.Contains("/test-db-connection") 
                || path.Contains("/swagger")
                || path.Contains("/api/mongodbhealth/health")))
            {
                await _next(context);
                return;
            }

            // Extract session-key
            if (!context.Request.Headers.TryGetValue("session-key", out var sessionKey))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Missing session-key header");
                return;
            }

            // Validate session
            var session = await _sessionCollection
                .Find(s => s.SessionKey == sessionKey.ToString())
                .FirstOrDefaultAsync();

            if (session == null)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Invalid session key");
                return;
            }

            // If session expired
            var currentTimestamp = ((DateTimeOffset)DateTime.UtcNow).ToUnixTimeMilliseconds();
            if (session.ExpirationTimestamp < currentTimestamp)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("Session expired");
                return;
            }

            // Attach user id & session key for controllers
            context.Items["UserId"] = session.UserId;
            context.Items["SessionKey"] = sessionKey.ToString();

            await _next(context);
        }
    }
}
