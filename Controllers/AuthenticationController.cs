using EncantoWebAPI.Managers;
using EncantoWebAPI.Models.Auth;
using Microsoft.AspNetCore.Mvc;

namespace EncantoWebAPI.Controllers
{
    [Route("")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly AuthenticationManager _authenticationManager;

        public AuthenticationController(AuthenticationManager authenticationManager)
        {
            _authenticationManager = authenticationManager;
        }

        [HttpPost("auth/signup")]
        public async Task<ActionResult> CreateProfile([FromBody] SignupRequest signupRequest)
        {
            try
            {
                await _authenticationManager.CreateNewUser(signupRequest);
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
            try
            {
                var userId = await _authenticationManager.LoginExistingUser(loginRequest);
                var sessionKey = _authenticationManager.GenerateSessionKey(userId);

                await _authenticationManager.StoreSessionKey(userId, sessionKey);

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
            // Retrieve session key from context (middleware)
            var sessionKey = HttpContext.Items["SessionKey"] as string;

            if (!string.IsNullOrEmpty(sessionKey))
            {
                try
                {
                    await _authenticationManager.DeleteSessionKey(sessionKey);
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
