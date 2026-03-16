using EncantoWebAPI.Accessors;
using Microsoft.AspNetCore.Mvc;

namespace EncantoWebAPI.Controllers
{
    [Route("")]
    [ApiController]
    public class UtilitiesController : ControllerBase
    {

        [HttpGet("test-db-connection")]
        public ActionResult TestDatabase()
        {
            var mongo = new MongoDBAccessor();
            if (mongo.TestConnection())
            {
                var connectionInfo = mongo.GetConnectionInfo();
                return Ok($"✅ Connected to MongoDB!\n{connectionInfo}");
            }
            return StatusCode(500, "❌ MongoDB connection failed.");
        }

    }
}
