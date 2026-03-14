using EncantoWebAPI.Managers;
using EncantoWebAPI.Models.Profiles;
using EncantoWebAPI.Models.Profiles.Requests;
using Microsoft.AspNetCore.Mvc;

namespace EncantoWebAPI.Controllers
{
    [Route("")]
    [ApiController]
    public class UserDetailsController : ControllerBase
    {
        private readonly UserDetailsManager _userDetailsManager;

        public UserDetailsController(UserDetailsManager userDetailsManager)
        {
            _userDetailsManager = userDetailsManager;
        }

        [HttpGet("profile/info")]
        public async Task<ActionResult<UserProfile>> GetProfileDetails()
        {
            // Retrieve session key from context (middleware)
            var sessionKey = HttpContext.Items["SessionKey"] as string;

            try
            {
                if (sessionKey != null)
                {
                    var userIdFromSession = await _userDetailsManager.GetUserIdFromSessionDetails(sessionKey);
                    var profileDetails = await _userDetailsManager.GetProfileDetailsFromUserId(userIdFromSession);
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
            try
            {
                if (userNameUpdateRequest != null)
                {
                    await _userDetailsManager.UpdateProfileName(userNameUpdateRequest);
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
            try
            {
                if (userPhnUpdateRequest != null)
                {
                    await _userDetailsManager.UpdateProfilePhn(userPhnUpdateRequest);
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
            try
            {
                if (userGenderUpdateRequest != null)
                {
                    await _userDetailsManager.UpdateProfileGender(userGenderUpdateRequest);
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
            try
            {
                if (userBirthdayUpdateRequest != null)
                {
                    await _userDetailsManager.UpdateProfileBirthday(userBirthdayUpdateRequest);
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
            try
            {
                if (userAddressUpdateRequest != null)
                {
                    await _userDetailsManager.UpdateProfileAddress(userAddressUpdateRequest);
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
            try
            {
                if (userOccupationUpdateRequest != null)
                {
                    await _userDetailsManager.UpdateProfileOccupation(userOccupationUpdateRequest);
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