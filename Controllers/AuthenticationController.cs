using EncantoWebAPI.Models.Auth;
using Microsoft.AspNetCore.Mvc;

namespace EncantoWebAPI.Controllers
{
    [Route("")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly IConfiguration _config;

        public AuthenticationController(IConfiguration config)
        {
            _config = config;
        }

        [HttpPost("auth/signup")]
        public async Task<ActionResult> CreateProfile([FromBody] SignupRequest signupRequest)
        {
            var authenticationManager = new Managers.AuthenticationManager(_config);
            try
            {
                await authenticationManager.CreateNewUser(signupRequest);
                return Ok("Profile created successfully.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("auth/login")]
        public async Task<ActionResult> LoginUser([FromBody] LoginRequest loginRequest)
        {
            var authenticationManager = new Managers.AuthenticationManager(_config);
            try
            {
                var userId = await authenticationManager.LoginExistingUser(loginRequest);
                var sessionKey = authenticationManager.GenerateSessionKey(userId);

                await authenticationManager.StoreSessionKey(userId, sessionKey);

                return Ok(new { sessionKey });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("auth/logout")]
        public async Task<ActionResult> LogoutUser()
        {
            var authenticationManager = new Managers.AuthenticationManager(_config);

            // Retrieve session key from context (middleware)
            var sessionKey = HttpContext.Items["SessionKey"] as string;

            if (!string.IsNullOrEmpty(sessionKey))
            {
                try
                {
                    await authenticationManager.DeleteSessionKey(sessionKey);
                    return Ok("Logged out successfully.");
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
            }
            else
            {
                return BadRequest("Session key not found in headers.");
            }
        }

    }
}
