using EncantoWebAPI.Models.Profiles;
using EncantoWebAPI.Models.Profiles.Requests;
using Microsoft.AspNetCore.Mvc;

namespace EncantoWebAPI.Controllers
{
    [Route("")]
    [ApiController]
    public class UserDetailsController : ControllerBase
    {
        private readonly IConfiguration _config;

        public UserDetailsController(IConfiguration config)
        {
            _config = config;
        }

        [HttpGet("profile/info")]
        public async Task<ActionResult<UserProfile>> GetProfileDetails()
        {
            var userDetailsManager = new Managers.UserDetailsManager(_config);

            // Retrieve session key from context (middleware)
            var sessionKey = HttpContext.Items["SessionKey"] as string;

            try
            {
                if (sessionKey != null)
                {
                    var userIdFromSession = await userDetailsManager.GetUserIdFromSessionDetails(sessionKey);
                    var profileDetails = await userDetailsManager.GetProfileDetailsFromUserId(userIdFromSession);
                    return Ok(profileDetails); //returns profileDetails
                }
                else
                {
                    return BadRequest("Session key not found.");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        #region Update Profile Details

        [HttpPut("profile/update-user-name")]
        public async Task<ActionResult> UpdateProfileName([FromBody] UserNameUpdateRequest userNameUpdateRequest)
        {
            var userDetailsManager = new Managers.UserDetailsManager(_config);
            try
            {
                if (userNameUpdateRequest != null)
                {
                    await userDetailsManager.UpdateProfileName(userNameUpdateRequest);
                    return Ok();
                }
                else
                {
                    return BadRequest("Invaild User name Request");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);

            }
        }

        [HttpPut("profile/update-user-phone-number")]
        public async Task<ActionResult> UpdateProfilePhn([FromBody] UserPhnUpdateRequest userPhnUpdateRequest)
        {
            var userDetailsManager = new Managers.UserDetailsManager(_config);
            try
            {
                if (userPhnUpdateRequest != null)
                {
                    await userDetailsManager.UpdateProfilePhn(userPhnUpdateRequest);
                    return Ok();
                }
                else
                {
                    return BadRequest("Invaild User Phone number Request");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);

            }
        }

        [HttpPut("profile/update-user-gender")]
        public async Task<ActionResult> UpdateProfileGender([FromBody] UserGenderUpdateRequest userGenderUpdateRequest)
        {
            var userDetailsManager = new Managers.UserDetailsManager(_config);
            try
            {
                if (userGenderUpdateRequest != null)
                {
                    await userDetailsManager.UpdateProfileGender(userGenderUpdateRequest);
                    return Ok();
                }
                else
                {
                    return BadRequest("Invaild User gender Request");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);

            }
        }

        [HttpPut("profile/update-user-birthday")]
        public async Task<ActionResult> UpdateProfileBirthday([FromBody] UserBirthdayUpdateRequest userBirthdayUpdateRequest)
        {
            var userDetailsManager = new Managers.UserDetailsManager(_config);
            try
            {
                if (userBirthdayUpdateRequest != null)
                {
                    await userDetailsManager.UpdateProfileBirthday(userBirthdayUpdateRequest);
                    return Ok();
                }
                else
                {
                    return BadRequest("Invaild User birthday Request");
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);

            }
        }

        #endregion


        #region Update Address Details

        [HttpPut("profile/update-user-address")]
        public async Task<ActionResult> UpdateProfileAddress([FromBody] UserAddressUpdateRequest userAddressUpdateRequest)
        {
            var userDetailsManager = new Managers.UserDetailsManager(_config);
            try
            {
                if (userAddressUpdateRequest != null)
                {
                    await userDetailsManager.UpdateProfileAddress(userAddressUpdateRequest);
                    return Ok();
                }
                else
                {
                    return BadRequest("Invaild User address Request");
                }
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        #endregion

        #region Update Occupation Details

        [HttpPut("profile/update-user-occupation")]
        public async Task<ActionResult> UpdateProfileOccupation([FromBody] UserOccupationUpdateRequest userOccupationUpdateRequest)
        {
            var userDetailsManager = new Managers.UserDetailsManager(_config);
            try
            {
                if (userOccupationUpdateRequest != null)
                {
                    await userDetailsManager.UpdateProfileOccupation(userOccupationUpdateRequest);
                    return Ok();
                }
                else
                {
                    return BadRequest("Invaild User Occupation Request");
                }
            }
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        #endregion

    }
}