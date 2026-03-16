using EncantoWebAPI.Accessors;
using Microsoft.AspNetCore.Mvc;

namespace EncantoWebAPI.Controllers
{
    [Route("")]
    [ApiController]
    public class UtilitiesController : ControllerBase
    {
        private readonly IConfiguration _config;

        public UtilitiesController(IConfiguration config)
        {
            _config = config;
        }

        [HttpGet("test-db-connection")]
        public ActionResult TestDatabase()
        {
            var mongo = new MongoDBAccessor(_config);
            if (mongo.TestConnection())
            {
                return Ok($"✅ Connected to MongoDB!");
            }
            return StatusCode(500, "❌ MongoDB connection failed.");
        }

    }
}
