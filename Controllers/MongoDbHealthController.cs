using EncantoWebAPI.Services;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace EncantoWebAPI.Controllers
{
    /// <summary>
    /// Example controller demonstrating how to inject and use MongoDbService.
    /// This controller provides basic CRUD operations for any MongoDB collection.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class MongoDbHealthController : ControllerBase
    {
        private readonly MongoDbService _mongoDbService;
        private readonly ILogger<MongoDbHealthController> _logger;

        public MongoDbHealthController(MongoDbService mongoDbService, ILogger<MongoDbHealthController> logger)
        {
            _mongoDbService = mongoDbService;
            _logger = logger;
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
                bool isConnected = _mongoDbService.TestConnection();
                if (isConnected)
                {
                    return Ok(new { status = "MongoDB connection is healthy", timestamp = DateTime.UtcNow });
                }
                else
                {
                    _logger.LogError("MongoDB connection test failed");
                    return StatusCode(StatusCodes.Status503ServiceUnavailable, 
                        new { status = "MongoDB connection failed" });
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
