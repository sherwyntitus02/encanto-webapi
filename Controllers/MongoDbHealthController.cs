using EncantoWebAPI.Config;
using EncantoWebAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace EncantoWebAPI.Controllers
{
    /// <summary>
    /// Health check controller for MongoDB connection verification.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class MongoDbHealthController : ControllerBase
    {
        private readonly MongoDbService _mongoDbService;
        private readonly ILogger<MongoDbHealthController> _logger;
        private readonly IOptions<MongoDbSettings> _mongoDbSettings;

        public MongoDbHealthController(MongoDbService mongoDbService, ILogger<MongoDbHealthController> logger, IOptions<MongoDbSettings> mongoDbSettings)
        {
            _mongoDbService = mongoDbService;
            _logger = logger;
            _mongoDbSettings = mongoDbSettings;
        }

        /// <summary>
        /// Health check endpoint to verify MongoDB connection.
        /// </summary>
        /// <returns>200 OK if connection is successful; 503 Service Unavailable otherwise.</returns>
        [HttpGet("health")]
        public IActionResult HealthCheck()
        {
            try
            {
                var databaseName = _mongoDbSettings.Value.DatabaseName;

                if (_mongoDbService.TestConnection())
                {
                    return Ok(new 
                    { 
                        status = "MongoDB connection is healthy", 
                        timestamp = DateTime.UtcNow,
                        database = databaseName
                    });
                }
                else
                {
                    _logger.LogError("MongoDB connection test failed.");
                    return StatusCode(StatusCodes.Status503ServiceUnavailable, 
                        new 
                        { 
                            status = "MongoDB connection failed",
                            message = "Unable to connect to MongoDB. Check connection settings and server availability."
                        });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing MongoDB connection");
                return StatusCode(StatusCodes.Status503ServiceUnavailable, 
                    new { status = "MongoDB connection error", error = ex.Message });
            }
        }
    }
}
