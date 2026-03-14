using EncantoWebAPI.Accessors;
using Microsoft.AspNetCore.Mvc;

namespace EncantoWebAPI.Controllers
{
    [Route("")]
    [ApiController]
    public class UtilitiesController : ControllerBase
    {
        private readonly MongoDBAccessor _mongoDBAccessor;

        public UtilitiesController(MongoDBAccessor mongoDBAccessor)
        {
            _mongoDBAccessor = mongoDBAccessor;
        }

        [HttpGet("test-db-connection")]
        public ActionResult TestDatabase()
        {
            return _mongoDBAccessor.TestConnection() ? Ok("✅ Connected to MongoDB!") : StatusCode(500, "❌ MongoDB connection failed.");
        }

    }
}
